using System.Text.Json;
using ModelContextProtocol;

namespace WebAPI.Services;

public interface IMcpService
{
    Task<string> CallCmdbToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null);
    Task<string> CallCrmToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null);
    Task<string> CallServiceDeskToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null);
}

public class McpService : IMcpService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<McpService> _logger;

    public McpService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<McpService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> CallCmdbToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null)
    {
        var endpoint = _configuration["Mcp:CmdbEndpoint"];
        return await CallMcpToolAsync("cmdb", endpoint, toolName, parameters, accessToken);
    }

    public async Task<string> CallCrmToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null)
    {
        var endpoint = _configuration["Mcp:CrmEndpoint"];
        return await CallMcpToolAsync("crm", endpoint, toolName, parameters, accessToken);
    }

    public async Task<string> CallServiceDeskToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null)
    {
        var endpoint = _configuration["Mcp:ServiceDeskEndpoint"];
        return await CallMcpToolAsync("servicedesk", endpoint, toolName, parameters, accessToken);
    }

    private async Task<string> CallMcpToolAsync(
        string clientKey,
        string? endpointUrl,
        string toolName,
        Dictionary<string, object> parameters,
        string? accessToken)
    {
        if (string.IsNullOrEmpty(endpointUrl))
        {
            _logger.LogWarning("MCP endpoint URL is not configured for {ClientKey}", clientKey);
            return "MCP endpoint not configured";
        }

        try
        {
            // Create HTTP client with SSE transport configuration for MCP
            var httpClient = _httpClientFactory.CreateClient(clientKey);
            httpClient.BaseAddress = new Uri(endpointUrl);
            httpClient.Timeout = TimeSpan.FromMinutes(2);
            
            // Add Bearer token for authentication
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }

            // Configure headers for Server-Sent Events (SSE) transport
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
            httpClient.DefaultRequestHeaders.CacheControl = 
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _logger.LogInformation("Calling MCP tool {ToolName} on {ClientKey} using SSE transport", toolName, clientKey);

            // Create MCP tool call request using JSON-RPC 2.0 format
            // This follows the Model Context Protocol specification
            var request = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString(),
                method = "tools/call",
                @params = new
                {
                    name = toolName,
                    arguments = parameters
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Make the HTTP POST request to the MCP server
            var response = await httpClient.PostAsync(string.Empty, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("MCP call failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return $"MCP call failed: {response.StatusCode} - {errorContent}";
            }

            var result = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("MCP tool {ToolName} call successful", toolName);
            
            // Parse the JSON-RPC 2.0 response
            try
            {
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result);
                
                // Check for result field (success case)
                if (jsonResponse.TryGetProperty("result", out var resultElement))
                {
                    // Check if result has content field
                    if (resultElement.TryGetProperty("content", out var contentElement))
                    {
                        return contentElement.ToString();
                    }
                    // Return formatted result
                    return JsonSerializer.Serialize(resultElement, new JsonSerializerOptions { WriteIndented = true });
                }
                // Check for error field (error case)
                else if (jsonResponse.TryGetProperty("error", out var errorElement))
                {
                    _logger.LogError("MCP returned error: {Error}", errorElement.ToString());
                    return $"MCP Error: {errorElement.ToString()}";
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse MCP response as JSON, returning raw response");
            }

            return result;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("MCP tool call {ToolName} timed out", toolName);
            return "MCP call timed out";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MCP tool {ToolName} on {ClientKey}", toolName, clientKey);
            return $"Error calling MCP tool: {ex.Message}";
        }
    }
}

