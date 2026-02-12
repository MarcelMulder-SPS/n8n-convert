# WebChat UI Preview

Since the application requires Azure OpenAI credentials to run, here's a description of what the chat interface looks like:

## Chat Interface Features

### Header Section
- **Title**: "🤖 AI Multi-Agent Coordinator"
- **Subtitle**: "Intelligent routing across CMDB, CRM, and ServiceDesk agents"
- **Design**: Gradient background (purple to violet)

### Welcome Message
When you first open the application, you'll see:

```
Welcome to the Multi-Agent Coordinator

I can help you with:

📊 CMDB - Configuration Items, infrastructure, and IT assets
👥 CRM - Organizations, employees, users, and contacts
🎫 ServiceDesk - Tickets, incidents, and service requests

Ask me anything!
```

### Chat Area
- **Layout**: Scrollable message container
- **User Messages**: Right-aligned, purple gradient background, white text
- **AI Messages**: Left-aligned, white background, dark text with shadow
- **Animations**: Messages slide in smoothly when added

### Input Section
- **Text Input**: Large rounded input field with placeholder "Type your message here..."
- **Send Button**: Purple gradient button with hover effects
- **Status Bar**: Shows "Ready", "Agent is thinking...", or error messages

### Visual Design
- **Color Scheme**: Purple (#667eea) to Violet (#764ba2) gradient
- **Typography**: Modern sans-serif (Segoe UI, Roboto)
- **Spacing**: Clean, generous padding and margins
- **Responsiveness**: Adapts to mobile and desktop screens

### Example Conversation Flow

```
USER: Show me all servers in production

🤖 Typing indicator appears (animated dots)

ASSISTANT: I'll search the CMDB for production servers.

Found 5 servers:
1. srv-prod-web-01 (Web Server)
2. srv-prod-db-01 (Database Server)
3. srv-prod-app-01 (Application Server)
4. vsrv-prod-cache-01 (Virtual Cache Server)
5. vsrv-prod-api-01 (Virtual API Server)

Would you like more details about any of these servers?
```

## UI Elements

### Buttons
- **Send Button**: Gradient background, transforms on hover
- **Clear Session**: Red button in status bar

### Status Indicators
- **Ready**: Gray text
- **Thinking**: Purple bold text
- **Error**: Red bold text

### Typing Indicator
Animated three dots that bounce in sequence while the AI is processing.

## Browser Support
- ✅ Chrome/Edge (Recommended)
- ✅ Firefox
- ✅ Safari
- ✅ Mobile browsers

## Accessibility
- Keyboard navigation (Tab, Enter)
- Screen reader friendly
- High contrast ratios
- Clear focus indicators

## To See It Live
1. Configure Azure OpenAI credentials
2. Run `./start.sh` or `start.bat`
3. Open http://localhost:5001 in your browser

## Screenshot Locations
Once running, take screenshots and save them to:
- `docs/screenshots/` directory (create as needed)

## Customization
To change colors, edit: `src/WebChat/wwwroot/css/style.css`

Key CSS variables you might want to customize:
```css
/* Main gradient */
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);

/* User message color */
.message.user { background: #667eea; }

/* Hover effects */
#sendButton:hover { box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4); }
```
