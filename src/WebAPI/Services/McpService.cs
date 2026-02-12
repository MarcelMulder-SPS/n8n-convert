using System.Text.Json;

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
        return await CallMcpEndpointAsync(endpoint, toolName, parameters, accessToken);
    }

    public async Task<string> CallCrmToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null)
    {
        var endpoint = _configuration["Mcp:CrmEndpoint"];
        return await CallMcpEndpointAsync(endpoint, toolName, parameters, accessToken);
    }

    public async Task<string> CallServiceDeskToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null)
    {
        var endpoint = _configuration["Mcp:ServiceDeskEndpoint"];
        return await CallMcpEndpointAsync(endpoint, toolName, parameters, accessToken);
    }

    private async Task<string> CallMcpEndpointAsync(
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
            var httpClient = _httpClientFactory.CreateClient();
            
            // Add Bearer token if available
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }

            // Create MCP tool call request
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

            _logger.LogInformation("Calling MCP endpoint {Endpoint} with tool {ToolName}", endpointUrl, toolName);

            var response = await httpClient.PostAsync(endpointUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("MCP call failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return $"MCP call failed: {response.StatusCode}";
            }

            var result = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("MCP call successful");
            
            // Parse the JSON-RPC response
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result);
            if (jsonResponse.TryGetProperty("result", out var resultElement))
            {
                if (resultElement.TryGetProperty("content", out var contentElement))
                {
                    return contentElement.ToString();
                }
                return resultElement.ToString();
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
