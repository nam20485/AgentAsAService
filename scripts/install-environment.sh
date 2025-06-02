#! /bin/sh

# This script is used to install the environment for the project.
# It will install the required packages and set up the environment variables.

#
# nvm (node & npm)
#

export NVM_DIR="$HOME/.nvm"
# Try to source nvm if it exists
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"

if ! command -v nvm >/dev/null 2>&1; then
  sudo apt install curl -y
  curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash
  # Source nvm after installation
  [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"
fi

if command -v nvm >/dev/null 2>&1; then
  if ! nvm ls --no-colors | grep -q "v$(nvm version-remote --lts)"; then
    nvm install --lts
  fi
  nvm use --lts
  node -v
  npm -v
else
  echo "nvm installation failed or nvm not found in PATH."
  exit 1
fi

#
# Firebase CLI (v14.3.1)
#
REQUIRED_FIREBASE_VERSION="14.3.1"
INSTALLED_FIREBASE_VERSION=$(firebase --version 2>/dev/null || echo "")

if [ "$INSTALLED_FIREBASE_VERSION" != "$REQUIRED_FIREBASE_VERSION" ]; then
  echo "Installing Firebase CLI version $REQUIRED_FIREBASE_VERSION..."
  npm install -g firebase-tools@$REQUIRED_FIREBASE_VERSION
else
  echo "Firebase CLI version $REQUIRED_FIREBASE_VERSION already installed."
fi

firebase --version

#
# Google Cloud CLI (gcloud) v521.0.0
#
REQUIRED_GCLOUD_VERSION="521.0.0"
GCLOUD_CMD=$(command -v gcloud || echo "")
INSTALLED_GCLOUD_VERSION=""

if [ -n "$GCLOUD_CMD" ]; then
  # Parse the version from the standard output
  INSTALLED_GCLOUD_VERSION=$("$GCLOUD_CMD" version 2>/dev/null | grep 'Google Cloud SDK' | awk '{print $4}' | tr -d '[:space:]')
fi

echo "GCLOUD_CMD: '$GCLOUD_CMD'"
echo "INSTALLED_GCLOUD_VERSION: '$INSTALLED_GCLOUD_VERSION'"
echo "REQUIRED_GCLOUD_VERSION: '$REQUIRED_GCLOUD_VERSION'"

if [ -z "$GCLOUD_CMD" ] || [ "$INSTALLED_GCLOUD_VERSION" != "$REQUIRED_GCLOUD_VERSION" ]; then
  echo "Installing Google Cloud CLI version $REQUIRED_GCLOUD_VERSION..."
  sudo apt-get update -y && \
    sudo apt-get install -y apt-transport-https ca-certificates gnupg curl
  echo "deb [signed-by=/usr/share/keyrings/cloud.google.gpg] https://packages.cloud.google.com/apt cloud-sdk main" | \
    sudo tee /etc/apt/sources.list.d/google-cloud-sdk.list
  curl https://packages.cloud.google.com/apt/doc/apt-key.gpg | \
    sudo apt-key --keyring /usr/share/keyrings/cloud.google.gpg add -
  sudo apt-get update -y && sudo apt-get install -y google-cloud-cli=521.0.0-0
else
  echo "Google Cloud CLI version $REQUIRED_GCLOUD_VERSION already installed."
fi

gcloud version

#
# GitHub CLI (gh)
#
REQUIRED_GH_VERSION="2.64.0"
GH_CMD=$(command -v gh || echo "")
INSTALLED_GH_VERSION=""

if [ -n "$GH_CMD" ]; then
  # Parse the version from the gh version output
  INSTALLED_GH_VERSION=$("$GH_CMD" version 2>/dev/null | grep 'gh version' | awk '{print $3}' | tr -d '[:space:]')
fi

echo "GH_CMD: '$GH_CMD'"
echo "INSTALLED_GH_VERSION: '$INSTALLED_GH_VERSION'"
echo "REQUIRED_GH_VERSION: '$REQUIRED_GH_VERSION'"

if [ -z "$GH_CMD" ] || [ "$INSTALLED_GH_VERSION" != "$REQUIRED_GH_VERSION" ]; then
  echo "Installing GitHub CLI version $REQUIRED_GH_VERSION..."
  curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg \
  && sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg \
  && echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null \
  && sudo apt update \
  && sudo apt install gh=$REQUIRED_GH_VERSION-1
else
  echo "GitHub CLI version $REQUIRED_GH_VERSION already installed."
fi

gh version

echo "Environment setup completed successfully!"
