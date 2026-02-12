// Configuration
const API_BASE_URL = 'http://localhost:5000/api';
let sessionId = '';

// DOM Elements
const chatMessages = document.getElementById('chatMessages');
const messageInput = document.getElementById('messageInput');
const sendButton = document.getElementById('sendButton');
const clearButton = document.getElementById('clearButton');
const statusText = document.getElementById('statusText');

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    messageInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            sendMessage();
        }
    });

    sendButton.addEventListener('click', sendMessage);
    clearButton.addEventListener('click', clearSession);
});

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
            },
            body: JSON.stringify({
                message: message,
                sessionId: sessionId
            })
        });

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
