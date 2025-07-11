"""
AI Service - Hybrid LLM Architecture with RAG and NL2SQL
"""
import asyncio
from contextlib import asynccontextmanager
from typing import Dict, Any, List, Optional
import logging
import time

from fastapi import FastAPI, HTTPException, BackgroundTasks, Depends
from pydantic import BaseModel, Field
import httpx
from langchain.chains import ConversationalRetrievalChain, LLMChain
from langchain.memory import ConversationBufferMemory
from langchain.prompts import PromptTemplate
from langchain.schema import BaseRetriever
from langchain.vectorstores import FAISS
from langchain.embeddings import HuggingFaceEmbeddings
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain.docstore.document import Document
import tensorflow as tf
import numpy as np
import structlog

# Import shared modules
import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..', '..'))

from shared.config import settings
from shared.database import get_postgres_session, mongodb_manager
from shared.messaging import messaging_manager

# Configure logging
logger = structlog.get_logger()


class LLMProvider(BaseModel):
    """LLM Provider configuration"""
    provider: str = Field(..., description="Provider name (ollama or openroute)")
    model: str = Field(..., description="Model name")
    temperature: float = Field(default=0.7, ge=0.0, le=2.0)
    max_tokens: int = Field(default=1000, gt=0)


class ChatRequest(BaseModel):
    """Chat request model"""
    message: str = Field(..., description="User message")
    context: Optional[str] = Field(None, description="Additional context")
    provider: Optional[str] = Field("ollama", description="LLM provider")
    use_rag: bool = Field(default=True, description="Use RAG for enhanced responses")


class SQLGenerationRequest(BaseModel):
    """SQL generation request"""
    natural_language_query: str = Field(..., description="Natural language query")
    schema_context: Optional[str] = Field(None, description="Database schema context")
    table_names: Optional[List[str]] = Field(None, description="Relevant table names")


class DocumentProcessingRequest(BaseModel):
    """Document processing for RAG"""
    document_text: str = Field(..., description="Document text to process")
    document_id: str = Field(..., description="Document identifier")
    metadata: Optional[Dict[str, Any]] = Field(default_factory=dict)


class OllamaClient:
    """Ollama client for local LLM"""
    
    def __init__(self, base_url: str = "http://localhost:11434"):
        self.base_url = base_url
        self.client = httpx.AsyncClient(timeout=60.0)
    
    async def generate(self, model: str, prompt: str, **kwargs) -> str:
        """Generate response using Ollama"""
        try:
            response = await self.client.post(
                f"{self.base_url}/api/generate",
                json={
                    "model": model,
                    "prompt": prompt,
                    "stream": False,
                    **kwargs
                }
            )
            response.raise_for_status()
            result = response.json()
            return result.get("response", "")
            
        except Exception as e:
            logger.error(f"Ollama generation failed: {e}")
            raise HTTPException(status_code=500, detail=f"Ollama error: {str(e)}")
    
    async def list_models(self) -> List[str]:
        """List available models"""
        try:
            response = await self.client.get(f"{self.base_url}/api/tags")
            response.raise_for_status()
            result = response.json()
            return [model["name"] for model in result.get("models", [])]
            
        except Exception as e:
            logger.error(f"Failed to list Ollama models: {e}")
            return []


class OpenRouteClient:
    """OpenRoute client for cloud LLM"""
    
    def __init__(self, api_key: str):
        self.api_key = api_key
        self.client = httpx.AsyncClient(timeout=60.0)
        self.base_url = "https://openrouter.ai/api/v1"
    
    async def generate(self, model: str, prompt: str, **kwargs) -> str:
        """Generate response using OpenRoute"""
        try:
            headers = {
                "Authorization": f"Bearer {self.api_key}",
                "HTTP-Referer": "https://ai-trading-platform.com",
                "X-Title": "AI Trading Platform"
            }
            
            response = await self.client.post(
                f"{self.base_url}/chat/completions",
                headers=headers,
                json={
                    "model": model,
                    "messages": [{"role": "user", "content": prompt}],
                    "temperature": kwargs.get("temperature", 0.7),
                    "max_tokens": kwargs.get("max_tokens", 1000)
                }
            )
            response.raise_for_status()
            result = response.json()
            
            return result["choices"][0]["message"]["content"]
            
        except Exception as e:
            logger.error(f"OpenRoute generation failed: {e}")
            raise HTTPException(status_code=500, detail=f"OpenRoute error: {str(e)}")


