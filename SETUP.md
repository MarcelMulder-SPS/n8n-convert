# Setup Guide

This guide will help you set up and run the N8N Agent Framework C# application.

## Step 1: Prerequisites

Before you begin, ensure you have:

1. **.NET 10 SDK** installed
   - Download from: https://dotnet.microsoft.com/download
   - Verify installation: `dotnet --version`

2. **Azure OpenAI Account**
   - You need an Azure subscription
   - Create an Azure OpenAI resource
   - Deploy a GPT model (e.g., gpt-4, gpt-35-turbo, or gpt-5.2-chat if available)

3. **MCP Endpoints** (Optional for testing)
   - If you have access to the MCP endpoints, prepare your authentication credentials
   - If not, the system will still work but won't return real data from MCP services

## Step 2: Configure Azure OpenAI

1. Navigate to `src/WebAPI/appsettings.json`

2. Update the `AzureOpenAI` section:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-actual-api-key",
    "DeploymentName": "your-deployment-name"
  }
}
```

**How to find these values:**

- **Endpoint**: In Azure Portal, go to your Azure OpenAI resource → "Keys and Endpoint" → Copy "Endpoint"
- **ApiKey**: In Azure Portal, go to your Azure OpenAI resource → "Keys and Endpoint" → Copy "KEY 1" or "KEY 2"
- **DeploymentName**: In Azure AI Studio, go to "Deployments" → Note the deployment name (e.g., "gpt-4", "gpt-35-turbo")

## Step 3: Configure MCP Endpoints (Optional)

If you have access to MCP endpoints:

1. Navigate to `src/WebAPI/appsettings.json`

2. Update the `Mcp` section:

```json
{
  "Mcp": {
    "CmdbEndpoint": "https://your-cmdb-endpoint/",
    "CrmEndpoint": "https://your-crm-endpoint/",
    "ServiceDeskEndpoint": "https://your-servicedesk-endpoint/",
    "AuthHeaderName": "Authorization",
    "AuthHeaderValue": "Bearer your-token-here"
  }
}
```

If you don't have MCP endpoints, you can leave the default values for now.

## Step 4: Build the Solution

```bash
# Navigate to the root directory
cd /path/to/n8n-convert

# Restore NuGet packages and build
dotnet build
```

You should see output similar to:
```
Build succeeded.
    0 Error(s)
    4 Warning(s)
```

The warnings about package versions are normal and can be ignored.

## Step 5: Run the Application

### Option A: Using Two Terminal Windows

**Terminal 1 - Start the WebAPI:**
```bash
cd src/WebAPI
dotnet run
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

**Terminal 2 - Start the WebChat:**
```bash
cd src/WebChat
dotnet run
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
```

### Option B: Using Visual Studio or Rider

1. Open `N8nAgentFramework.slnx` in Visual Studio or JetBrains Rider
2. Right-click on the solution → "Set Startup Projects"
3. Select "Multiple startup projects"
4. Set both WebAPI and WebChat to "Start"
5. Press F5 or click "Start"

## Step 6: Access the Application

1. Open your web browser
2. Navigate to: http://localhost:5001
3. You should see the chat interface with a welcome message

## Step 7: Test the Chat

Try these example queries:

**Simple Test:**
```
Hello! Can you explain what you can help me with?
```

**CMDB Query:**
```
Show me all servers in the production environment
```

**CRM Query:**
```
Find employees in the IT department
```

**ServiceDesk Query:**
```
List all high priority tickets
```

## Troubleshooting

### Issue: "Unable to connect to API"

**Solution:**
1. Verify WebAPI is running on port 5000
2. Check browser console for CORS errors
3. Ensure both services are running

### Issue: "Azure OpenAI authentication failed"

**Solution:**
1. Verify your API key is correct in `appsettings.json`
2. Check that your Azure subscription is active
3. Ensure the deployment name matches exactly
4. Verify the endpoint URL is correct (should end with `.openai.azure.com/`)

### Issue: "No response from agents"

**Solution:**
1. Check the WebAPI console for error messages
2. Verify Azure OpenAI quotas haven't been exceeded
3. Check network connectivity to Azure

### Issue: "MCP endpoints returning errors"

**Solution:**
1. Verify endpoint URLs are accessible
2. Check authentication credentials
3. If endpoints are not available, the system will still work but with mock/error responses

## Development Tips

### Using appsettings.Development.json

For local development, you can use `appsettings.Development.json` to override settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-dev-resource.openai.azure.com/",
    "ApiKey": "your-dev-api-key",
    "DeploymentName": "gpt-35-turbo"
  }
}
```

### Hot Reload

Both projects support hot reload during development:
```bash
dotnet watch run
```

### Debugging

Set breakpoints in your IDE and run in debug mode to step through the code.

## Next Steps

1. Customize agent system messages in the `Agents` folder
2. Implement actual MCP client calls in sub-agents
3. Add authentication to secure the API
4. Deploy to Azure App Service or container platform

## Security Notes

⚠️ **Important Security Reminders:**

1. **Never commit `appsettings.json` with real credentials to version control**
2. Use environment variables or Azure Key Vault for production
3. Enable HTTPS for production deployments
4. Implement proper authentication and authorization
5. Set up rate limiting to prevent abuse

## Production Deployment

For production deployment:

1. Use Azure App Service or Azure Kubernetes Service
2. Configure Application Insights for monitoring
3. Set up CI/CD pipelines
4. Use managed identities instead of API keys
5. Enable SSL/TLS
6. Configure proper CORS policies
7. Implement health checks

## Getting Help

If you encounter issues:

1. Check the logs in both WebAPI and WebChat consoles
2. Review the [README.md](./README.md) for architecture details
3. Open an issue in the repository
4. Check Azure OpenAI service health status

## Useful Commands

```bash
# Clean build
dotnet clean && dotnet build

# Restore packages
dotnet restore

# Run tests (when added)
dotnet test

# Publish for production
dotnet publish -c Release
```
