# MCP (Model Context Protocol) Integration Guide

## Overview

This application uses the Model Context Protocol (MCP) to communicate with external tool servers for CMDB, CRM, and ServiceDesk operations.

## MCP Implementation

### Transport: Server-Sent Events (SSE) over HTTP

The McpService uses **SSE (Server-Sent Events)** transport for HTTP-based communication with MCP servers, following the MCP specification.

### How It Works

```
Client → HTTP POST with SSE headers → MCP Server
    ↓
JSON-RPC 2.0 Request
{
  "jsonrpc": "2.0",
  "id": "unique-id",
  "method": "tools/call",
  "params": {
    "name": "tool_name",
    "arguments": { ... }
  }
}
    ↓
MCP Server Processes via SSE Transport
    ↓
JSON-RPC 2.0 Response
{
  "result": { "content": "..." }
}
```

### SSE Transport Configuration

The McpService configures the HTTP client for SSE:

```csharp
// Accept SSE stream
httpClient.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("text/event-stream"));

// Disable caching for real-time events
httpClient.DefaultRequestHeaders.CacheControl = 
    new CacheControlHeaderValue { NoCache = true };

// Increase timeout for long-running operations
httpClient.Timeout = TimeSpan.FromMinutes(2);
```

### Authentication

All MCP calls use the **MSAL access token** from user login:

```csharp
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", accessToken);
```

No separate MCP credentials needed in configuration!

## Available Methods

### CMDB Tools
```csharp
await mcpService.CallCmdbToolAsync("get_cis", new Dictionary<string, object>
{
    { "class", "server" },
    { "include_virtual", true }
}, accessToken);
```

### CRM Tools
```csharp
await mcpService.CallCrmToolAsync("get_employees", new Dictionary<string, object>
{
    { "organization_id", "12345" }
}, accessToken);
```

### ServiceDesk Tools
```csharp
await mcpService.CallServiceDeskToolAsync("get_tickets", new Dictionary<string, object>
{
    { "status", "open" },
    { "priority", "high" }
}, accessToken);
```

## MCP Server Requirements

Your MCP servers must:

1. **Support SSE Transport** - Accept Server-Sent Events over HTTP
2. **Implement JSON-RPC 2.0** - Standard protocol for tool calls
3. **Accept Bearer Tokens** - Use the `Authorization: Bearer <token>` header
4. **Expose Tool Endpoints** - Respond to `tools/call` method

### Example MCP Server Response

```json
{
  "jsonrpc": "2.0",
  "id": "request-id-123",
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Found 3 servers: srv-01, srv-02, srv-03"
      }
    ]
  }
}
```

## Configuration

### appsettings.json

```json
{
  "Mcp": {
    "CmdbEndpoint": "https://cloud.sps.nl/mcp/cmdb/",
    "CrmEndpoint": "https://cloud.sps.nl/mcp/crm/",
    "ServiceDeskEndpoint": "https://cloud.sps.nl/mcp/servicedesk/"
  }
}
```

**Note:** No authentication credentials needed - uses MSAL token!

## Error Handling

The McpService handles:

- **Timeouts** - 2-minute timeout for long operations
- **HTTP Errors** - Returns status code and error message
- **JSON-RPC Errors** - Parses error field from response
- **Network Failures** - Catches and logs exceptions

## Logging

All MCP operations are logged:

```
[Information] Calling MCP tool get_cis on cmdb using SSE transport
[Information] MCP tool get_cis call successful
[Error] MCP call failed: 401 - Unauthorized
```

## Model Context Protocol SDK

This implementation is compatible with the ModelContextProtocol NuGet package (v0.8.0-preview.1) but implements the protocol manually for maximum compatibility with SSE transport.

### Why Manual Implementation?

The SDK version 0.8.0-preview.1 may not have all the transport and client methods shown in newer examples. This implementation:

- ✅ **Works with current SDK** - Uses what's actually available
- ✅ **Follows MCP Spec** - Implements JSON-RPC 2.0 correctly
- ✅ **Uses SSE Transport** - Proper headers for Server-Sent Events
- ✅ **Supports Authentication** - Bearer token from MSAL
- ✅ **Production Ready** - Error handling and logging

### Future SDK Updates

When newer SDK versions are available with:
- `HttpClientTransport` class
- `McpClient.CreateAsync()` method
- `client.ListPromptsAsync()` method
- `client.CallToolAsync()` method

The implementation can be updated to use those SDK methods directly.

## Testing

### Test with curl

```bash
curl -X POST https://cloud.sps.nl/mcp/cmdb/ \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -H "Accept: text/event-stream" \
  -d '{
    "jsonrpc": "2.0",
    "id": "test-123",
    "method": "tools/call",
    "params": {
      "name": "get_cis",
      "arguments": { "class": "server" }
    }
  }'
```

### Expected Response

```json
{
  "jsonrpc": "2.0",
  "id": "test-123",
  "result": {
    "content": "Server list: ..."
  }
}
```

## Troubleshooting

### "MCP endpoint not configured"
- Check `appsettings.json` has correct endpoint URLs

### "MCP call failed: 401 - Unauthorized"
- Verify MSAL token is valid
- Check MCP server accepts Bearer tokens
- Ensure token has required scopes

### "MCP call timed out"
- MCP server may be slow or unresponsive
- Increase timeout if needed (currently 2 minutes)

### "Failed to parse MCP response as JSON"
- MCP server may not be following JSON-RPC 2.0 spec
- Check server logs for actual response format

## References

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [JSON-RPC 2.0 Specification](https://www.jsonrpc.org/specification)
- [Server-Sent Events (SSE)](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events)

---

**Last Updated:** February 12, 2026  
**SDK Version:** ModelContextProtocol 0.8.0-preview.1  
**Transport:** SSE over HTTP