class HybridLLMManager:
    """Hybrid LLM manager for switching between local and cloud"""
    
    def __init__(self):
        self.ollama_client = OllamaClient(settings.ollama_url)
        self.openroute_client = None
        if settings.openroute_api_key:
            self.openroute_client = OpenRouteClient(settings.openroute_api_key)
    
    async def generate_response(self, prompt: str, provider: str = "ollama", model: str = None, **kwargs) -> str:
        """Generate response using specified provider"""
        if provider == "ollama":
            if not model:
                model = "llama2"  # Default model
            return await self.ollama_client.generate(model, prompt, **kwargs)
        
        elif provider == "openroute":
            if not self.openroute_client:
                raise HTTPException(status_code=400, detail="OpenRoute not configured")
            if not model:
                model = "mistralai/mistral-7b-instruct"  # Default model
            return await self.openroute_client.generate(model, prompt, **kwargs)
        
        else:
            raise HTTPException(status_code=400, detail=f"Unknown provider: {provider}")


class RAGSystem:
    """Retrieval-Augmented Generation system"""
    
    def __init__(self):
        self.embeddings = HuggingFaceEmbeddings(
            model_name="sentence-transformers/all-MiniLM-L6-v2"
        )
        self.text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=1000,
            chunk_overlap=200
        )
        self.vector_store = None
        self.retriever = None
    
    async def initialize(self):
        """Initialize RAG system"""
        try:
            # Load existing vector store or create new one
            await self._load_or_create_vector_store()
            logger.info("RAG system initialized")
        except Exception as e:
            logger.error(f"Failed to initialize RAG system: {e}")
            raise
    
    async def _load_or_create_vector_store(self):
        """Load existing vector store or create new one"""
        try:
            # Try to load from MongoDB
            collection = mongodb_manager.get_collection("embeddings")
            documents = await collection.find({}).to_list(length=None)
            
            if documents:
                # Convert MongoDB documents to LangChain documents
                langchain_docs = []
                for doc in documents:
                    langchain_docs.append(Document(
                        page_content=doc["content"],
                        metadata=doc.get("metadata", {})
                    ))
                
                # Create vector store
                self.vector_store = FAISS.from_documents(langchain_docs, self.embeddings)
                self.retriever = self.vector_store.as_retriever(search_kwargs={"k": 5})
                
                logger.info(f"Loaded {len(documents)} documents into vector store")
            else:
                # Create empty vector store
                self.vector_store = FAISS.from_texts(["placeholder"], self.embeddings)
                self.retriever = self.vector_store.as_retriever(search_kwargs={"k": 5})
                
        except Exception as e:
            logger.error(f"Failed to load vector store: {e}")
            # Create empty vector store as fallback
            self.vector_store = FAISS.from_texts(["placeholder"], self.embeddings)
            self.retriever = self.vector_store.as_retriever(search_kwargs={"k": 5})
    
    async def add_documents(self, documents: List[Document]) -> bool:
        """Add documents to the vector store"""
        try:
            if not self.vector_store:
                await self.initialize()
            
            # Add to vector store
            self.vector_store.add_documents(documents)
            
            # Store in MongoDB for persistence
            collection = mongodb_manager.get_collection("embeddings")
            
            docs_to_insert = []
            for doc in documents:
                docs_to_insert.append({
                    "content": doc.page_content,
                    "metadata": doc.metadata,
                    "created_at": time.time()
                })
            
            await collection.insert_many(docs_to_insert)
            
            logger.info(f"Added {len(documents)} documents to RAG system")
            return True
            
        except Exception as e:
            logger.error(f"Failed to add documents: {e}")
            return False
    
    async def retrieve_context(self, query: str) -> str:
        """Retrieve relevant context for query"""
        try:
            if not self.retriever:
                return ""
            
            docs = await asyncio.to_thread(self.retriever.get_relevant_documents, query)
            context = "\n\n".join([doc.page_content for doc in docs])
            return context
            
        except Exception as e:
            logger.error(f"Failed to retrieve context: {e}")
            return ""


