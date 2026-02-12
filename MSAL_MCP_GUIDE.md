# MSAL Authentication and MCP Integration Guide

## Overview

The application now uses **Microsoft Authentication Library (MSAL)** with PublicClientApplication for authentication and implements **actual MCP (Model Context Protocol) calls** for data retrieval.

## Authentication Flow

### 1. WebChat (Frontend) - MSAL Public Client

The WebChat project implements MSAL PublicClientApplication authentication:

```csharp
// WebChat/Program.cs
builder.Services.AddSingleton<IPublicClientApplication>(sp =>
{
    return PublicClientApplicationBuilder
        .Create(clientId)
        .WithAuthority(authority)
        .WithRedirectUri("http://localhost")
        .Build();
});
```

### 2. Device Code Flow

The authentication uses device code flow suitable for applications without web browser:

**JavaScript Flow:**
1. User clicks "Login with Microsoft"
2. App calls `/api/auth/device-code`
3. Server initiates device code flow with Azure AD
4. Access token is returned and stored in localStorage
5. Token is included in all API requests as Bearer token

**Token Management:**
- Stored in `localStorage` with expiry time
- Automatically included in API requests
- Silent token refresh when expired
- Automatic re-authentication on 401 errors

### 3. WebAPI - JWT Bearer Validation

```csharp
// WebAPI/Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = azureAdConfig["Authority"];
        options.Audience = azureAdConfig["ClientId"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
```

All API endpoints require Bearer token authentication.

## MCP Integration

### ModelContextProtocol Package

The application uses the `ModelContextProtocol` NuGet package (v0.8.0-preview.1) for MCP communication.

### MCP Service Implementation

**New Service: `McpService`**

Located at `src/WebAPI/Services/McpService.cs`, this service implements actual MCP JSON-RPC calls:

```csharp
public interface IMcpService
{
    Task<string> CallCmdbToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null);
    Task<string> CallCrmToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null);
    Task<string> CallServiceDeskToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null);
}
```

**Key Features:**
- Uses Bearer token for MCP endpoint authentication
- Implements JSON-RPC 2.0 protocol
- Proper error handling and logging
- Returns structured data from MCP endpoints

### Agent Implementation with Real MCP Calls

All sub-agents now make **actual MCP calls** instead of placeholder comments:

#### CMDB SubAgent

```csharp
// Example: Get servers
if (lowerInput.Contains("server"))
{
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
```

**Supported Queries:**
- Servers (includes virtual servers)
- Configuration Items (CIs)
- CI Classes

#### CRM SubAgent

```csharp
// Example: Get employees
if (lowerInput.Contains("employee"))
{
    var employees = await _mcpService.CallCrmToolAsync(
        "get_employees",
        new Dictionary<string, object> { { "limit", 20 } },
        accessToken);
    
    return $"CRM Query Result for employees:\n{employees}";
}
```

**Supported Queries:**
- Employees
- Organizations
- Support Groups

#### ServiceDesk SubAgent

```csharp
// Example: Get tickets
if (lowerInput.Contains("ticket"))
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
```

**Supported Queries:**
- Tickets (filtered by status)
- Incidents

### MCP Request Format

Each MCP call follows the JSON-RPC 2.0 format:

```json
{
  "jsonrpc": "2.0",
  "id": "unique-guid",
  "method": "tools/call",
  "params": {
    "name": "get_cis",
    "arguments": {
      "class": "server",
      "include_virtual": true
    }
  }
}
```

### Access Token Flow

```
User Login → MSAL → Access Token → API Request → MCP Service → MCP Endpoint
                ↓                        ↓               ↓
          localStorage            Bearer Header    Bearer Header
```

## Configuration

### Azure AD Configuration

**WebChat (`appsettings.json`):**
```json
{
  "AzureAd": {
    "ClientId": "your-client-id-here",
    "Authority": "https://login.microsoftonline.com/your-tenant-id-here",
    "RedirectUri": "http://localhost:5001",
    "Scopes": [
      "user.read",
      "api://your-api-client-id/.default"
    ]
  }
}
```

**WebAPI (`appsettings.json`):**
```json
{
  "AzureAd": {
    "ClientId": "your-client-id-here",
    "Authority": "https://login.microsoftonline.com/your-tenant-id-here"
  }
}
```

