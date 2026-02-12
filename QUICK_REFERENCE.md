# Developer Quick Reference

## Project Overview

**n8n-convert** - Multi-agent AI system converted from n8n to C# using Microsoft Agent Framework

## Quick Start

```bash
# Clone and navigate
cd n8n-convert

# Configure Azure OpenAI
nano src/WebAPI/appsettings.json

# Run (Linux/Mac)
./start.sh

# Run (Windows)
start.bat

# Or manually
cd src/WebAPI && dotnet run
cd src/WebChat && dotnet run
```

## Ports

- **WebAPI**: http://localhost:5000
- **WebChat**: http://localhost:5001

## Key Files

| File | Purpose |
|------|---------|
| `src/WebAPI/Program.cs` | Main entry point, DI configuration |
| `src/WebAPI/Controllers/ChatController.cs` | REST API endpoints |
| `src/WebAPI/Agents/CoordinatorAgent.cs` | Main coordinator logic |
| `src/WebAPI/Agents/*SubAgent.cs` | Specialized sub-agents |
| `src/WebAPI/Services/ConversationMemoryService.cs` | Session memory |
| `src/WebAPI/Services/McpClient.cs` | MCP endpoint client |
| `src/WebAPI/appsettings.json` | Configuration |
| `src/WebChat/wwwroot/index.html` | Chat UI |
| `src/WebChat/wwwroot/js/chat.js` | Frontend logic |

## API Endpoints

### POST /api/chat
Send a message to the AI

**Request:**
```json
{
  "message": "Your question here",
  "sessionId": "optional-session-id"
}
```

**Response:**
```json
{
  "message": "AI response",
  "sessionId": "session-id",
  "intermediateSteps": []
}
```

### DELETE /api/chat/session/{sessionId}
Clear conversation history

## Configuration

### Azure OpenAI (Required)

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "ApiKey": "YOUR-API-KEY",
    "DeploymentName": "gpt-4"
  }
}
```

### MCP Endpoints (Optional)

```json
{
  "Mcp": {
    "CmdbEndpoint": "https://your-cmdb-endpoint/",
    "CrmEndpoint": "https://your-crm-endpoint/",
    "ServiceDeskEndpoint": "https://your-servicedesk-endpoint/",
    "AuthHeaderName": "Authorization",
    "AuthHeaderValue": "Bearer YOUR-TOKEN"
  }
}
```

## Agent System

### Coordinator Agent
- **File**: `Agents/CoordinatorAgent.cs`
- **Role**: Routes requests to sub-agents
- **Features**: Multi-language, ID hiding, synthesis

### CMDB SubAgent
- **File**: `Agents/CmdbSubAgent.cs`
- **Domain**: Infrastructure, CIs, assets
- **MCP**: https://cloud.sps.nl/mcp/cmdb/

### CRM SubAgent
- **File**: `Agents/CrmSubAgent.cs`
- **Domain**: People, orgs, users
- **MCP**: https://cloud.sps.nl/mcp/crm/

### ServiceDesk Agent
- **File**: `Agents/ServiceDeskSubAgent.cs`
- **Domain**: Tickets, incidents
- **MCP**: https://cloud.sps.nl/mcp/servicedesk/

## Memory Management

- **Service**: `ConversationMemoryService`
- **Storage**: In-memory, concurrent dictionary
- **Window**: 20 messages per session
- **Session**: GUID-based, client-managed

## Common Tasks

### Add a New Agent

1. Create `Agents/MyNewAgent.cs`:
```csharp
public class MyNewAgent : BaseAgent
{
    public override string Name => "My New Agent";
    public override string Description => "What it does";
    
    private const string SystemMessage = @"Your system prompt";
    
