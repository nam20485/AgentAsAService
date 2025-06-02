#!/bin/bash

# Firebase Deployment Script for OrchestratorWebApp
# This script builds and deploys the Blazor WebAssembly app to Firebase Hosting

set -e

# Configuration
PROJECT_ID="agent-as-a-service-459620"
BUILD_CONFIGURATION="Release"
WEBAPP_PROJECT="OrchestratorWebApp"

echo "🚀 Starting Firebase deployment for $WEBAPP_PROJECT..."

# Ensure we're in the project root
cd "$(dirname "$0")/.."

# Set Firebase project
echo "📋 Setting Firebase project to $PROJECT_ID..."
firebase use $PROJECT_ID

# Build the Blazor WebAssembly app
echo "🔨 Building $WEBAPP_PROJECT in $BUILD_CONFIGURATION mode..."
dotnet publish $WEBAPP_PROJECT/$WEBAPP_PROJECT.csproj \
    --configuration $BUILD_CONFIGURATION \
    --output $WEBAPP_PROJECT/publish \
    --verbosity minimal

# Verify build output
if [ ! -d "$WEBAPP_PROJECT/publish/wwwroot" ]; then
    echo "❌ Error: Build output directory not found at $WEBAPP_PROJECT/publish/wwwroot"
    exit 1
fi

echo "✅ Build completed successfully"

# Copy production configuration
echo "📄 Copying production configuration..."
if [ -f "$WEBAPP_PROJECT/wwwroot/appsettings.Production.json" ]; then
    cp "$WEBAPP_PROJECT/wwwroot/appsettings.Production.json" "$WEBAPP_PROJECT/publish/wwwroot/appsettings.json"
    echo "✅ Production configuration applied"
else
    echo "⚠️  Warning: No production configuration found, using default"
fi

# Deploy to Firebase Hosting
echo "🌐 Deploying to Firebase Hosting..."
firebase deploy --only hosting

echo ""
echo "✅ Deployment completed successfully!"
echo ""
echo "🔗 Your app is available at:"
echo "   https://$PROJECT_ID.web.app"
echo "   https://$PROJECT_ID.firebaseapp.com"
echo ""
echo "📋 Next steps:"
echo "1. Test the authentication flow"
echo "2. Verify API connectivity to OrchestratorService"
echo "3. Check browser developer tools for any errors"
