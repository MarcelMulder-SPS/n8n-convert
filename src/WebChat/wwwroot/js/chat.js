// Configuration
const API_BASE_URL = 'http://localhost:5000/api';
const WEBCHAT_API_URL = 'http://localhost:5001/api';
let sessionId = '';
let accessToken = '';
let msalConfig = null;

// DOM Elements
const chatMessages = document.getElementById('chatMessages');
const messageInput = document.getElementById('messageInput');
const sendButton = document.getElementById('sendButton');
const clearButton = document.getElementById('clearButton');
const statusText = document.getElementById('statusText');
const loginButton = document.getElementById('loginButton');
const loginSection = document.getElementById('loginSection');

// Initialize
document.addEventListener('DOMContentLoaded', async () => {
    messageInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            sendMessage();
        }
    });

    sendButton.addEventListener('click', sendMessage);
    clearButton.addEventListener('click', clearSession);
    
    if (loginButton) {
        loginButton.addEventListener('click', login);
    }

    // Load MSAL config and check for existing token
    await loadMsalConfig();
    await checkAuthentication();
});

// Load MSAL configuration from server
async function loadMsalConfig() {
    try {
        const response = await fetch(`${WEBCHAT_API_URL}/auth/config`);
        if (response.ok) {
            msalConfig = await response.json();
        }
    } catch (error) {
        console.error('Error loading MSAL config:', error);
    }
}

// Check if user is already authenticated
async function checkAuthentication() {
    const storedToken = localStorage.getItem('accessToken');
    const tokenExpiry = localStorage.getItem('tokenExpiry');
    
    if (storedToken && tokenExpiry) {
        const expiryDate = new Date(tokenExpiry);
        if (expiryDate > new Date()) {
            accessToken = storedToken;
            showChatInterface();
            updateStatus('Authenticated');
            return;
        }
    }
    
    // Try to acquire token silently
    try {
        const accounts = await getAccounts();
        if (accounts && accounts.length > 0) {
            await acquireTokenSilent(accounts[0].homeAccountId.identifier);
            return;
        }
    } catch (error) {
        console.log('No existing authentication found');
    }
    
    showLoginInterface();
}

// Get accounts from server
async function getAccounts() {
    try {
        const response = await fetch(`${WEBCHAT_API_URL}/auth/accounts`);
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('Error getting accounts:', error);
    }
    return null;
}

// Acquire token silently
async function acquireTokenSilent(accountId) {
    try {
        const response = await fetch(`${WEBCHAT_API_URL}/auth/acquire-token-silent`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ accountId: accountId })
        });
        
        if (response.ok) {
            const data = await response.json();
            accessToken = data.accessToken;
            localStorage.setItem('accessToken', data.accessToken);
            localStorage.setItem('tokenExpiry', data.expiresOn);
            showChatInterface();
            updateStatus('Authenticated');
        } else {
            showLoginInterface();
        }
    } catch (error) {
        console.error('Error acquiring token silently:', error);
        showLoginInterface();
    }
}

// Login with device code flow
async function login() {
    updateStatus('Logging in...', 'thinking');
    
    try {
        const response = await fetch(`${WEBCHAT_API_URL}/auth/device-code`, {
            method: 'POST'
        });
        
        if (response.ok) {
            const data = await response.json();
            accessToken = data.accessToken;
            localStorage.setItem('accessToken', data.accessToken);
            localStorage.setItem('tokenExpiry', data.expiresOn);
            showChatInterface();
            updateStatus('Authenticated');
        } else {
            const error = await response.json();
            updateStatus(`Login failed: ${error.error}`, 'error');
        }
    } catch (error) {
        console.error('Error during login:', error);
        updateStatus(`Login error: ${error.message}`, 'error');
    }
}

// Show login interface
function showLoginInterface() {
    if (loginSection) {
        loginSection.style.display = 'block';
    }
    messageInput.disabled = true;
    sendButton.disabled = true;
}