    public MyNewAgent(ChatClient client, ILogger<MyNewAgent> logger)
        : base(client, SystemMessage, logger) { }
}
```

2. Register in `Program.cs`:
```csharp
builder.Services.AddSingleton<MyNewAgent>();
```

3. Add to coordinator:
```csharp
public CoordinatorAgent(
    ChatClient chatClient,
    MyNewAgent myNewAgent, // Add parameter
    ...)
{
    _subAgents = new Dictionary<string, IAgent>
    {
        { "mynew", myNewAgent }, // Register
        ...
    };
}
```

### Modify System Prompt

Edit the `SystemMessage` constant in the respective agent class.

### Change Memory Buffer Size

Edit `ConversationMemoryService.cs`:
```csharp
private readonly int _maxMessagesPerSession = 20; // Change this
```

### Add Logging

Inject `ILogger<T>` and use:
```csharp
_logger.LogInformation("Message");
_logger.LogError(ex, "Error occurred");
```

## Debugging

### Visual Studio / Rider
1. Set breakpoints in agent files
2. F5 to debug both projects
3. Inspect variables, step through

### Console Logging
```csharp
Console.WriteLine("Debug info");
```

### Browser DevTools
- F12 → Console for JavaScript errors
- Network tab for API calls

## Testing

### Manual Testing
1. Start both projects
2. Open http://localhost:5001
3. Type test queries

### Example Queries
```
"Show me all servers"
"Find employees in IT"
"List high priority tickets"
"Who manages server X?"
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Can't connect to API | Check WebAPI is running on port 5000 |
| Azure OpenAI error | Verify credentials in appsettings.json |
| CORS error | Check CORS policy in Program.cs |
| No response | Check console logs for exceptions |
| Session lost | Sessions are in-memory, restart clears them |

## Build & Deploy

```bash
# Clean build
dotnet clean && dotnet build

# Publish for production
dotnet publish -c Release -o ./publish

# Run published version
cd publish && dotnet WebAPI.dll
```

## Environment Variables (Alternative Config)

Instead of `appsettings.json`, use:

```bash
export AzureOpenAI__Endpoint="https://..."
export AzureOpenAI__ApiKey="..."
export AzureOpenAI__DeploymentName="gpt-4"
```

## Code Structure

```
src/WebAPI/
├── Agents/           # AI agent classes
├── Controllers/      # REST API endpoints
├── Models/          # Data models
├── Services/        # Business logic
└── Program.cs       # Startup & DI

src/WebChat/
├── wwwroot/
│   ├── index.html   # Chat UI
│   ├── css/         # Styles
│   └── js/          # Frontend logic
└── Program.cs       # Static file server
```

## Dependencies

```xml
<PackageReference Include="Azure.AI.OpenAI" Version="2.1.0" />
<PackageReference Include="Microsoft.Extensions.AI" Version="9.5.0" />
```

## NuGet Commands

```bash
# Add package
dotnet add package PackageName

# Update all
dotnet restore

# List packages
dotnet list package
```

## Git Workflow

```bash
# Status
git status

# Commit
git add .
git commit -m "Description"

# Push
git push origin main
```

## Performance Tips

1. **Caching**: Consider caching MCP responses
2. **Connection Pooling**: HttpClient is registered as singleton
3. **Async/Await**: All I/O operations are async
4. **Memory**: Monitor session count in production

## Security Checklist

- [ ] Use environment variables for secrets
- [ ] Enable HTTPS in production
- [ ] Implement authentication
- [ ] Add rate limiting
- [ ] Validate all inputs
- [ ] Don't expose error details to users
- [ ] Use Azure Key Vault for secrets

## Resources

- [README.md](./README.md) - Full documentation
- [SETUP.md](./SETUP.md) - Setup instructions  
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Architecture diagram
- [CONVERSION_NOTES.md](./CONVERSION_NOTES.md) - n8n comparison
- [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)
- [Azure OpenAI Docs](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

## Support

- GitHub Issues: Report bugs
- Pull Requests: Contribute improvements
- Discussions: Ask questions

## License

MIT License - See LICENSE file
