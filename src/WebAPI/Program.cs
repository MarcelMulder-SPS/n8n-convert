using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using WebAPI.Agents;
using WebAPI.Models;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Azure OpenAI
var azureConfig = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAIConfig>();
if (azureConfig == null)
{
    throw new InvalidOperationException("AzureOpenAI configuration is missing");
}

// Register ChatClient as singleton
builder.Services.AddSingleton(sp =>
{
    var client = new AzureOpenAIClient(
        new Uri(azureConfig.Endpoint),
        new AzureKeyCredential(azureConfig.ApiKey));
    return client.GetChatClient(azureConfig.DeploymentName);
});

// Configure MCP settings
var mcpConfig = builder.Configuration.GetSection("Mcp").Get<McpConfig>();
if (mcpConfig == null)
{
    throw new InvalidOperationException("MCP configuration is missing");
}

// Register MCP clients
builder.Services.AddHttpClient();
builder.Services.AddSingleton(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var logger = sp.GetRequiredService<ILogger<McpClient>>();
    var httpClient = httpClientFactory.CreateClient("CmdbMcp");
    return new McpClient(httpClient, mcpConfig.CmdbEndpoint, mcpConfig.AuthHeaderName, mcpConfig.AuthHeaderValue, logger);
});

// Register services
builder.Services.AddSingleton<IConversationMemoryService, ConversationMemoryService>();

// Register agents
builder.Services.AddSingleton<CmdbSubAgent>();
builder.Services.AddSingleton<CrmSubAgent>();
builder.Services.AddSingleton<ServiceDeskSubAgent>();
builder.Services.AddSingleton<CoordinatorAgent>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
