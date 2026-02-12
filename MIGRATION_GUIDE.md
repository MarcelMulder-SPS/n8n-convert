# Migration Guide - MSAL Token Authentication & Razor Pages

## Overview of Changes

This document explains the recent updates to simplify authentication and modernize the WebChat UI.

## 1. Unified Authentication with MSAL Token

### What Changed

Previously, MCP endpoints required separate authentication configuration in `appsettings.json`. Now, MCP endpoints use the **same access token** from your MSAL login.

### Before (Old Approach)

**Configuration:**
```json
{
  "Mcp": {
    "CmdbEndpoint": "https://cloud.sps.nl/mcp/cmdb/",
    "AuthHeaderName": "Authorization",
    "AuthHeaderValue": "Bearer your-static-token-here"  ← Required
  }
}
```

**Problem:** You had to manage two separate authentication tokens.

### After (New Approach)

**Configuration:**
```json
{
  "Mcp": {
    "CmdbEndpoint": "https://cloud.sps.nl/mcp/cmdb/",
    "CrmEndpoint": "https://cloud.sps.nl/mcp/crm/",
    "ServiceDeskEndpoint": "https://cloud.sps.nl/mcp/servicedesk/"
  }
}
```

**Benefit:** One authentication token for everything!

### How It Works

```
┌─────────────────┐
│  User Login     │ → MSAL Device Code Flow
└────────┬────────┘
         ↓
┌─────────────────┐
│  Access Token   │ → Stored in localStorage
└────────┬────────┘
         ↓
┌─────────────────┐
│  API Request    │ → Authorization: Bearer {token}
└────────┬────────┘
         ↓
┌─────────────────┐
│  MCP Service    │ → Uses SAME token
└────────┬────────┘
         ↓
┌─────────────────┐
│  MCP Endpoints  │ → CMDB, CRM, ServiceDesk authenticated
└─────────────────┘
```

### Code Changes

**McpService.cs** already uses the passed token:

```csharp
private async Task<string> CallMcpEndpointAsync(
    string? endpointUrl,
    string toolName,
    Dictionary<string, object> parameters,
    string? accessToken)  // ← Token from MSAL login
{
    var httpClient = _httpClientFactory.CreateClient();
    
    // Add Bearer token if available
    if (!string.IsNullOrEmpty(accessToken))
    {
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    }
    
    // Make MCP call...
}
```

### What You Need to Do

**Remove** old MCP authentication from your `appsettings.json`:
```json
// ❌ DELETE these lines:
"AuthHeaderName": "Authorization",
"AuthHeaderValue": "your-auth-header-value"
```

That's it! The MCP endpoints will automatically use your login token.

## 2. WebChat Converted to Razor Pages

### What Changed

WebChat was using static HTML files. Now it uses **ASP.NET Core Razor Pages** for better integration.

### Before (Static HTML)

```
src/WebChat/
├── wwwroot/
│   └── index.html  ← Static HTML file
│   └── css/style.css
│   └── js/chat.js
└── Program.cs
```

### After (Razor Pages)

```
src/WebChat/
├── Pages/
│   ├── Index.cshtml       ← Razor page (replaces index.html)
│   ├── Index.cshtml.cs    ← Page model (server-side code)
│   └── _ViewImports.cshtml ← Razor configuration
├── wwwroot/
│   ├── css/style.css      ← Same CSS
│   └── js/chat.js         ← Same JavaScript
└── Program.cs
```

### What Stayed the Same

✅ All CSS styling (no visual changes)  
✅ All JavaScript functionality (same chat behavior)  
✅ MSAL authentication (same login flow)  
✅ API communication (same endpoints)  

### What's Different

**Razor Syntax** - Instead of pure HTML:

```html
<!-- Old: static HTML -->
<title>AI Multi-Agent Coordinator</title>

<!-- New: Razor syntax -->
<title>@ViewData["Title"]</title>
```

**Server-Side Rendering** - Better performance and SEO

**ASP.NET Core Integration** - Can add server-side logic easily

### Why Razor Pages?

1. **Better Integration**: Native ASP.NET Core, not "static files in an ASP.NET app"
2. **Flexibility**: Can add server-side logic without API calls
3. **Type Safety**: Compile-time checking of page models
4. **Still No Blazor**: As requested - no WebAssembly, just server-rendered pages
5. **JavaScript Still Works**: All client-side code preserved

### How to Use

**Navigate to:**
```
http://localhost:5001/
```

Razor Pages automatically routes `/` to `Pages/Index.cshtml`

