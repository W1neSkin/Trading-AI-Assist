#!/usr/bin/env python3
"""
Azure Infrastructure Deployment for Trading AI Assist

This script deploys the Azure infrastructure using ARM templates and Bicep files.
It manages the deployment of all Azure resources required for the Trading AI Assist platform.
"""

import json
import os
import subprocess
import sys
from typing import Dict, Any
from pathlib import Path

class AzureInfrastructureDeployer:
    """Manages Azure infrastructure deployment using ARM templates and Bicep."""
    
    def __init__(self, subscription_id: str, resource_group: str, location: str):
        self.subscription_id = subscription_id
        self.resource_group = resource_group
        self.location = location
        self.template_dir = Path(__file__).parent / "templates"
        
    def validate_azure_cli(self) -> bool:
        """Validate that Azure CLI is installed and authenticated."""
        try:
            result = subprocess.run(
                ["az", "account", "show"],
                capture_output=True,
                text=True,
                check=True
            )
            account_info = json.loads(result.stdout)
            print(f"✅ Authenticated as: {account_info.get('user', {}).get('name', 'Unknown')}")
            return True
        except (subprocess.CalledProcessError, FileNotFoundError):
            print("❌ Azure CLI not found or not authenticated")
            print("Please install Azure CLI and run 'az login'")
            return False
    
    def create_resource_group(self) -> bool:
        """Create the Azure resource group if it doesn't exist."""
        try:
            subprocess.run([
                "az", "group", "create",
                "--name", self.resource_group,
                "--location", self.location,
                "--tags", "project=trading-ai-assist", "environment=production"
            ], check=True)
            print(f"✅ Resource group '{self.resource_group}' created/verified")
            return True
        except subprocess.CalledProcessError as e:
            print(f"❌ Failed to create resource group: {e}")
            return False
    
    def deploy_network_stack(self) -> bool:
        """Deploy the networking infrastructure."""
        try:
            template_file = self.template_dir / "network.bicep"
            parameters_file = self.template_dir / "network.parameters.json"
            
            subprocess.run([
                "az", "deployment", "group", "create",
                "--resource-group", self.resource_group,
                "--template-file", str(template_file),
                "--parameters", f"@{str(parameters_file)}",
                "--parameters", f"location={self.location}",
                "--verbose"
            ], check=True)
            print("✅ Network infrastructure deployed successfully")
            return True
        except subprocess.CalledProcessError as e:
            print(f"❌ Failed to deploy network stack: {e}")
            return False
    
    def deploy_database_stack(self) -> bool:
        """Deploy the database infrastructure."""
        try:
            template_file = self.template_dir / "database.bicep"
            parameters_file = self.template_dir / "database.parameters.json"
            
            subprocess.run([
                "az", "deployment", "group", "create",
                "--resource-group", self.resource_group,
                "--template-file", str(template_file),
                "--parameters", f"@{str(parameters_file)}",
                "--parameters", f"location={self.location}",
                "--verbose"
            ], check=True)
            print("✅ Database infrastructure deployed successfully")
            return True
        except subprocess.CalledProcessError as e:
            print(f"❌ Failed to deploy database stack: {e}")
            return False
    
    def deploy_messaging_stack(self) -> bool:
        """Deploy the messaging infrastructure (Service Bus, Event Grid)."""
        try:
            template_file = self.template_dir / "messaging.bicep"
            parameters_file = self.template_dir / "messaging.parameters.json"
            
            subprocess.run([
                "az", "deployment", "group", "create",
                "--resource-group", self.resource_group,
                "--template-file", str(template_file),
                "--parameters", f"@{str(parameters_file)}",
                "--parameters", f"location={self.location}",
                "--verbose"
            ], check=True)
            print("✅ Messaging infrastructure deployed successfully")
            return True
        except subprocess.CalledProcessError as e:
            print(f"❌ Failed to deploy messaging stack: {e}")
            return False
    
    def deploy_app_services_stack(self) -> bool:
        """Deploy the application services infrastructure."""
        try:
            template_file = self.template_dir / "app-services.bicep"
            parameters_file = self.template_dir / "app-services.parameters.json"
            
            subprocess.run([
                "az", "deployment", "group", "create",
                "--resource-group", self.resource_group,
                "--template-file", str(template_file),
                "--parameters", f"@{str(parameters_file)}",
                "--parameters", f"location={self.location}",
                "--verbose"
            ], check=True)
            print("✅ Application services infrastructure deployed successfully")
            return True
        except subprocess.CalledProcessError as e:
            print(f"❌ Failed to deploy app services stack: {e}")
            return False
    
    def deploy_security_stack(self) -> bool:
        """Deploy the security infrastructure (Key Vault, AD integration)."""
        try:
            template_file = self.template_dir / "security.bicep"
            parameters_file = self.template_dir / "security.parameters.json"
            
            subprocess.run([
                "az", "deployment", "group", "create",
                "--resource-group", self.resource_group,
                "--template-file", str(template_file),
                "--parameters", f"@{str(parameters_file)}",
                "--parameters", f"location={self.location}",
                "--verbose"
            ], check=True)
            print("✅ Security infrastructure deployed successfully")
            return True
        except subprocess.CalledProcessError as e:
            print(f"❌ Failed to deploy security stack: {e}")
            return False
    
    def deploy_monitoring_stack(self) -> bool:
        """Deploy the monitoring and logging infrastructure."""
        try:
            template_file = self.template_dir / "monitoring.bicep"
            parameters_file = self.template_dir / "monitoring.parameters.json"
            
            subprocess.run([
                "az", "deployment", "group", "create",
                "--resource-group", self.resource_group,
                "--template-file", str(template_file),
                "--parameters", f"@{str(parameters_file)}",
                "--parameters", f"location={self.location}",
                "--verbose"
            ], check=True)
            print("✅ Monitoring infrastructure deployed successfully")
            return True
        except subprocess.CalledProcessError as e:
            print(f"❌ Failed to deploy monitoring stack: {e}")
            return False
    
    def deploy_all(self) -> bool:
        """Deploy all infrastructure components in the correct order."""
        print("🚀 Starting Azure infrastructure deployment...")
        
        # Validate prerequisites
        if not self.validate_azure_cli():
            return False
        
        # Create resource group
        if not self.create_resource_group():
            return False
        
        # Deploy infrastructure in dependency order
        deployment_steps = [
            ("Network", self.deploy_network_stack),
            ("Security", self.deploy_security_stack),
            ("Database", self.deploy_database_stack),
            ("Messaging", self.deploy_messaging_stack),
            ("App Services", self.deploy_app_services_stack),
            ("Monitoring", self.deploy_monitoring_stack),
        ]
        
        for step_name, deploy_func in deployment_steps:
            print(f"\n📦 Deploying {step_name} stack...")
            if not deploy_func():
                print(f"❌ Deployment failed at {step_name} step")
                return False
        
        print("\n🎉 All infrastructure deployed successfully!")
        return True
    
    def get_outputs(self) -> Dict[str, Any]:
        """Get deployment outputs for configuration."""
        try:
            result = subprocess.run([
                "az", "deployment", "group", "show",
                "--resource-group", self.resource_group,
                "--name", "app-services-deployment"
            ], capture_output=True, text=True, check=True)
            
            deployment_info = json.loads(result.stdout)
            return deployment_info.get("properties", {}).get("outputs", {})
        except subprocess.CalledProcessError:
            print("⚠️ Could not retrieve deployment outputs")
            return {}

