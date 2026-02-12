# Conversion Notes: n8n to C# Agent Framework

This document outlines how the original n8n workflow was converted to a C# application using the Microsoft Agent Framework.

## Original n8n Workflow Structure

The n8n workflow consisted of:

1. **Chat Trigger Node** - Public webhook for chat interactions
2. **Main Agent Node** ("Answer Questions") - Coordinator with system message
3. **Memory Node** - Buffer window with 20 context messages
4. **Three Sub-Agent Nodes**:
   - CMDB SubAgent - With MCP Client tool
   - CRM SubAgent - With MCP Client tool
   - ServiceDesk Agent - With MCP Client tool
5. **Azure OpenAI Chat Model Nodes** - One for main agent, one for each sub-agent
6. **Think Tool Node** - For reasoning and planning

## C# Implementation Mapping

### 1. Chat Trigger → WebChat + ChatController

**n8n:** `@n8n/n8n-nodes-langchain.chatTrigger`
```json
{
  "public": true,
  "options": {}
}
```

**C#:** 
- `WebChat/wwwroot/index.html` - HTML chat interface
- `WebChat/wwwroot/js/chat.js` - JavaScript client
- `Controllers/ChatController.cs` - REST API endpoint

### 2. Main Agent → CoordinatorAgent.cs

**n8n:** `@n8n/n8n-nodes-langchain.agent`
```json
{
  "parameters": {
    "options": {
      "systemMessage": "You are a Multi-Agent Coordinator...",
      "maxIterations": 10,
      "returnIntermediateSteps": false,
      "enableStreaming": true
    }
  }
}
```

**C#:** `Agents/CoordinatorAgent.cs`
```csharp
public class CoordinatorAgent : BaseAgent
{
    private const string SystemMessage = @"You are a Multi-Agent Coordinator...";
    // System message preserved from n8n
}
```

### 3. Sub-Agents → CmdbSubAgent, CrmSubAgent, ServiceDeskSubAgent

**n8n:** `@n8n/n8n-nodes-langchain.agentTool`
```json
{
  "toolDescription": "Use this cmdb sub-agent for...",
  "text": "={{ $fromAI('Prompt__User_Message_', ...) }}",
  "options": {
    "systemMessage": "You are a specialized CMDB AI sub-agent...",
    "returnIntermediateSteps": true
  }
}
```

**C#:** `Agents/CmdbSubAgent.cs`, `Agents/CrmSubAgent.cs`, `Agents/ServiceDeskSubAgent.cs`
```csharp
public class CmdbSubAgent : BaseAgent
{
    public override string Description => "Use this cmdb sub-agent for...";
    private const string SystemMessage = @"You are a specialized CMDB AI sub-agent...";
    // All system messages preserved from n8n
}
```

### 4. Memory Buffer → ConversationMemoryService

**n8n:** `@n8n/n8n-nodes-langchain.memoryBufferWindow`
```json
{
  "contextWindowLength": 20
}
```

**C#:** `Services/ConversationMemoryService.cs`
```csharp
public class ConversationMemoryService
{
    private readonly int _maxMessagesPerSession = 20;
    // Implements buffer window with 20 messages
}
```

### 5. Azure OpenAI Chat Model → ChatClient Integration

**n8n:** `@n8n/n8n-nodes-langchain.lmChatAzureOpenAi`
```json
{
  "model": "gpt-5.2-chat",
  "credentials": {
    "azureOpenAiApi": {
      "id": "kr5Au6bNOLdwaMyD",
      "name": "Azure Open AI account"
    }
  }
}
```

**C#:** `Program.cs` + `appsettings.json`
```csharp
builder.Services.AddSingleton(sp =>
{
    var client = new AzureOpenAIClient(
        new Uri(azureConfig.Endpoint),
        new AzureKeyCredential(azureConfig.ApiKey));
    return client.GetChatClient(azureConfig.DeploymentName);
});
```

### 6. MCP Client Tools → McpClient Service

**n8n:** `@n8n/n8n-nodes-langchain.mcpClientTool`
```json
{
  "endpointUrl": "https://cloud.sps.nl/mcp/cmdb/",
  "authentication": "headerAuth",
  "credentials": {
    "httpHeaderAuth": {
      "name": "MCP Gensys Headers"
    }
  }
}
```

**C#:** `Services/McpClient.cs`
```csharp
public class McpClient : IMcpClient
{
    public async Task<string> CallToolAsync(string toolName, 
        Dictionary<string, object> parameters)
    {
        // HTTP client with header authentication
    }
}
```

