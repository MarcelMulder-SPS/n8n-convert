using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenAI.Chat;
using WebAPI.Agents;
using WebAPI.Models;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure JWT Authentication
var azureAdConfig = builder.Configuration.GetSection("AzureAd");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = azureAdConfig["Authority"];
        options.Audience = azureAdConfig["ClientId"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

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

// Register HttpClient for MCP Service
builder.Services.AddHttpClient();

// Register MCP Service with ModelContextProtocol
builder.Services.AddSingleton<IMcpService, McpService>();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
