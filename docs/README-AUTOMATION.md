# AgentAsAService - Quick Start

This document explains how to use the automated startup options for the AgentAsAService project.

### PowerShell Script (Recommended)

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

### VS Code Debug (F5)

- **🚀 Launch AgentAsAService (Full Stack)** - Full development setup
- **🔧 Debug OrchestratorService Only** - API backend only
- **🌐 Debug OrchestratorWebApp Only** - Frontend only

## Service URLs

| Service | URL | Purpose |
|---------|-----|---------|
| Web App | <http://localhost:5264> | Main interface |
| API | <http://localhost:8080> | REST endpoints |
| Swagger | <http://localhost:8080/swagger> | API docs |
| Health | <http://localhost:8080/health> | Status check |

## Manual Start

```powershell
# Terminal 1
cd OrchestratorService && dotnet run

# Terminal 2  
cd OrchestratorWebApp && dotnet run
```

## Troubleshooting

### Port Conflicts

```powershell
netstat -ano | findstr ":8080 \|:5264"
taskkill /F /PID <PID>
```

### Startup Issues

- Check .NET version: `dotnet --version`
- Build first: `dotnet build AgentAsAService.sln`
- Verify local-storage directory exists

## Development Workflow

```powershell
.\start-services.ps1    # Start everything
# Code, test, iterate
# Ctrl+C to stop
```

**Features:**

- One-command startup
- Health checks
- Auto-browser launch
- Clean shutdown
- VS Code integration
