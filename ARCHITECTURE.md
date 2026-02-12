# Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          USER BROWSER                                   │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │              WebChat (http://localhost:5001)                       │ │
│  │                                                                    │ │
│  │  ┌──────────────────────────────────────────────────────────┐    │ │
│  │  │  HTML5 Chat Interface (index.html)                       │    │ │
│  │  │  - Modern gradient UI                                    │    │ │
│  │  │  - Real-time messaging                                   │    │ │
│  │  │  - Session management                                    │    │ │
│  │  └──────────────────────────────────────────────────────────┘    │ │
│  │                            │                                      │ │
│  │                            │ JavaScript (chat.js)                 │ │
│  │                            ▼                                      │ │
│  │  ┌──────────────────────────────────────────────────────────┐    │ │
│  │  │  REST API Client                                         │    │ │
│  │  │  - POST /api/chat                                        │    │ │
│  │  │  - DELETE /api/chat/session/{id}                         │    │ │
│  │  └──────────────────────────────────────────────────────────┘    │ │
│  └────────────────────────────────────┬─────────────────────────────┘ │
└─────────────────────────────────────────┬─────────────────────────────┘
                                          │
                                          │ HTTP/JSON
                                          ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    WebAPI (http://localhost:5000)                       │
│                      ASP.NET Core Web API                               │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │  ChatController                                                    │ │
│  │  - Handles HTTP requests                                          │ │
│  │  - Manages sessions                                               │ │
│  │  - Routes to coordinator                                          │ │
│  └─────────────────────────────┬─────────────────────────────────────┘ │
│                                │                                       │
│                                ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │  ConversationMemoryService                                        │ │
│  │  - 20 message buffer window                                       │ │
│  │  - Session-based storage                                          │ │
│  │  - In-memory (concurrent dict)                                    │ │
│  └───────────────────────────────────────────────────────────────────┘ │
│                                │                                       │
│                                ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │             COORDINATOR AGENT                                     │ │
│  │  ┌─────────────────────────────────────────────────────────────┐ │ │
│  │  │ CoordinatorAgent (BaseAgent)                                │ │ │
│  │  │ - Analyzes user requests                                    │ │ │
│  │  │ - Routes to specialized agents                              │ │ │
│  │  │ - Synthesizes responses                                     │ │ │
│  │  │ - Hides internal IDs                                        │ │ │
│  │  │ - Multilingual support                                      │ │ │
│  │  └─────────────────────────────────────────────────────────────┘ │ │
│  │                     │           │            │                    │ │
│  │        ┌────────────┴───────────┴────────────┴────────┐          │ │
│  │        │            │           │            │         │          │ │
│  │        ▼            ▼           ▼            ▼         │          │ │
│  │  ┌─────────┐  ┌─────────┐ ┌──────────┐ ┌────────┐    │          │ │
│  │  │  CMDB   │  │   CRM   │ │ServiceDesk│ │ Think  │    │          │ │
│  │  │ SubAgent│  │SubAgent │ │  Agent    │ │ (impl) │    │          │ │
│  │  └────┬────┘  └────┬────┘ └─────┬────┘ └────────┘    │          │ │
│  │       │            │            │                      │          │ │
│  │       │            │            │                      │          │ │
│  │  ┌────▼────────────▼────────────▼──────────────────┐  │          │ │
│  │  │         ChatClient (Azure OpenAI)               │  │          │ │
│  │  │  - gpt-5.2-chat deployment                      │  │          │ │
│  │  │  - Shared across all agents                     │  │          │ │
│  │  └─────────────────────────────────────────────────┘  │          │ │
│  └───────────────────────────────────────────────────────────────────┘ │
│                                │                                       │
│                                ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │  McpClient Service                                                │ │
│  │  - HTTP client for MCP endpoints                                 │ │
│  │  - Header authentication                                         │ │
│  │  - JSON request/response                                         │ │
│  └─────────────────┬───────────────┬─────────────────┬───────────────┘ │
└─────────────────────┼───────────────┼─────────────────┼─────────────────┘
                      │               │                 │
                      │ HTTPS         │ HTTPS           │ HTTPS
                      ▼               ▼                 ▼
         ┌────────────────┐ ┌────────────────┐ ┌────────────────┐
         │  MCP: CMDB     │ │  MCP: CRM      │ │ MCP: ServiceDesk│
         │  Endpoint      │ │  Endpoint      │ │   Endpoint     │
         │                │ │                │ │                │
         │ - Get CIs      │ │ - Employees    │ │ - Tickets      │
         │ - Attributes   │ │ - Organizations│ │ - Incidents    │
         │ - Classes      │ │ - Users        │ │ - Requests     │
         │                │ │ - Support Grps │ │                │
         └────────────────┘ └────────────────┘ └────────────────┘
```

## Data Flow

1. **User Input**: User types message in WebChat UI
2. **HTTP Request**: JavaScript sends POST to `/api/chat`
3. **Session Management**: Controller retrieves/creates session ID
4. **Memory Lookup**: Gets last 20 messages from ConversationMemory
5. **Coordinator Analysis**: Coordinator analyzes request type
6. **Agent Routing**: Routes to CMDB, CRM, or ServiceDesk agent
7. **Azure OpenAI**: Agent calls ChatClient for AI response
8. **MCP Integration**: (Optional) Agent calls MCP endpoints for data
9. **Response Synthesis**: Coordinator combines sub-agent responses
10. **Memory Update**: Saves conversation to memory
11. **HTTP Response**: Returns JSON response to UI
12. **UI Update**: Chat interface displays message

## Technology Stack

### Frontend (WebChat)
- **Framework**: ASP.NET Core 10.0 (Static Files)
- **UI**: Pure HTML5, CSS3, JavaScript (ES6+)
- **Design**: Gradient UI, responsive, modern
- **Port**: 5001

### Backend (WebAPI)
- **Framework**: ASP.NET Core 10.0
- **Language**: C# 13
- **AI Integration**: Azure.AI.OpenAI 2.1.0
- **DI**: Microsoft.Extensions.DependencyInjection
- **Port**: 5000

### External Services
- **AI Model**: Azure OpenAI (gpt-5.2-chat)
- **Data Sources**: MCP Endpoints (CMDB, CRM, ServiceDesk)
- **Authentication**: Header-based (configurable)

## Agent Capabilities

### CMDB SubAgent
- Configuration Items (CIs)
- Infrastructure components
- Virtual and physical assets
- CI relationships
- Hardware/software inventory

### CRM SubAgent
- Employee information
- Organization details
- User accounts
- Support groups
- Contact management

### ServiceDesk Agent
- Ticket management
- Incident tracking
- Service requests
- Priority handling
- Status monitoring

## Key Design Patterns

1. **Multi-Agent Coordinator**: Central routing intelligence
2. **Strategy Pattern**: Different agents for different domains
3. **Dependency Injection**: Loose coupling, testability
4. **Repository Pattern**: ConversationMemoryService
5. **Adapter Pattern**: McpClient wraps external APIs
6. **Template Method**: BaseAgent defines agent structure
