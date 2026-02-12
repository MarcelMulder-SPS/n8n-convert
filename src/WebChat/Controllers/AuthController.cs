using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace WebChat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPublicClientApplication _publicClientApp;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IPublicClientApplication publicClientApp, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _publicClientApp = publicClientApp;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("config")]
    public IActionResult GetMsalConfig()
    {
        var config = new
        {
            ClientId = _configuration["AzureAd:ClientId"],
            Authority = _configuration["AzureAd:Authority"],
            RedirectUri = _configuration["AzureAd:RedirectUri"],
            Scopes = _configuration.GetSection("AzureAd:Scopes").Get<string[]>()
        };
        
        return Ok(config);
    }

    [HttpPost("device-code")]
    public async Task<IActionResult> GetDeviceCode()
    {
        try
        {
            var scopes = _configuration.GetSection("AzureAd:Scopes").Get<string[]>() ?? new[] { "user.read" };
            
            var result = await _publicClientApp.AcquireTokenWithDeviceCode(
                scopes,
                deviceCodeResult =>
                {
                    // This will be returned to the client for display
                    return Task.CompletedTask;
                }).ExecuteAsync();

            return Ok(new
            {
                AccessToken = result.AccessToken,
                ExpiresOn = result.ExpiresOn,
                Account = result.Account?.Username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring device code token");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpGet("accounts")]
    public IActionResult GetAccounts()
    {
        var accounts = _publicClientApp.GetAccountsAsync().Result;
        return Ok(accounts.Select(a => new { a.Username, a.HomeAccountId }));
    }

    [HttpPost("acquire-token-silent")]
    public async Task<IActionResult> AcquireTokenSilent([FromBody] SilentTokenRequest request)
    {
        try
        {
            var accounts = await _publicClientApp.GetAccountsAsync();
            var account = accounts.FirstOrDefault(a => a.HomeAccountId.Identifier == request.AccountId);

            if (account == null)
            {
                return BadRequest(new { Error = "Account not found" });
            }

            var scopes = _configuration.GetSection("AzureAd:Scopes").Get<string[]>() ?? new[] { "user.read" };
            
            var result = await _publicClientApp.AcquireTokenSilent(scopes, account).ExecuteAsync();

            return Ok(new
            {
                AccessToken = result.AccessToken,
                ExpiresOn = result.ExpiresOn
            });
        }
        catch (MsalUiRequiredException)
        {
            return Unauthorized(new { Error = "User interaction required" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring token silently");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    public class SilentTokenRequest
    {
        public string AccountId { get; set; } = string.Empty;
    }
}
