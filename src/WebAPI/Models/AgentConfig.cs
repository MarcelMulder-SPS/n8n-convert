namespace WebAPI.Models;

public class AgentConfig
{
    public string Name { get; set; } = string.Empty;
    public string SystemMessage { get; set; } = string.Empty;
    public string ToolDescription { get; set; } = string.Empty;
    public string McpEndpointUrl { get; set; } = string.Empty;
    public bool ReturnIntermediateSteps { get; set; }
}

public class AzureOpenAIConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
}

public class McpConfig
{
    public string CmdbEndpoint { get; set; } = string.Empty;
    public string CrmEndpoint { get; set; } = string.Empty;
    public string ServiceDeskEndpoint { get; set; } = string.Empty;
    public string AuthHeaderName { get; set; } = string.Empty;
    public string AuthHeaderValue { get; set; } = string.Empty;
}
