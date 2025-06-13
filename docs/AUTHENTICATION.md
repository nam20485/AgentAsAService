# Local Development Authentication Guide

This guide explains how to authenticate with your services when testing locally with Swagger.

## Quick Start (Development Mode)

**For the easiest local testing experience**, the services are now configured to bypass authentication in development mode. Simply:

1. Run your services in Development environment:
   ```powershell
   dotnet run --environment Development
   ```

2. Open Swagger UI (e.g., `https://localhost:7001/swagger`)

3. Test endpoints directly - no authentication required! 🎉

## Production-like Testing (With Authentication)

If you want to test with actual authentication (similar to production), you have several options:

### Option 1: Using Google Cloud CLI Token

1. Install and authenticate with Google Cloud CLI:
   ```powershell
   gcloud auth login
   gcloud config set project YOUR_PROJECT_ID
   ```

2. Get an access token:
   ```powershell
   .\scripts\get-auth-token.ps1 -Method gcloud -ProjectId "your-project-id"
   ```

3. Use the token in Swagger:
   - Click the 🔒 "Authorize" button in Swagger UI
   - Paste the token (including "Bearer " prefix)
   - Click "Authorize"

### Option 2: Using Firebase CLI Token

1. Install and authenticate with Firebase CLI:
   ```powershell
   npm install -g firebase-tools
   firebase login
   ```

2. Get a Firebase ID token:
   ```powershell
   .\scripts\get-auth-token.ps1 -Method firebase -ProjectId "your-project-id"
   ```

3. Use the token in Swagger as described above

### Option 3: Using Service Account Impersonation

1. Set up service account impersonation:
   ```powershell
   gcloud auth login
   gcloud config set project YOUR_PROJECT_ID
   ```

2. Get a service account token:
   ```powershell
   .\scripts\get-auth-token.ps1 -Method service-account -ProjectId "your-project-id" -ServiceAccountEmail "your-service@your-project.iam.gserviceaccount.com"
   ```

3. Use the token in Swagger as described above

## Environment Configuration

### Development Mode (No Auth)
- Set `ASPNETCORE_ENVIRONMENT=Development`
- Authentication is bypassed automatically
- Perfect for rapid development and testing

### Production Mode (Full Auth)
- Set `ASPNETCORE_ENVIRONMENT=Production` 
- Full JWT authentication is enforced
- Requires valid tokens as described above

## Troubleshooting

### "401 Unauthorized" Error
- Make sure you're using a valid token
- Check that the token hasn't expired (tokens typically last 1 hour)
- Verify your project ID is correct
- Ensure you have the right permissions

### "403 Forbidden" Error
- Your token is valid but you don't have permission
- Check that your service account email is in the `AllowedServiceEmails` configuration
- Verify the service account has the necessary roles

### Token Expired
- Get a new token using the scripts above
- Tokens typically expire after 1 hour

### Can't Get Token
- Make sure you're logged in: `gcloud auth login` or `firebase login`
- Check your internet connection
- Verify your project ID exists and you have access

## Configuration Files

The authentication configuration is in:
- `AgentService/appsettings.json` - Service configuration
- `OrchestratorService/appsettings.json` - Orchestrator configuration

Update the `GoogleCloud:ProjectId` and `AgentService:AllowedServiceEmails` as needed.

## Security Notes

⚠️ **Development Mode Security**: The development mode bypasses authentication for convenience. Never use development mode in production!

🔐 **Token Security**: Never commit tokens to source control. The helper scripts generate fresh tokens as needed.

🕐 **Token Expiration**: Most tokens expire after 1 hour. Get fresh tokens if you get authentication errors.
