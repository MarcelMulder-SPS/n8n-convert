using System.Collections.Concurrent;
using WebAPI.Models;

namespace WebAPI.Services;

public interface IConversationMemoryService
{
    void AddMessage(string sessionId, ConversationMessage message);
    List<ConversationMessage> GetMessages(string sessionId, int maxMessages = 20);
    void ClearSession(string sessionId);
}

public class ConversationMemoryService : IConversationMemoryService
{
    private readonly ConcurrentDictionary<string, List<ConversationMessage>> _conversations = new();
    private readonly int _maxMessagesPerSession = 20;

    public void AddMessage(string sessionId, ConversationMessage message)
    {
        var messages = _conversations.GetOrAdd(sessionId, _ => new List<ConversationMessage>());
        
        lock (messages)
        {
            messages.Add(message);
            
            // Keep only the last N messages to prevent memory issues
            if (messages.Count > _maxMessagesPerSession * 2)
            {
                messages.RemoveRange(0, messages.Count - _maxMessagesPerSession);
            }
        }
    }

    public List<ConversationMessage> GetMessages(string sessionId, int maxMessages = 20)
    {
        if (!_conversations.TryGetValue(sessionId, out var messages))
        {
            return new List<ConversationMessage>();
        }

        lock (messages)
        {
            return messages.TakeLast(maxMessages).ToList();
        }
    }

    public void ClearSession(string sessionId)
    {
        _conversations.TryRemove(sessionId, out _);
    }
}
