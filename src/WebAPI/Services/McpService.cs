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
        return await CallMcpToolAsync(endpoint, toolName, parameters, accessToken);
    }

    public async Task<string> CallCrmToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null)
    {
        var endpoint = _configuration["Mcp:CrmEndpoint"];
        return await CallMcpToolAsync(endpoint, toolName, parameters, accessToken);
    }

    public async Task<string> CallServiceDeskToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null)
    {
        var endpoint = _configuration["Mcp:ServiceDeskEndpoint"];
        return await CallMcpToolAsync(endpoint, toolName, parameters, accessToken);
    }

    private async Task<string> CallMcpToolAsync(
        string? endpointUrl,
        string toolName,
        Dictionary<string, object> parameters,
        string? accessToken)
    {
        if (string.IsNullOrEmpty(endpointUrl))
        {
            _logger.LogWarning("MCP endpoint URL is not configured");
            return "MCP endpoint not configured";
        }

        try
        {
            // Create HTTP client with SSE transport configuration
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(endpointUrl);
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            
            // Add Bearer token for authentication
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }

            // Add headers for SSE
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
            httpClient.DefaultRequestHeaders.CacheControl = 
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _logger.LogInformation("Calling MCP endpoint {Endpoint} with tool {ToolName} using SSE transport", 
                endpointUrl, toolName);

            // Create MCP request using JSON-RPC 2.0 format
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

            // Make the request
            var response = await httpClient.PostAsync(string.Empty, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("MCP call failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return $"MCP call failed: {response.StatusCode} - {errorContent}";
            }

            var result = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("MCP call successful for tool {ToolName}", toolName);
            
            // Parse the JSON-RPC response
            try
            {
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result);
                if (jsonResponse.TryGetProperty("result", out var resultElement))
                {
                    if (resultElement.TryGetProperty("content", out var contentElement))
                    {
                        return contentElement.ToString();
                    }
                    return JsonSerializer.Serialize(resultElement, new JsonSerializerOptions { WriteIndented = true });
                }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MCP endpoint {Endpoint} with tool {ToolName}", endpointUrl, toolName);
            return $"Error calling MCP: {ex.Message}";
        }
    }
}