def main():
    """Main deployment function."""
    # Configuration
    subscription_id = os.getenv("AZURE_SUBSCRIPTION_ID")
    resource_group = os.getenv("AZURE_RESOURCE_GROUP", "trading-ai-assist-rg")
    location = os.getenv("AZURE_LOCATION", "East US")
    
    if not subscription_id:
        print("❌ AZURE_SUBSCRIPTION_ID environment variable is required")
        sys.exit(1)
    
    # Initialize deployer
    deployer = AzureInfrastructureDeployer(subscription_id, resource_group, location)
    
    # Deploy infrastructure
    if deployer.deploy_all():
        print("\n📋 Deployment Summary:")
        print(f"   Resource Group: {resource_group}")
        print(f"   Location: {location}")
        print(f"   Subscription: {subscription_id}")
        
        # Get and display outputs
        outputs = deployer.get_outputs()
        if outputs:
            print("\n🔧 Configuration Outputs:")
            for key, value in outputs.items():
                print(f"   {key}: {value.get('value', 'N/A')}")
        
        print("\n✅ Infrastructure deployment completed successfully!")
        print("📖 Next steps:")
        print("   1. Configure application settings with the outputs above")
        print("   2. Deploy application containers to App Service")
        print("   3. Configure Azure AD for authentication")
        print("   4. Set up monitoring and alerting")
    else:
        print("\n❌ Infrastructure deployment failed!")
        sys.exit(1)

if __name__ == "__main__":
    main() 