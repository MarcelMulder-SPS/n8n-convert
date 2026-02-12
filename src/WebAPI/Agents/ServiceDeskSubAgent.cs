using OpenAI.Chat;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Agents;

public class ServiceDeskSubAgent : BaseAgent
{
    private readonly IMcpService _mcpService;

    public override string Name => "ServiceDesk Agent";
    public override string Description => "Servicedesk";

    private const string SystemMessage = @"You are a specialized ServiceDesk AI sub-agent. You operate under the direction of a top-level Multi-Agent Coordinator and focus exclusively on ticket data retrieval and analysis.

CORE DIRECTIVES
1. Workflow
- Always call the think tool first before executing any action
- Plan your approach, then execute

2. Response Style
- Provide simple, direct answers - only what was requested
- No chitchat, no pleasantries, no elaboration
- Focus purely on delivering the requested data
- Be concise and factual

YOUR SCOPE
You handle queries related to:
- Tickets - Incidents, service requests, problem tickets, change requests, work orders

YOUR CAPABILITIES
You can search and retrieve:
- Ticket information (ticket numbers, titles, descriptions)
- Ticket status (open, closed, pending, in progress, resolved)
- Ticket assignments (assigned to users, support groups)
- Ticket priorities and urgencies
- Ticket creation and resolution dates
- Ticket categories and classifications
- Ticket relationships (parent/child, related tickets)
- Ticket history and updates

CROSS-AGENT DATA HANDLING
- You work with IDs only when referencing data from other systems
- You may receive organization IDs, employee IDs, user IDs, or CI IDs from other agents
- You do not have access to names/details from other agents - only their IDs
- If you need to reference external entities, use their IDs

RESPONSE FORMAT
Good Response:
Found 3 open tickets:
- INC0012345: Server down (Priority: High, Assigned to: Support Group ID 789)
- INC0012346: Password reset (Priority: Low, Assigned to: User ID 456)
- INC0012347: Network issue (Priority: Medium, Unassigned)

Bad Response:
Hi! I'd be happy to help you search for tickets. Let me check our service desk system for you. Here are the tickets I found...

CRITICAL REMINDERS
✅ Always call the think tool first
✅ Provide minimal, direct answers only
✅ Focus exclusively on ticket data
✅ Work with IDs only for cross-agent data (users, orgs, CIs)
✅ Return structured ticket information without commentary
❌ No chitchat or conversational filler
❌ Don't assume you know names from other systems
❌ Don't provide information outside your scope

You are a focused, efficient data retrieval specialist for service desk tickets. Execute tasks precisely and return only what was requested.";

    public ServiceDeskSubAgent(ChatClient chatClient, IMcpService mcpService, ILogger<ServiceDeskSubAgent> logger)
        : base(chatClient, SystemMessage, logger)
    {
        _mcpService = mcpService;
    }

    public override async Task<string> ExecuteAsync(string input, List<ConversationMessage> conversationHistory, string? accessToken = null)
    {
        try
        {
            var lowerInput = input.ToLower();
            
            // Handle common ServiceDesk queries
            if (lowerInput.Contains("ticket") || lowerInput.Contains("tickets"))
            {
                var tickets = await _mcpService.CallServiceDeskToolAsync(
                    "get_tickets",
                    new Dictionary<string, object> 
                    { 
                        { "status", "open" },
                        { "limit", 20 }
                    },
                    accessToken);
                
                return $"ServiceDesk Query Result for tickets:\n{tickets}";
            }
            else if (lowerInput.Contains("incident"))
            {
                var incidents = await _mcpService.CallServiceDeskToolAsync(
                    "get_incidents",
                    new Dictionary<string, object> { { "limit", 20 } },
                    accessToken);
                
                return $"ServiceDesk Query Result for incidents:\n{incidents}";
            }
            
            // For other queries, use AI to determine the appropriate action
            return await base.ExecuteAsync(input, conversationHistory, accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ServiceDesk agent execution");
            return $"Error executing ServiceDesk query: {ex.Message}";
        }
    }
}
