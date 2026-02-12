using Microsoft.AspNetCore.Mvc;
using WebAPI.Agents;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly CoordinatorAgent _coordinatorAgent;
    private readonly IConversationMemoryService _memoryService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        CoordinatorAgent coordinatorAgent,
        IConversationMemoryService memoryService,
        ILogger<ChatController> logger)
    {
        _coordinatorAgent = coordinatorAgent;
        _memoryService = memoryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            // Generate session ID if not provided
            var sessionId = string.IsNullOrWhiteSpace(request.SessionId) 
                ? Guid.NewGuid().ToString() 
                : request.SessionId;

            // Add user message to memory
            _memoryService.AddMessage(sessionId, new ConversationMessage
            {
                Role = "user",
                Content = request.Message
            });

            // Get conversation history
            var history = _memoryService.GetMessages(sessionId, 20);

            // Execute the coordinator agent
            var response = await _coordinatorAgent.ExecuteAsync(request.Message, history);

            // Add assistant response to memory
            _memoryService.AddMessage(sessionId, new ConversationMessage
            {
                Role = "assistant",
                Content = response
            });

            return Ok(new ChatResponse
            {
                Message = response,
                SessionId = sessionId,
                IntermediateSteps = new List<string>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return StatusCode(500, "An error occurred processing your request");
        }
    }

    [HttpDelete("session/{sessionId}")]
    public ActionResult ClearSession(string sessionId)
    {
        _memoryService.ClearSession(sessionId);
        return Ok();
    }
}
