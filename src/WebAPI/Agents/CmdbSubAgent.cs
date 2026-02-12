using OpenAI.Chat;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Agents;

public class CmdbSubAgent : BaseAgent
{
    private readonly IMcpService _mcpService;

    public override string Name => "CMDB Agent";
    public override string Description => "Use this cmdb sub-agent for the following tasks:\n- get cis\n- get attributes\n- get classes";

    private const string SystemMessage = @"You are a specialized CMDB (Configuration Management Database) AI sub-agent. You operate under the direction of a top-level Multi-Agent Coordinator and focus exclusively on Configuration Item (CI) data retrieval and analysis.

CORE DIRECTIVES
1. Workflow
- Always analyze the user request first
- Determine which CMDB tools to call
- Execute MCP calls to retrieve data
- Return structured results

2. Response Style
- Provide simple, direct answers - only what was requested
- No chitchat, no pleasantries, no elaboration
- Focus purely on delivering the requested data
- Be concise and factual

3. CI Class Search Strategy
CRITICAL: When searching for CIs by class, always include virtual equivalents
Examples:
- If searching for ""servers"" → also search for ""virtual servers""
- If searching for ""storage"" → also search for ""virtual storage""
- If searching for ""networks"" → also search for ""virtual networks""
- Apply this logic to all infrastructure components that may have virtualized versions

4. Cross-Agent Data Handling
- You only know CI data and CI identifiers
- You do not have access to names/details from other agents (CRM data like employee names, organization names, etc.)
- You work with IDs only when referencing data from other systems
- If you need information about organizations or employees, you can only work with their IDs, not their names

YOUR CAPABILITIES
You can search and retrieve:
- Configuration Items (CIs)
- CI relationships and dependencies
- CI attributes and properties
- Infrastructure components (servers, networks, applications, storage, etc.)
- Both physical and virtual assets
- CI class hierarchies
- CI status and lifecycle information

RESPONSE FORMAT
Good Response:
Found 3 servers:
- srv-prod-01 (Physical Server)
- srv-prod-02 (Physical Server)  
- vsrv-prod-03 (Virtual Server)

Bad Response:
Great! I'd be happy to help you find servers. Let me search through the CMDB for you. 
I found several servers that might be what you're looking for...

CRITICAL REMINDERS
✅ Always use think1 tool first
✅ Search for virtual equivalents when querying CI classes
✅ Provide minimal, direct answers only
✅ Work with IDs only for cross-agent data
❌ No chitchat or conversational filler
❌ Don't assume you know names from other systems

You are a focused, efficient data retrieval specialist. Execute tasks precisely and return only what was requested.";

    public CmdbSubAgent(ChatClient chatClient, IMcpService mcpService, ILogger<CmdbSubAgent> logger)
        : base(chatClient, SystemMessage, logger)
    {
        _mcpService = mcpService;
    }

    public override async Task<string> ExecuteAsync(string input, List<ConversationMessage> conversationHistory, string? accessToken = null)
    {
        try
        {
            // Example: Parse the input and make actual MCP calls
            // This is a simple implementation - in production, you'd use more sophisticated parsing
            
            var lowerInput = input.ToLower();
            
            // Handle common CMDB queries
            if (lowerInput.Contains("server") || lowerInput.Contains("servers"))
            {
                // Call MCP to get servers
                var servers = await _mcpService.CallCmdbToolAsync(
                    "get_cis", 
                    new Dictionary<string, object> 
                    { 
                        { "class", "server" },
                        { "include_virtual", true }
                    },
                    accessToken);
                
                return $"CMDB Query Result for servers:\n{servers}";
            }
            else if (lowerInput.Contains("ci") || lowerInput.Contains("configuration item"))
            {
                // General CI query
                var cis = await _mcpService.CallCmdbToolAsync(
                    "get_cis",
                    new Dictionary<string, object> { { "limit", 10 } },
                    accessToken);
                
                return $"CMDB Query Result:\n{cis}";
            }
            else if (lowerInput.Contains("class"))
            {
                // Get CI classes
                var classes = await _mcpService.CallCmdbToolAsync(
                    "get_classes",
                    new Dictionary<string, object>(),
                    accessToken);
                
                return $"CMDB Classes:\n{classes}";
            }
            
            // For other queries, use AI to determine the appropriate action
            var aiResponse = await base.ExecuteAsync(input, conversationHistory, accessToken);
            
            // If AI suggests a specific tool, you could parse and execute it here
            // For now, return the AI response which provides guidance
            return aiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CMDB agent execution");
            return $"Error executing CMDB query: {ex.Message}";
        }
    }
}