### Required Azure AD App Registration

1. **Create Azure AD App Registration** in Azure Portal
2. **Authentication Settings:**
   - Platform: Mobile and desktop applications
   - Redirect URI: `http://localhost`
   - Enable Public client flows: Yes

3. **API Permissions:**
   - Microsoft Graph: User.Read (delegated)
   - Your custom API scope (if applicable)

4. **Expose an API** (for WebAPI):
   - Create an app ID URI
   - Add a scope (e.g., `api://your-app-id/access_as_user`)

5. **Copy Configuration:**
   - Client ID from Overview
   - Tenant ID from Overview
   - Update `appsettings.json` in both projects

## Testing the Authentication Flow

### 1. Start Applications

**Terminal 1 - WebAPI:**
```bash
cd src/WebAPI
dotnet run
```

**Terminal 2 - WebChat:**
```bash
cd src/WebChat
dotnet run
```

### 2. Access WebChat

Navigate to `http://localhost:5001`

### 3. Login Flow

1. You'll see a login prompt
2. Click "Login with Microsoft"
3. Device code flow will initiate
4. Follow authentication prompts
5. Once authenticated, you can chat

### 4. Test MCP Integration

Try these queries to test actual MCP calls:

**CMDB Queries:**
- "Show me all servers"
- "List configuration items"
- "What CI classes are available?"

**CRM Queries:**
- "Find all employees"
- "Show organizations"
- "List support groups"

**ServiceDesk Queries:**
- "Show open tickets"
- "List incidents"

## Troubleshooting

### Authentication Issues

**Problem:** Login fails
**Solution:** 
- Verify Azure AD app registration settings
- Check ClientId and Authority in configuration
- Ensure Public client flows are enabled

**Problem:** Token expired
**Solution:**
- App automatically attempts silent refresh
- If that fails, user will be prompted to login again

### MCP Call Issues

**Problem:** MCP calls return errors
**Solution:**
- Verify MCP endpoints are accessible
- Check Bearer token is being passed correctly
- Review logs for detailed error messages

**Problem:** "MCP endpoint not configured"
**Solution:**
- Update `Mcp:CmdbEndpoint`, `Mcp:CrmEndpoint`, `Mcp:ServiceDeskEndpoint` in `appsettings.json`

### CORS Issues

**Problem:** CORS errors in browser console
**Solution:**
- CORS is configured to allow `http://localhost:5001`
- If using different URLs, update CORS policy in `WebAPI/Program.cs`

## Security Considerations

### Token Storage

- Access tokens stored in browser localStorage
- Tokens have expiration and are refreshed automatically
- Clear localStorage on logout

### Bearer Token Transmission

- All API requests include `Authorization: Bearer {token}` header
- WebAPI validates token signature and claims
- Token must not be expired

### MCP Authentication

- MCP endpoints receive the same Bearer token
- Endpoints should validate token before returning data
- Token scopes should match MCP permissions

## Development Notes

### Adding New MCP Tools

To add a new MCP tool call:

1. **Update McpService** with new method:
```csharp
public async Task<string> CallNewToolAsync(string toolName, Dictionary<string, object> parameters, string? accessToken = null)
{
    var endpoint = _configuration["Mcp:NewEndpoint"];
    return await CallMcpEndpointAsync(endpoint, toolName, parameters, accessToken);
}
```

2. **Update Agent** to use the new tool:
```csharp
var result = await _mcpService.CallNewToolAsync("tool_name", parameters, accessToken);
```

### Extending Authentication

For different auth flows:
- **Interactive:** Use `AcquireTokenInteractive`
- **Silent:** Already implemented with `AcquireTokenSilent`
- **Confidential Client:** Use `ConfidentialClientApplication` for daemon apps

## Summary

The application now provides:
- ✅ Secure MSAL authentication with PublicClientApplication
- ✅ JWT Bearer token validation
- ✅ **Actual MCP JSON-RPC calls** (not placeholders)
- ✅ Bearer token passed to MCP endpoints
- ✅ Automatic token refresh
- ✅ Comprehensive error handling
- ✅ Working authentication flow in UI

All "placeholder" comments have been replaced with real MCP integration code.
