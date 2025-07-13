using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.WPF.Services
{
    /// <summary>
    /// Service for handling WebSocket connections and real-time updates
    /// </summary>
    public class WebSocketService : IDisposable
    {
        private readonly ILogger<WebSocketService> _logger;
        private readonly string _webSocketUrl;
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isConnected = false;
        private bool _disposed = false;

        public event EventHandler<SystemHealthStatus>? SystemHealthUpdated;
        public event EventHandler<AiUsageReport>? AiUsageUpdated;
        public event EventHandler<Notification>? NotificationReceived;
        public event EventHandler<string>? ConnectionStatusChanged;

        public bool IsConnected => _isConnected && _webSocket?.State == WebSocketState.Open;

        public WebSocketService(ILogger<WebSocketService> logger, string webSocketUrl = "ws://localhost:8000/ws")
        {
            _logger = logger;
            _webSocketUrl = webSocketUrl;
        }

        /// <summary>
        /// Connects to the WebSocket server
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (_webSocket != null)
                {
                    await DisconnectAsync();
                }

                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();

                _logger.LogInformation("Connecting to WebSocket server: {Url}", _webSocketUrl);
                
                await _webSocket.ConnectAsync(new Uri(_webSocketUrl), _cancellationTokenSource.Token);
                
                _isConnected = true;
                OnConnectionStatusChanged("Connected");
                
                _logger.LogInformation("WebSocket connected successfully");

                // Start listening for messages
                _ = Task.Run(() => ListenForMessagesAsync(_cancellationTokenSource.Token));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to WebSocket server");
                _isConnected = false;
                OnConnectionStatusChanged("Connection Failed");
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the WebSocket server
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (_webSocket != null && _webSocket.State == WebSocketState.Open)
                {
                    _logger.LogInformation("Disconnecting from WebSocket server");
                    
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", 
                        CancellationToken.None);
                }

                _isConnected = false;
                OnConnectionStatusChanged("Disconnected");
                
                _logger.LogInformation("WebSocket disconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from WebSocket server");
            }
            finally
            {
                _webSocket?.Dispose();
                _webSocket = null;
                _cancellationTokenSource?.Cancel();
            }
        }

        /// <summary>
        /// Sends a message to the WebSocket server
        /// </summary>
        public async Task<bool> SendMessageAsync(string message)
        {
            try
            {
                if (!IsConnected)
                {
                    _logger.LogWarning("Cannot send message: WebSocket is not connected");
                    return false;
                }

                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);

                await _webSocket!.SendAsync(segment, WebSocketMessageType.Text, true, 
                    _cancellationTokenSource!.Token);

                _logger.LogDebug("Message sent: {Message}", message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send WebSocket message");
                return false;
            }
        }

        /// <summary>
        /// Subscribes to specific update types
        /// </summary>
        public async Task SubscribeToUpdatesAsync(params string[] updateTypes)
        {
            try
            {
                var subscriptionMessage = new
                {
                    type = "subscribe",
                    updates = updateTypes
                };

                var json = JsonSerializer.Serialize(subscriptionMessage);
                await SendMessageAsync(json);

                _logger.LogInformation("Subscribed to updates: {UpdateTypes}", string.Join(", ", updateTypes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe to updates");
            }
        }

        /// <summary>
        /// Listens for incoming messages from the WebSocket server
        /// </summary>
        private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];

            try
            {
                while (_webSocket != null && _webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await ProcessMessageAsync(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket connection closed by server");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("WebSocket listening cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listening for WebSocket messages");
            }
            finally
            {
                _isConnected = false;
                OnConnectionStatusChanged("Disconnected");
            }
        }

        /// <summary>
        /// Processes incoming messages and raises appropriate events
        /// </summary>
        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                _logger.LogDebug("Received WebSocket message: {Message}", message);

                var document = JsonDocument.Parse(message);
                var root = document.RootElement;

                if (root.TryGetProperty("type", out var typeElement))
                {
                    var messageType = typeElement.GetString();

                    switch (messageType?.ToLower())
                    {
                        case "system_health":
                            if (root.TryGetProperty("data", out var healthData))
                            {
                                var healthStatus = JsonSerializer.Deserialize<SystemHealthStatus>(healthData.GetRawText());
                                if (healthStatus != null)
                                {
                                    SystemHealthUpdated?.Invoke(this, healthStatus);
                                }
                            }
                            break;

                        case "ai_usage":
                            if (root.TryGetProperty("data", out var usageData))
                            {
                                var usageReport = JsonSerializer.Deserialize<AiUsageReport>(usageData.GetRawText());
                                if (usageReport != null)
                                {
                                    AiUsageUpdated?.Invoke(this, usageReport);
                                }
                            }
                            break;

                        case "notification":
                            if (root.TryGetProperty("data", out var notificationData))
                            {
                                var notification = JsonSerializer.Deserialize<Notification>(notificationData.GetRawText());
                                if (notification != null)
                                {
                                    NotificationReceived?.Invoke(this, notification);
                                }
                            }
                            break;

                        case "ping":
                            // Respond to ping with pong
                            await SendMessageAsync(JsonSerializer.Serialize(new { type = "pong" }));
                            break;

                        default:
                            _logger.LogWarning("Unknown message type: {MessageType}", messageType);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WebSocket message");
            }
        }

        /// <summary>
        /// Raises the connection status changed event
        /// </summary>
        protected virtual void OnConnectionStatusChanged(string status)
        {
            ConnectionStatusChanged?.Invoke(this, status);
        }

        /// <summary>
        /// Attempts to reconnect to the WebSocket server
        /// </summary>
        public async Task<bool> ReconnectAsync()
        {
            _logger.LogInformation("Attempting to reconnect to WebSocket server");
            
            await DisconnectAsync();
            await Task.Delay(1000); // Wait 1 second before reconnecting
            
            return await ConnectAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _cancellationTokenSource?.Cancel();
                _webSocket?.Dispose();
                _cancellationTokenSource?.Dispose();
                _disposed = true;
            }
        }
    }
} 