### 7. Think Tool → Implicit in Agent Workflow

**n8n:** `@n8n/n8n-nodes-langchain.toolThink`
```json
{
  "description": "Use the tool to think about something..."
}
```

**C#:** Built into the agent's execution flow
```csharp
// The coordinator agent analyzes requests before routing
private async Task<string> AnalyzeRequest(string input, ...)
{
    var analysisPrompt = "Analyze this user request...";
    return await base.ExecuteAsync(analysisPrompt, ...);
}
```

## Key Architectural Differences

### n8n (Visual Workflow)
- **Node-based**: Each component is a visual node
- **Connections**: Explicit wiring between nodes
- **Execution**: Flow-based, node-to-node
- **Configuration**: JSON parameters per node
- **State**: Managed by n8n runtime
- **Deployment**: n8n server instance

### C# (Code-based)
- **Class-based**: Each component is a C# class
- **Dependencies**: Dependency injection
- **Execution**: Method calls and async/await
- **Configuration**: appsettings.json + DI
- **State**: In-memory or database
- **Deployment**: ASP.NET Core web apps

## Preserved Features

✅ **All system messages preserved exactly** from n8n configuration
✅ **Memory buffer window of 20 messages** maintained
✅ **Multi-agent coordinator pattern** implemented
✅ **Sub-agent specialization** (CMDB, CRM, ServiceDesk)
✅ **Azure OpenAI integration** with same model
✅ **MCP endpoint connectivity** structure
✅ **Conversation context** across multiple exchanges

## Enhancements in C# Version

🎯 **Type Safety**: Strong typing vs. dynamic JSON
🎯 **IDE Support**: IntelliSense, refactoring, debugging
🎯 **Testability**: Unit testing infrastructure ready
🎯 **Scalability**: Standard ASP.NET Core scaling
🎯 **Monitoring**: Application Insights ready
🎯 **Security**: Built-in authentication options
🎯 **Deployment**: Cloud-native (Azure, AWS, etc.)

## Missing Features (To Be Implemented)

⚠️ **Streaming responses**: n8n had `enableStreaming: true`
- Can be added using Server-Sent Events or SignalR

⚠️ **Intermediate steps**: n8n returned intermediate steps
- Framework is ready, needs UI implementation

⚠️ **Actual MCP tool calling**: Currently returns base responses
- Need to implement tool parsing and MCP invocation logic

⚠️ **fromAI expressions**: n8n's dynamic prompting
- Can be implemented with prompt templates

## Migration Path

For teams moving from n8n to this C# implementation:

1. **Phase 1**: Deploy C# version alongside n8n
2. **Phase 2**: A/B test both implementations
3. **Phase 3**: Gradually move traffic to C#
4. **Phase 4**: Decommission n8n workflow

## Performance Considerations

**n8n:**
- Single-threaded Node.js
- Sequential node execution
- Memory limited by Node.js

**C#:**
- Multi-threaded .NET runtime
- Parallel async operations possible
- Better memory management with GC

## Configuration Comparison

### n8n Credentials
```
Azure Open AI account:
  - ID: kr5Au6bNOLdwaMyD
  - Name: Azure Open AI account

MCP Gensys Headers:
  - ID: 0DsOrcTbuBNsWVPc
  - Name: MCP Gensys Headers
```

### C# appsettings.json
```json
{
  "AzureOpenAI": {
    "Endpoint": "...",
    "ApiKey": "...",
    "DeploymentName": "gpt-5.2-chat"
  },
  "Mcp": {
    "CmdbEndpoint": "...",
    "CrmEndpoint": "...",
    "ServiceDeskEndpoint": "...",
    "AuthHeaderName": "...",
    "AuthHeaderValue": "..."
  }
}
```

## Recommendations

1. **Keep n8n JSON**: Store original workflow as reference
2. **Version control**: Track both implementations during transition
3. **Testing**: Compare responses between n8n and C# versions
4. **Documentation**: Keep this mapping doc updated
5. **Training**: Train team on C# codebase

## Future Enhancements

- [ ] Implement function calling for MCP tools
- [ ] Add streaming responses via SignalR
- [ ] Implement intermediate step tracking
- [ ] Add metrics and monitoring
- [ ] Create automated tests
- [ ] Add CI/CD pipeline
- [ ] Implement advanced routing logic
- [ ] Add support for more sub-agents
