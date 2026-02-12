# N8N Agent Framework - C# Conversion

This project converts an n8n workflow multi-agent system into a C# application using the Microsoft Agent Framework and Azure OpenAI.

## Architecture

The solution consists of two main projects:

### 1. WebAPI (Port 5000)
ASP.NET Core Web API that hosts the multi-agent coordinator system.

**Key Components:**
- **CoordinatorAgent**: Main agent that routes requests to specialized sub-agents
- **CmdbSubAgent**: Handles Configuration Management Database queries
- **CrmSubAgent**: Manages Customer Relationship Management queries
- **ServiceDeskSubAgent**: Processes ticket and service desk queries
- **ConversationMemoryService**: Manages conversation context and history
- **McpClient**: Communicates with MCP (Model Context Protocol) endpoints

### 2. WebChat (Port 5001)
HTML5/JavaScript web application providing a chat interface.

**Features:**
- Clean, modern chat UI
- Real-time conversation with the AI coordinator
- Session management
- Responsive design

## Prerequisites

- .NET 10 SDK
- Azure OpenAI account with API access
- Access to MCP endpoints (CMDB, CRM, ServiceDesk)

## Configuration

### WebAPI Configuration

Edit `src/WebAPI/appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-5.2-chat"
  },
  "Mcp": {
    "CmdbEndpoint": "https://cloud.sps.nl/mcp/cmdb/",
    "CrmEndpoint": "https://cloud.sps.nl/mcp/crm/",
    "ServiceDeskEndpoint": "https://cloud.sps.nl/mcp/servicedesk/",
    "AuthHeaderName": "Authorization",
    "AuthHeaderValue": "your-auth-header-value"
  }
}
```

**Configuration Parameters:**

1. **AzureOpenAI**:
   - `Endpoint`: Your Azure OpenAI resource endpoint URL
   - `ApiKey`: Your Azure OpenAI API key
   - `DeploymentName`: The deployment name (e.g., "gpt-5.2-chat")

2. **Mcp** (Model Context Protocol):
   - `CmdbEndpoint`: URL for the CMDB MCP service
   - `CrmEndpoint`: URL for the CRM MCP service
   - `ServiceDeskEndpoint`: URL for the ServiceDesk MCP service
   - `AuthHeaderName`: Authentication header name (e.g., "Authorization")
   - `AuthHeaderValue`: Authentication token/value

## Running the Application

### Option 1: Run Both Projects Separately

#### Terminal 1 - Start WebAPI:
```bash
cd src/WebAPI
dotnet run
```

The API will start on http://localhost:5000

#### Terminal 2 - Start WebChat:
```bash
cd src/WebChat
dotnet run
```

The web interface will start on http://localhost:5001

### Option 2: Using Docker (Future Enhancement)

Docker support can be added by creating Dockerfiles for each project.

## Using the Application

1. Open your browser and navigate to http://localhost:5001
2. You'll see the chat interface with a welcome message
3. Type your questions in the input field
4. The coordinator will analyze your request and route it to the appropriate agent(s)
5. Responses will appear in the chat window

### Example Queries

**CMDB Queries:**
- "Show me all servers in the production environment"
- "What are the configuration items for application X?"
- "List all virtual machines"

**CRM Queries:**
- "Find employees in the IT department"
- "Show me details for organization Acme Corp"
- "List all support groups"

**ServiceDesk Queries:**
- "Show all open tickets"
- "What are the high priority incidents?"
- "List tickets assigned to me"

**Cross-Domain Queries:**
- "Which employees manage server srv-prod-01?"
- "What systems does Company Y use?"
- "Show tickets related to configuration item X"

## Project Structure

```
n8n-convert/
├── N8nAgentFramework.slnx          # Solution file
├── src/
│   ├── WebAPI/                     # Backend API
│   │   ├── Agents/                 # Agent implementations
│   │   │   ├── BaseAgent.cs
│   │   │   ├── CoordinatorAgent.cs
│   │   │   ├── CmdbSubAgent.cs
│   │   │   ├── CrmSubAgent.cs
│   │   │   └── ServiceDeskSubAgent.cs
│   │   ├── Controllers/            # API controllers
│   │   │   └── ChatController.cs
│   │   ├── Models/                 # Data models
│   │   │   ├── ChatMessage.cs
│   │   │   └── AgentConfig.cs
│   │   ├── Services/               # Business services
│   │   │   ├── ConversationMemoryService.cs
│   │   │   └── McpClient.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   └── WebChat/                    # Frontend web app
│       ├── wwwroot/
│       │   ├── index.html
│       │   ├── css/
│       │   │   └── style.css
│       │   └── js/
│       │       └── chat.js
│       ├── Program.cs
│       └── appsettings.json
└── README.md
```

## Agent System Design

### Multi-Agent Coordinator Pattern

The system uses a hierarchical agent pattern:

1. **Coordinator Agent**: Analyzes user requests and routes to appropriate sub-agents
2. **Sub-Agents**: Specialized agents that handle specific domains:
   - **CMDB Agent**: Infrastructure and asset data
   - **CRM Agent**: People and organization data
   - **ServiceDesk Agent**: Ticket and service data

### Conversation Memory

The system maintains conversation context using a buffer window approach:
- Stores last 20 messages per session
- Session-based isolation
- In-memory storage (can be extended to persistent storage)

### MCP Integration

Each sub-agent can communicate with MCP endpoints to fetch real data:
- HTTP-based communication
- Custom authentication headers
- JSON request/response format

## API Endpoints

### POST /api/chat
Send a message to the coordinator agent.

**Request:**
```json
{
  "message": "Show me all servers",
  "sessionId": "optional-session-id"
}
```

**Response:**
```json
{
  "message": "Agent response here",
  "sessionId": "session-id",
  "intermediateSteps": []
}
```

### DELETE /api/chat/session/{sessionId}
Clear a conversation session.

## Development

### Building
```bash
dotnet build
```

### Running Tests (if added)
```bash
dotnet test
```

### Adding New Agents

1. Create a new agent class inheriting from `BaseAgent`
2. Implement the required properties and methods
3. Register the agent in `Program.cs`
4. Update the coordinator's system message to include the new agent

Example:
```csharp
public class MyNewAgent : BaseAgent
{
    public override string Name => "My New Agent";
    public override string Description => "Description here";
    
    private const string SystemMessage = "System prompt here";
    
    public MyNewAgent(ChatClient chatClient, ILogger<MyNewAgent> logger)
        : base(chatClient, SystemMessage, logger)
    {
    }
}
```

## Future Enhancements

- [ ] Add persistent conversation storage (database)
- [ ] Implement actual MCP tool calling in agents
- [ ] Add authentication and authorization
- [ ] Implement streaming responses
- [ ] Add unit tests
- [ ] Add integration tests
- [ ] Docker containerization
- [ ] Kubernetes deployment
- [ ] Add telemetry and monitoring
- [ ] Implement rate limiting
- [ ] Add response caching

## Troubleshooting

### API Connection Issues
- Ensure WebAPI is running on port 5000
- Check CORS configuration in `Program.cs`
- Verify Azure OpenAI credentials in `appsettings.json`

### Azure OpenAI Issues
- Verify your API key is correct
- Check deployment name matches your Azure setup
- Ensure your Azure account has sufficient quota

### MCP Endpoint Issues
- Verify endpoint URLs are accessible
- Check authentication headers are correct
- Review network/firewall settings

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please follow these steps:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## Support

For issues and questions, please open an issue in the repository.