// Show chat interface
function showChatInterface() {
    if (loginSection) {
        loginSection.style.display = 'none';
    }
    messageInput.disabled = false;
    sendButton.disabled = false;
}

// Add message to chat
function addMessage(content, role) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${role}`;
    
    const contentDiv = document.createElement('div');
    contentDiv.className = 'message-content';
    contentDiv.textContent = content;
    
    messageDiv.appendChild(contentDiv);
    chatMessages.appendChild(messageDiv);
    
    // Scroll to bottom
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

// Show typing indicator
function showTypingIndicator() {
    const typingDiv = document.createElement('div');
    typingDiv.className = 'typing-indicator';
    typingDiv.id = 'typingIndicator';
    typingDiv.innerHTML = '<span></span><span></span><span></span>';
    chatMessages.appendChild(typingDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

// Remove typing indicator
function removeTypingIndicator() {
    const typingIndicator = document.getElementById('typingIndicator');
    if (typingIndicator) {
        typingIndicator.remove();
    }
}

// Update status
function updateStatus(message, type = '') {
    statusText.textContent = message;
    statusText.className = type;
}

// Send message
async function sendMessage() {
    const message = messageInput.value.trim();
    
    if (!message) {
        return;
    }

    if (!accessToken) {
        addMessage('Please login first', 'error');
        return;
    }

    // Clear input
    messageInput.value = '';
    
    // Add user message
    addMessage(message, 'user');
    
    // Disable input
    sendButton.disabled = true;
    messageInput.disabled = true;
    
    // Show typing indicator
    showTypingIndicator();
    updateStatus('Agent is thinking...', 'thinking');

    try {
        const response = await fetch(`${API_BASE_URL}/chat`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            },
            body: JSON.stringify({
                message: message,
                sessionId: sessionId
            })
        });

        if (response.status === 401) {
            // Token expired, need to re-authenticate
            removeTypingIndicator();
            addMessage('Session expired. Please login again.', 'error');
            localStorage.removeItem('accessToken');
            localStorage.removeItem('tokenExpiry');
            accessToken = '';
            showLoginInterface();
            updateStatus('Authentication required', 'error');
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        // Update session ID
        if (data.sessionId) {
            sessionId = data.sessionId;
        }

        // Remove typing indicator
        removeTypingIndicator();

        // Add assistant response
        addMessage(data.message, 'assistant');
        updateStatus('Ready');

    } catch (error) {
        console.error('Error:', error);
        removeTypingIndicator();
        addMessage(`Error: ${error.message}. Please check if the API server is running.`, 'error');
        updateStatus('Error occurred', 'error');
    } finally {
        // Re-enable input
        sendButton.disabled = false;
        messageInput.disabled = false;
        messageInput.focus();
    }
}

// Clear session
async function clearSession() {
    if (!sessionId) {
        // Just clear the UI
        chatMessages.innerHTML = '';
        addWelcomeMessage();
        updateStatus('Session cleared');
        return;
    }

    try {
        await fetch(`${API_BASE_URL}/chat/session/${sessionId}`, {
            method: 'DELETE'
        });

        // Clear session ID
        sessionId = '';

        // Clear messages
        chatMessages.innerHTML = '';
        addWelcomeMessage();

        updateStatus('Session cleared');
    } catch (error) {
        console.error('Error clearing session:', error);
        updateStatus('Error clearing session', 'error');
    }
}

// Add welcome message
function addWelcomeMessage() {
    chatMessages.innerHTML = `
        <div class="welcome-message">
            <h2>Welcome to the Multi-Agent Coordinator</h2>
            <p>I can help you with:</p>
            <ul>
                <li>📊 <strong>CMDB</strong> - Configuration Items, infrastructure, and IT assets</li>
                <li>👥 <strong>CRM</strong> - Organizations, employees, users, and contacts</li>
                <li>🎫 <strong>ServiceDesk</strong> - Tickets, incidents, and service requests</li>
            </ul>
            <p>Ask me anything!</p>
        </div>
    `;
}
