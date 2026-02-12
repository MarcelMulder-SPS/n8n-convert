using OpenAI.Chat;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Agents;

public class CrmSubAgent : BaseAgent
{
    private readonly IMcpService _mcpService;

    public override string Name => "CRM Agent";
    public override string Description => "Use this crm sub-agent for the following tasks:\n- employees\n- organisations\n- users (engineers)\n- supportgroups";

    private const string SystemMessage = @"You are a specialized CRM (Customer Relationship Management) AI sub-agent. You operate under the direction of a top-level Multi-Agent Coordinator and focus exclusively on people, organizations, and relationship data retrieval.

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
- Employees - Staff members, personnel, workforce data
- Organizations - Companies, business units, departments, organizational entities
- Users (Engineers) - Technical users, engineering staff, system users
- Support Groups - Teams, support units, service groups

YOUR CAPABILITIES
You can search and retrieve:
- Employee information (names, roles, contact details, assignments)
- Organization details (company names, structures, relationships)
- User/Engineer data (accounts, technical roles, responsibilities)
- Support group information (group names, members, assignments)
- Relationships between these entities

RESPONSE FORMAT
Good Response:
Found 2 employees:
- Jan de Vries (Senior Engineer)
- Maria Johnson (Support Specialist)

Bad Response:
Sure, I'd be happy to help you find employees! Let me look that up for you in our CRM system. Here's what I found...

CRITICAL REMINDERS
✅ Always call the think tool first
✅ Provide minimal, direct answers only
✅ Focus on employees, organizations, users, and support groups
✅ Return structured data without commentary
❌ No chitchat or conversational filler
❌ Don't provide information outside your scope (e.g., CI/infrastructure data)

You are a focused, efficient data retrieval specialist for people and organizational data. Execute tasks precisely and return only what was requested.";

    public CrmSubAgent(ChatClient chatClient, IMcpService mcpService, ILogger<CrmSubAgent> logger)
        : base(chatClient, SystemMessage, logger)
    {
        _mcpService = mcpService;
    }

    public override async Task<string> ExecuteAsync(string input, List<ConversationMessage> conversationHistory, string? accessToken = null)
    {
        try
        {
            var lowerInput = input.ToLower();
            
            // Handle common CRM queries
            if (lowerInput.Contains("employee") || lowerInput.Contains("employees"))
            {
                var employees = await _mcpService.CallCrmToolAsync(
                    "get_employees",
                    new Dictionary<string, object> { { "limit", 20 } },
                    accessToken);
                
                return $"CRM Query Result for employees:\n{employees}";
            }
            else if (lowerInput.Contains("organization") || lowerInput.Contains("organisation"))
            {
                var orgs = await _mcpService.CallCrmToolAsync(
                    "get_organizations",
                    new Dictionary<string, object>(),
                    accessToken);
                
                return $"CRM Query Result for organizations:\n{orgs}";
            }
            else if (lowerInput.Contains("support group"))
            {
                var groups = await _mcpService.CallCrmToolAsync(
                    "get_support_groups",
                    new Dictionary<string, object>(),
                    accessToken);
                
                return $"CRM Query Result for support groups:\n{groups}";
            }
            
            // For other queries, use AI to determine the appropriate action
            return await base.ExecuteAsync(input, conversationHistory, accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CRM agent execution");
            return $"Error executing CRM query: {ex.Message}";
        }
    }
}
