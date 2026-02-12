#!/bin/bash

# Quick Start Script for N8N Agent Framework

echo "========================================="
echo "N8N Agent Framework - Quick Start"
echo "========================================="
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null
then
    echo "❌ .NET SDK is not installed"
    echo "Please install .NET 10 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✓ .NET SDK detected: $(dotnet --version)"
echo ""

# Build the solution
echo "Building the solution..."
dotnet build
if [ $? -ne 0 ]; then
    echo "❌ Build failed. Please check the error messages above."
    exit 1
fi
echo "✓ Build successful"
echo ""

# Check if appsettings.json is configured
if grep -q "your-api-key-here" src/WebAPI/appsettings.json; then
    echo "⚠️  WARNING: appsettings.json still contains placeholder values"
    echo "Please update src/WebAPI/appsettings.json with your Azure OpenAI credentials"
    echo ""
    read -p "Continue anyway? (y/n) " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]
    then
        exit 1
    fi
fi

echo "Starting the application..."
echo ""
echo "WebAPI will start on: http://localhost:5000"
echo "WebChat will start on: http://localhost:5001"
echo ""
echo "Press Ctrl+C to stop all services"
echo ""

# Function to cleanup background processes on exit
cleanup() {
    echo ""
    echo "Stopping services..."
    kill $API_PID 2>/dev/null
    kill $CHAT_PID 2>/dev/null
    exit 0
}

trap cleanup SIGINT SIGTERM

# Start WebAPI in background
echo "Starting WebAPI..."
cd src/WebAPI
dotnet run > ../../logs/webapi.log 2>&1 &
API_PID=$!
cd ../..

# Wait a bit for API to start
sleep 3

# Start WebChat in background
echo "Starting WebChat..."
cd src/WebChat
dotnet run > ../../logs/webchat.log 2>&1 &
CHAT_PID=$!
cd ../..

# Wait a bit for WebChat to start
sleep 3

echo ""
echo "========================================="
echo "✓ Services started successfully!"
echo "========================================="
echo ""
echo "WebAPI PID: $API_PID"
echo "WebChat PID: $CHAT_PID"
echo ""
echo "🌐 Open your browser and navigate to:"
echo "   http://localhost:5001"
echo ""
echo "📋 Logs are available in:"
echo "   - logs/webapi.log"
echo "   - logs/webchat.log"
echo ""
echo "Press Ctrl+C to stop all services"
echo ""

# Wait for user to interrupt
wait
