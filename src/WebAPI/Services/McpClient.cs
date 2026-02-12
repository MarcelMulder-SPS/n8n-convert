using System.Text;
using System.Text.Json;
using WebAPI.Models;

namespace WebAPI.Services;

public interface IMcpClient
{
    Task<string> CallToolAsync(string toolName, Dictionary<string, object> parameters);
}

public class McpClient : IMcpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _endpointUrl;
    private readonly ILogger<McpClient> _logger;

    public McpClient(HttpClient httpClient, string endpointUrl, string authHeaderName, string authHeaderValue, ILogger<McpClient> logger)
    {
        _httpClient = httpClient;
        _endpointUrl = endpointUrl;
        _logger = logger;

        if (!string.IsNullOrEmpty(authHeaderName) && !string.IsNullOrEmpty(authHeaderValue))
        {
            _httpClient.DefaultRequestHeaders.Add(authHeaderName, authHeaderValue);
        }
    }

    public async Task<string> CallToolAsync(string toolName, Dictionary<string, object> parameters)
    {
        try
        {
            var request = new
            {
                tool = toolName,
                parameters = parameters
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpointUrl, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MCP tool {ToolName}", toolName);
            return $"Error: {ex.Message}";
        }
    }
}