class NL2SQLGenerator:
    """Natural Language to SQL generator"""
    
    def __init__(self, llm_manager: HybridLLMManager):
        self.llm_manager = llm_manager
        self.sql_prompt = PromptTemplate(
            input_variables=["schema", "question"],
            template="""
You are a SQL expert. Given the following database schema and natural language question, 
generate a precise SQL query.

Database Schema:
{schema}

Question: {question}

Important Rules:
1. Only return the SQL query, no explanations
2. Use proper SQL syntax for PostgreSQL
3. Include proper JOINs when needed
4. Use appropriate WHERE clauses
5. Be mindful of data types
6. Return only SELECT statements for safety

SQL Query:
"""
        )
    
    async def generate_sql(self, natural_query: str, schema_context: str = None, table_names: List[str] = None) -> str:
        """Generate SQL from natural language"""
        try:
            # Get database schema if not provided
            if not schema_context:
                schema_context = await self._get_database_schema(table_names)
            
            # Create prompt
            prompt = self.sql_prompt.format(
                schema=schema_context,
                question=natural_query
            )
            
            # Generate SQL using LLM
            sql_query = await self.llm_manager.generate_response(
                prompt=prompt,
                provider="ollama",  # Use local model for SQL generation
                temperature=0.1  # Low temperature for more deterministic output
            )
            
            # Clean up the response
            sql_query = sql_query.strip()
            if sql_query.startswith("```sql"):
                sql_query = sql_query[6:]
            if sql_query.endswith("```"):
                sql_query = sql_query[:-3]
            
            return sql_query.strip()
            
        except Exception as e:
            logger.error(f"Failed to generate SQL: {e}")
            raise HTTPException(status_code=500, detail=f"SQL generation failed: {str(e)}")
    
    async def _get_database_schema(self, table_names: List[str] = None) -> str:
        """Get database schema information"""
        try:
            async with get_postgres_session() as session:
                if table_names:
                    # Get specific tables
                    table_filter = "AND table_name IN ({})".format(
                        ",".join([f"'{t}'" for t in table_names])
                    )
                else:
                    table_filter = ""
                
                query = f"""
                SELECT 
                    table_name,
                    column_name,
                    data_type,
                    is_nullable,
                    column_default
                FROM information_schema.columns 
                WHERE table_schema = 'public'
                {table_filter}
                ORDER BY table_name, ordinal_position;
                """
                
                result = await session.execute(query)
                rows = result.fetchall()
                
                # Format schema information
                schema_info = {}
                for row in rows:
                    table_name = row[0]
                    if table_name not in schema_info:
                        schema_info[table_name] = []
                    
                    schema_info[table_name].append({
                        "column": row[1],
                        "type": row[2],
                        "nullable": row[3],
                        "default": row[4]
                    })
                
                # Format as text
                schema_text = ""
                for table, columns in schema_info.items():
                    schema_text += f"\nTable: {table}\n"
                    for col in columns:
                        schema_text += f"  - {col['column']}: {col['type']}"
                        if col['nullable'] == 'NO':
                            schema_text += " NOT NULL"
                        if col['default']:
                            schema_text += f" DEFAULT {col['default']}"
                        schema_text += "\n"
                
                return schema_text
                
        except Exception as e:
            logger.error(f"Failed to get database schema: {e}")
            return "Schema information unavailable"


class PerformanceMonitor:
    """ML Performance monitoring"""
    
    def __init__(self):
        self.model_metrics = {}
        self.request_metrics = []
    
    def log_request(self, provider: str, model: str, response_time: float, success: bool):
        """Log request metrics"""
        self.request_metrics.append({
            "provider": provider,
            "model": model,
            "response_time": response_time,
            "success": success,
            "timestamp": time.time()
        })
        
        # Keep only last 1000 requests
        if len(self.request_metrics) > 1000:
            self.request_metrics = self.request_metrics[-1000:]
    
    def get_metrics(self) -> Dict[str, Any]:
        """Get performance metrics"""
        if not self.request_metrics:
            return {}
        
        # Calculate metrics
        total_requests = len(self.request_metrics)
        successful_requests = sum(1 for r in self.request_metrics if r["success"])
        success_rate = successful_requests / total_requests if total_requests > 0 else 0
        
        response_times = [r["response_time"] for r in self.request_metrics if r["success"]]
        avg_response_time = sum(response_times) / len(response_times) if response_times else 0
        
        return {
            "total_requests": total_requests,
            "success_rate": success_rate,
            "average_response_time": avg_response_time,
            "provider_breakdown": self._get_provider_breakdown()
        }
    
    def _get_provider_breakdown(self) -> Dict[str, Dict[str, Any]]:
        """Get breakdown by provider"""
        breakdown = {}
        for request in self.request_metrics:
            provider = request["provider"]
            if provider not in breakdown:
                breakdown[provider] = {"requests": 0, "successes": 0, "total_time": 0}
            
            breakdown[provider]["requests"] += 1
            if request["success"]:
                breakdown[provider]["successes"] += 1
                breakdown[provider]["total_time"] += request["response_time"]
        
        # Calculate averages
        for provider, data in breakdown.items():
            if data["successes"] > 0:
                data["avg_response_time"] = data["total_time"] / data["successes"]
                data["success_rate"] = data["successes"] / data["requests"]
            else:
                data["avg_response_time"] = 0
                data["success_rate"] = 0
        
        return breakdown


