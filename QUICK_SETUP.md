# Quick Setup Checklist

## ✅ Pre-Setup Requirements

- [ ] .NET 10 SDK installed
- [ ] Azure subscription
- [ ] Access to Azure OpenAI
- [ ] Azure AD tenant

## 🔧 Azure AD App Registration

### Step 1: Create App Registration

1. Go to Azure Portal → Azure Active Directory → App registrations
2. Click "New registration"
3. Enter name: `N8nAgentFramework-Client`
4. Supported account types: "Accounts in this organizational directory only"
5. Click "Register"

### Step 2: Configure Authentication

1. Go to "Authentication" section
2. Add platform → "Mobile and desktop applications"
3. Redirect URI: `http://localhost`
4. Enable "Public client flows": **Yes**
5. Save

### Step 3: API Permissions

1. Go to "API permissions"
2. Add permission → Microsoft Graph → Delegated
3. Select: `User.Read`
4. Grant admin consent

### Step 4: Copy Configuration Values

- [ ] Copy **Application (client) ID** → This is your `ClientId`
- [ ] Copy **Directory (tenant) ID** → Use in `Authority` URL
- [ ] Authority format: `https://login.microsoftonline.com/{tenant-id}`

## 📝 Configuration Files

### WebAPI appsettings.json

```bash
cd src/WebAPI
nano appsettings.json
```

Update:
```json
{
  "AzureAd": {
    "ClientId": "paste-your-client-id-here",
    "Authority": "https://login.microsoftonline.com/paste-your-tenant-id-here"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-openai-resource.openai.azure.com/",
    "ApiKey": "your-openai-api-key",
    "DeploymentName": "gpt-4"
  },
  "Mcp": {
    "CmdbEndpoint": "https://cloud.sps.nl/mcp/cmdb/",
    "CrmEndpoint": "https://cloud.sps.nl/mcp/crm/",
    "ServiceDeskEndpoint": "https://cloud.sps.nl/mcp/servicedesk/"
  }
}
```

### WebChat appsettings.json

```bash
cd src/WebChat
nano appsettings.json
```

Update:
```json
{
  "AzureAd": {
    "ClientId": "paste-your-client-id-here",
    "Authority": "https://login.microsoftonline.com/paste-your-tenant-id-here",
    "RedirectUri": "http://localhost:5001",
    "Scopes": [
      "user.read"
    ]
  }
}
```

## 🚀 Running the Application

### Terminal 1 - WebAPI

```bash
cd src/WebAPI
dotnet run
```

Expected output:
```
info: Now listening on: http://localhost:5000
```

### Terminal 2 - WebChat

```bash
cd src/WebChat
dotnet run
```

Expected output:
```
info: Now listening on: http://localhost:5001
```

### Access the Application

1. Open browser: `http://localhost:5001`
2. You should see login prompt
3. Click "Login with Microsoft"
4. Complete authentication
5. Start chatting!

## 🧪 Testing MCP Integration

Once authenticated, try these queries:

### CMDB Queries
```
Show me all servers
List configuration items
What CI classes are available?
```

### CRM Queries
```
Find all employees
Show organizations
List support groups
```

### ServiceDesk Queries
```
Show open tickets
List incidents
```

## ❗ Troubleshooting

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Authentication Fails

- [ ] Verify ClientId is correct
- [ ] Verify TenantId is correct
- [ ] Ensure Public client flows enabled in Azure AD
- [ ] Check browser console for errors

### MCP Calls Fail

- [ ] Check MCP endpoints are accessible
- [ ] Verify Bearer token is being sent
- [ ] Review WebAPI logs for detailed errors

### CORS Errors

- [ ] Verify WebChat is running on port 5001
- [ ] Check CORS configuration in WebAPI Program.cs

## 📚 Additional Documentation

- [MSAL_MCP_GUIDE.md](./MSAL_MCP_GUIDE.md) - Complete authentication guide
- [README.md](./README.md) - Full project documentation
- [SETUP.md](./SETUP.md) - Detailed setup instructions

## ✨ Next Steps

After successful setup:

1. Customize agent system messages
2. Add more MCP tool calls
3. Enhance UI with additional features
4. Deploy to production environment

## 🔒 Security Checklist

Before production deployment:

- [ ] Move secrets to Azure Key Vault
- [ ] Enable HTTPS
- [ ] Configure proper CORS policy
- [ ] Add rate limiting
- [ ] Enable Application Insights
- [ ] Set up proper logging
- [ ] Review and update token lifetimes
- [ ] Implement token revocation handling

## 📞 Need Help?

- Check [MSAL_MCP_GUIDE.md](./MSAL_MCP_GUIDE.md) troubleshooting section
- Review application logs in console
- Open a GitHub issue with details

---

**Last Updated:** February 12, 2026  
**Version:** 2.0.0 (with MSAL + MCP)
