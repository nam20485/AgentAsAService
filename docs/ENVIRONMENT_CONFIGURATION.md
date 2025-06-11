# Environment Configuration Guide

## 🌍 **Environment Overview**

The AgentAsAService system supports four distinct environments with specific configurations:

### **DEVELOPMENT** 
- **Purpose**: Local development on developer machines
- **Authentication**: Bypassed for easier testing
- **Storage**: LiteDB (local files)
- **Ports**: Fixed (OrchestratorService: 8080, AgentService: 7001)
- **CORS**: Permissive (localhost origins)

### **TESTING**
- **Purpose**: Automated testing and CI/CD pipelines
- **Authentication**: Bypassed or test tokens
- **Storage**: LiteDB or in-memory
- **Ports**: Dynamic (via PORT env var)
- **CORS**: Test-specific origins

### **STAGING** 
- **Purpose**: Pre-production testing with production-like setup
- **Authentication**: Full Google OAuth (staging credentials)
- **Storage**: Firestore (staging project)
- **Ports**: Dynamic (Cloud Run PORT)
- **CORS**: Staging domain origins

### **PRODUCTION**
- **Purpose**: Live production workloads
- **Authentication**: Full Google OAuth (production credentials)
- **Storage**: Firestore (production project)
- **Ports**: Dynamic (Cloud Run PORT)
- **CORS**: Production domain origins

## 🔧 **Configuration Files Per Environment**

Each service will have environment-specific configuration files:
- `appsettings.json` (base configuration)
- `appsettings.Development.json`
- `appsettings.Testing.json`
- `appsettings.Staging.json`
- `appsettings.Production.json`

## 📋 **Environment Variables**

### Required for all environments:
- `ASPNETCORE_ENVIRONMENT` - Sets the environment name
- `PORT` - Service port (provided by Cloud Run in non-dev environments)

### Authentication (Staging/Production):
- `GoogleCloud:ProjectId` - Google Cloud project ID
- `Authentication:Google:ClientId` - OAuth client ID

### Storage Configuration:
- `DocumentStore:Provider` - Storage provider type
- `DocumentStore:ConnectionString` - Connection details