**Adding New Pages:**
```bash
# Create a new Razor page
cd src/WebChat/Pages
# Add About.cshtml and About.cshtml.cs
# Access at: http://localhost:5001/About
```

## 3. Updated Application Flow

### Complete Authentication Flow

```
1. User Opens http://localhost:5001
   ↓
2. Razor Page Renders (Index.cshtml)
   ↓
3. JavaScript Checks for Existing Token
   ↓
4. If No Token → Shows Login Screen
   ↓
5. User Clicks "Login with Microsoft"
   ↓
6. Device Code Flow Initiated
   ↓
7. User Authenticates with Azure AD
   ↓
8. Access Token Returned
   ↓
9. Token Stored in localStorage
   ↓
10. Chat Interface Enabled
   ↓
11. User Sends Message
   ↓
12. JavaScript Adds "Authorization: Bearer {token}" Header
   ↓
13. WebAPI Validates Token
   ↓
14. Agent Processes Request
   ↓
15. MCP Service Uses SAME Token for External Calls
   ↓
16. Response Returns to User
```

## 4. Configuration Changes Summary

### appsettings.json (WebAPI)

```json
{
  "AzureAd": {
    "ClientId": "your-client-id",
    "Authority": "https://login.microsoftonline.com/your-tenant-id"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-openai.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4"
  },
  "Mcp": {
    "CmdbEndpoint": "https://cloud.sps.nl/mcp/cmdb/",
    "CrmEndpoint": "https://cloud.sps.nl/mcp/crm/",
    "ServiceDeskEndpoint": "https://cloud.sps.nl/mcp/servicedesk/"
  }
}
```

### What's Required

✅ Azure AD credentials (ClientId, Authority)  
✅ Azure OpenAI credentials (Endpoint, ApiKey, DeploymentName)  
✅ MCP endpoint URLs  
❌ NO separate MCP authentication needed  

## 5. Benefits of These Changes

### Simplified Configuration

**Before:** 2 authentication systems to configure  
**After:** 1 authentication system (MSAL)

### Better Security

- Single source of truth for authentication
- User's token is used throughout
- No static tokens in configuration
- Token expiration handled automatically

### More Maintainable

- Razor Pages integrate better with ASP.NET Core
- Server-side logic easier to add
- Type-safe page models
- Better tooling support

### User Experience

- Same login experience
- Same chat interface
- Faster page loads (server rendering)
- No visual changes

## 6. Migration Checklist

If you're updating from the previous version:

- [ ] Pull latest code
- [ ] Update `appsettings.json` - Remove MCP `AuthHeaderName` and `AuthHeaderValue`
- [ ] Update `appsettings.example.json` if you customized it
- [ ] Build solution: `dotnet build`
- [ ] Test WebAPI: `cd src/WebAPI && dotnet run`
- [ ] Test WebChat: `cd src/WebChat && dotnet run`
- [ ] Open browser: `http://localhost:5001`
- [ ] Verify login works
- [ ] Test chat functionality
- [ ] Verify MCP calls work with your token

## 7. Troubleshooting

### "Page not found" when accessing WebChat

**Problem:** Old browser cache pointing to `index.html`  
**Solution:** Clear browser cache or hard refresh (Ctrl+F5)

### MCP calls returning 401 Unauthorized

**Problem:** Token not being passed correctly  
**Solution:** 
1. Check browser console for token in request headers
2. Verify Azure AD app has required API permissions
3. Check MCP endpoints accept Bearer tokens

### Razor Page not rendering

**Problem:** Missing Razor Pages service registration  
**Solution:** Verify `Program.cs` has:
```csharp
builder.Services.AddRazorPages();
app.MapRazorPages();
```

## 8. What's Next

### Future Enhancements

With Razor Pages, you can now easily add:

1. **Server-side user info** - Display user details from token claims
2. **Configuration UI** - Manage settings without editing JSON
3. **Session history** - Show previous conversations
4. **Admin pages** - Manage users, view logs, etc.
5. **Partial views** - Reusable components

### Without Blazor

All of this can be done with:
- Razor Pages (server-side)
- JavaScript (client-side)
- Traditional HTML/CSS
- No WebAssembly required

## Summary

✅ **Simpler**: One authentication token for everything  
✅ **More Secure**: User token used throughout  
✅ **Better Architecture**: Razor Pages instead of static files  
✅ **No Breaking Changes**: Same user experience  
✅ **Still No Blazor**: As requested  

---

**Last Updated:** February 12, 2026  
**Version:** 3.0.0 (Unified Auth + Razor Pages)