# Initialize components
llm_manager = HybridLLMManager()
rag_system = RAGSystem()
nl2sql_generator = NL2SQLGenerator(llm_manager)
performance_monitor = PerformanceMonitor()


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager"""
    # Startup
    logger.info("Starting AI Service")
    
    try:
        # Initialize MongoDB manager
        await mongodb_manager.initialize()
        
        # Initialize RAG system
        await rag_system.initialize()
        
        # Initialize messaging
        await messaging_manager.initialize()
        
        logger.info("AI Service startup completed")
        
        yield
        
    finally:
        # Shutdown
        logger.info("Shutting down AI Service")


# Create FastAPI application
app = FastAPI(
    title="AI Service",
    description="Hybrid LLM with RAG and NL2SQL capabilities",
    version="1.0.0",
    lifespan=lifespan
)


@app.post("/chat")
async def chat(request: ChatRequest) -> Dict[str, Any]:
    """Chat endpoint with RAG support"""
    start_time = time.time()
    success = False
    
    try:
        context = ""
        if request.use_rag:
            # Retrieve relevant context
            context = await rag_system.retrieve_context(request.message)
        
        # Add user context if provided
        if request.context:
            context += f"\n\nAdditional Context: {request.context}"
        
        # Create enhanced prompt
        enhanced_prompt = request.message
        if context:
            enhanced_prompt = f"Context: {context}\n\nQuestion: {request.message}"
        
        # Generate response
        response = await llm_manager.generate_response(
            prompt=enhanced_prompt,
            provider=request.provider
        )
        
        success = True
        response_data = {
            "response": response,
            "provider": request.provider,
            "used_rag": request.use_rag and bool(context),
            "context_length": len(context) if context else 0
        }
        
        return response_data
        
    except Exception as e:
        logger.error(f"Chat request failed: {e}")
        raise HTTPException(status_code=500, detail=str(e))
        
    finally:
        response_time = time.time() - start_time
        performance_monitor.log_request(
            provider=request.provider,
            model="default",
            response_time=response_time,
            success=success
        )


@app.post("/generate-sql")
async def generate_sql(request: SQLGenerationRequest) -> Dict[str, Any]:
    """Generate SQL from natural language"""
    try:
        sql_query = await nl2sql_generator.generate_sql(
            natural_query=request.natural_language_query,
            schema_context=request.schema_context,
            table_names=request.table_names
        )
        
        return {
            "sql_query": sql_query,
            "natural_query": request.natural_language_query,
            "generated_at": time.time()
        }
        
    except Exception as e:
        logger.error(f"SQL generation failed: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/process-document")
async def process_document(request: DocumentProcessingRequest, background_tasks: BackgroundTasks) -> Dict[str, Any]:
    """Process document for RAG system"""
    try:
        # Split document into chunks
        texts = rag_system.text_splitter.split_text(request.document_text)
        
        # Create Document objects
        documents = []
        for i, text in enumerate(texts):
            metadata = request.metadata.copy()
            metadata.update({
                "document_id": request.document_id,
                "chunk_index": i,
                "chunk_count": len(texts)
            })
            documents.append(Document(page_content=text, metadata=metadata))
        
        # Add to RAG system in background
        background_tasks.add_task(rag_system.add_documents, documents)
        
        return {
            "document_id": request.document_id,
            "chunks_created": len(texts),
            "status": "processing"
        }
        
    except Exception as e:
        logger.error(f"Document processing failed: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/models")
async def list_models() -> Dict[str, Any]:
    """List available models"""
    try:
        ollama_models = await llm_manager.ollama_client.list_models()
        
        return {
            "ollama_models": ollama_models,
            "openroute_available": llm_manager.openroute_client is not None,
            "default_models": {
                "ollama": "llama2",
                "openroute": "mistralai/mistral-7b-instruct"
            }
        }
        
    except Exception as e:
        logger.error(f"Failed to list models: {e}")
        return {"error": str(e)}


@app.get("/metrics")
async def get_metrics() -> Dict[str, Any]:
    """Get AI service metrics"""
    return {
        "performance": performance_monitor.get_metrics(),
        "rag_status": {
            "initialized": rag_system.vector_store is not None,
            "retriever_available": rag_system.retriever is not None
        },
        "llm_providers": {
            "ollama_available": True,
            "openroute_available": llm_manager.openroute_client is not None
        }
    }


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "timestamp": time.time(),
        "services": {
            "ollama": "checking",
            "rag": "available" if rag_system.vector_store else "unavailable",
            "mongodb": "available" if mongodb_manager.database else "unavailable"
        }
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8003) 