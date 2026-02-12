@echo off
REM Quick Start Script for N8N Agent Framework (Windows)

echo =========================================
echo N8N Agent Framework - Quick Start
echo =========================================
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo X .NET SDK is not installed
    echo Please install .NET 10 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo + .NET SDK detected
dotnet --version
echo.

REM Build the solution
echo Building the solution...
dotnet build
if errorlevel 1 (
    echo X Build failed. Please check the error messages above.
    pause
    exit /b 1
)
echo + Build successful
echo.

REM Check if appsettings.json is configured
findstr /C:"your-api-key-here" src\WebAPI\appsettings.json >nul 2>&1
if not errorlevel 1 (
    echo ! WARNING: appsettings.json still contains placeholder values
    echo Please update src\WebAPI\appsettings.json with your Azure OpenAI credentials
    echo.
    set /p CONTINUE=Continue anyway? (y/n): 
    if /i not "%CONTINUE%"=="y" exit /b 1
)

echo Starting the application...
echo.
echo WebAPI will start on: http://localhost:5000
echo WebChat will start on: http://localhost:5001
echo.
echo Press Ctrl+C to stop all services
echo.

REM Create logs directory if it doesn't exist
if not exist logs mkdir logs

REM Start WebAPI
echo Starting WebAPI...
start "WebAPI" cmd /c "cd src\WebAPI && dotnet run > ..\..\logs\webapi.log 2>&1"

REM Wait for API to start
timeout /t 3 /nobreak >nul

REM Start WebChat
echo Starting WebChat...
start "WebChat" cmd /c "cd src\WebChat && dotnet run > ..\..\logs\webchat.log 2>&1"

REM Wait for WebChat to start
timeout /t 3 /nobreak >nul

echo.
echo =========================================
echo + Services started successfully!
echo =========================================
echo.
echo Services are running in separate windows
echo.
echo   Open your browser and navigate to:
echo   http://localhost:5001
echo.
echo   Logs are available in:
echo   - logs\webapi.log
echo   - logs\webchat.log
echo.
echo Close the WebAPI and WebChat windows to stop the services
echo.
pause
