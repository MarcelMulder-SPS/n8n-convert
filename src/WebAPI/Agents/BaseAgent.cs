using Azure.AI.OpenAI;
using OpenAI.Chat;
using WebAPI.Models;

namespace WebAPI.Agents;

public interface IAgent
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string input, List<ConversationMessage> conversationHistory, string? accessToken = null);
}

public abstract class BaseAgent : IAgent
{
    protected readonly ChatClient _chatClient;
    protected readonly string _systemMessage;
    protected readonly ILogger _logger;

    public abstract string Name { get; }
    public abstract string Description { get; }

    protected BaseAgent(ChatClient chatClient, string systemMessage, ILogger logger)
    {
        _chatClient = chatClient;
        _systemMessage = systemMessage;
        _logger = logger;
    }

    public virtual async Task<string> ExecuteAsync(string input, List<ConversationMessage> conversationHistory, string? accessToken = null)
    {
        try
        {
            var messages = new List<ChatMessage>();
            
            // Add system message
            messages.Add(ChatMessage.CreateSystemMessage(_systemMessage));

            // Add conversation history
            foreach (var msg in conversationHistory)
            {
                if (msg.Role == "system")
                    messages.Add(ChatMessage.CreateSystemMessage(msg.Content));
                else if (msg.Role == "assistant")
                    messages.Add(ChatMessage.CreateAssistantMessage(msg.Content));
                else
                    messages.Add(ChatMessage.CreateUserMessage(msg.Content));
            }

            // Add current user input
            messages.Add(ChatMessage.CreateUserMessage(input));

            var response = await _chatClient.CompleteChatAsync(messages);
            
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing agent {AgentName}", Name);
            return $"Error: {ex.Message}";
        }
    }
}
