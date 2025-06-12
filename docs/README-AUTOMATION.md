# AgentAsAService - Automated Launch Guide

This document explains how to use the automated startup options for the AgentAsAService project.

## 🚀 **Quick Start Options**

### Option 1: PowerShell Script (Recommended)
**Easiest way to start everything:**

```powershell
.\start-services.ps1
```

**What it does:**
- ✅ Checks if .NET SDK is installed
- ✅ Stops any existing processes on ports 8080/5264
- ✅ Creates the required local-storage directory
- ✅ Starts OrchestratorService (API) on port 8080
- ✅ Waits for API to be ready with health check
- ✅ Starts OrchestratorWebApp on port 5264
- ✅ Opens browser windows automatically:
  - Web App: http://localhost:5264
  - Swagger API: http://localhost:8080/swagger
- ✅ Monitors services and handles Ctrl+C cleanup

**Script Options:**
```powershell
.\start-services.ps1 -NoBrowser    # Start without opening browsers
.\start-services.ps1 -Help         # Show help information
```

### Option 2: VS Code Tasks
**Run via Command Palette (`Ctrl+Shift+P`):**

1. **Tasks: Run Task** → **"Start AgentAsAService (Full Stack)"**
   - Starts both services sequentially
   - Opens browsers automatically
   - Shows output in VS Code terminal

2. **Tasks: Run Task** → **"Run Startup Script"**
   - Runs the PowerShell script within VS Code

3. **Tasks: Run Task** → **"Stop All Services"**
   - Stops all running services cleanly

### Option 3: VS Code Launch Configuration
**Use the Debug panel (F5):**

1. **🚀 Launch AgentAsAService (Full Stack)**
   - Comprehensive startup with browser launch
   - Best for development work

2. **🚀 Debug Full Stack (Manual)**
   - Starts both services in debug mode
   - Allows breakpoint debugging

3. **🔧 Debug OrchestratorService Only**
   - Just the API service for backend development

4. **🌐 Debug OrchestratorWebApp Only**
   - Just the web app for frontend development

## 📋 **Manual Start (Traditional)**

If you prefer manual control:

**Terminal 1:**
```powershell
cd OrchestratorService
dotnet run
```

**Terminal 2:**
```powershell
cd OrchestratorWebApp
dotnet run
```

**Open browsers manually:**
- Web App: http://localhost:5264
- API Swagger: http://localhost:8080/swagger

## 🛠️ **Service URLs**

| Service | URL | Description |
|---------|-----|-------------|
| **Web App** | http://localhost:5264 | Main application interface |
| **API** | http://localhost:8080 | REST API endpoints |
| **Swagger** | http://localhost:8080/swagger | API documentation |
| **Health Check** | http://localhost:8080/health | Service health status |

## 🔧 **Troubleshooting**

### Port Already in Use
The startup script automatically handles this, but if needed:
```powershell
# Check what's using the ports
netstat -ano | findstr ":8080 \|:5264 "

# Kill specific process (replace PID)
taskkill /F /PID <PID_NUMBER>
```

### Services Won't Start
1. Ensure .NET 9.0 SDK is installed: `dotnet --version`
2. Build the solution first: `dotnet build AgentAsAService.sln`
3. Check the local-storage directory exists: `OrchestratorService\local-storage\`

### Browser Doesn't Open
- Run with `-NoBrowser` flag and open manually
- Check Windows firewall/antivirus settings
- Verify URLs are accessible: http://localhost:5264

## 📁 **Project Structure**

```
AgentAsAService/
├── start-services.ps1              # 🆕 Automated startup script
├── .vscode/
│   ├── tasks.json                  # 🆕 Enhanced with automation tasks
│   └── launch.json                 # 🆕 Debug configurations
├── OrchestratorService/            # Backend API
│   └── local-storage/              # 🆕 Auto-created database directory
├── OrchestratorWebApp/             # Frontend web app
└── README-AUTOMATION.md            # This file
```

## 🎯 **Development Workflow**

### For Full-Stack Development:
```powershell
.\start-services.ps1
# Both services start, browsers open automatically
# Code, test, iterate...
# Press Ctrl+C when done
```

### For API-Only Development:
Use VS Code: **🔧 Debug OrchestratorService Only**

### For Frontend-Only Development:
1. Ensure API is running (port 8080)
2. Use VS Code: **🌐 Debug OrchestratorWebApp Only**

## ✨ **Features**

- 🚀 **One-command startup** with full automation
- 🔍 **Health checks** ensure services are ready
- 🌐 **Auto-browser launch** to the right URLs
- 🛑 **Clean shutdown** with Ctrl+C handling
- 📊 **Service monitoring** with status updates
- 🔧 **VS Code integration** with tasks and launch configs
- 🎨 **Color-coded output** for easy reading
- ⚡ **Fast restart** capabilities

**Ready to develop! 🎉**
