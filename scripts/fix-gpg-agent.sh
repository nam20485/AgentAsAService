#!/bin/bash

echo "Stopping any existing gpg-agent for the current user..."
# Use gpgconf to see if an agent is running and try to kill it
# This can be more reliable than pkill if gpgconf knows about the agent.
if gpg-connect-agent /bye >/dev/null 2>&1; then
    echo "Attempting to stop running agent with gpg-connect-agent..."
    gpg-connect-agent killagent /bye || echo "Failed to stop agent with gpg-connect-agent, or it was already stopped."
else
    echo "No active agent found by gpg-connect-agent. Checking for processes..."
    pkill -u $(whoami) gpg-agent || echo "No gpg-agent process found for current user, or pkill not available."
fi
sleep 1 # Give it a moment to release sockets

echo "Removing stale agent socket files..."
rm -f ${HOME}/.gnupg/S.gpg-agent*

echo "Ensuring GPG directory and permissions..."
mkdir -p "${HOME}/.gnupg"
chmod 700 "${HOME}/.gnupg"
# These chmod commands are fine but gpg-agent usually sets these correctly on creation
find "${HOME}/.gnupg" -type f -exec chmod 600 {} \; >/dev/null 2>&1
find "${HOME}/.gnupg" -type d -exec chmod 700 {} \; >/dev/null 2>&1

echo "Setting GPG_TTY..."
export GPG_TTY=$(tty)

echo "Starting new gpg-agent..."
# We run it in the background. It already creates the sockets as seen in your test.
# No need for eval here as it doesn't output shell commands for it.
gpg-agent --daemon --pinentry-program /usr/bin/pinentry-curses --verbose
# Give it a brief moment to start and create sockets
sleep 0.5

# Define known socket paths
GPG_AGENT_SOCKET_PATH="${HOME}/.gnupg/S.gpg-agent"
SSH_AGENT_SOCKET_PATH="${HOME}/.gnupg/S.gpg-agent.ssh"

# Check if the main agent socket exists and set environment variables
if [ -S "${GPG_AGENT_SOCKET_PATH}" ]; then
    echo "gpg-agent socket found at ${GPG_AGENT_SOCKET_PATH}."
    # GPG_AGENT_INFO format is <socket_path>:<pid>:<protocol_version>
    # We don't easily get PID here. However, GPG tools can often work with just the socket path
    # or find it automatically. Forcing GPG_AGENT_INFO might not even be necessary.
    # If GPG tools are modern, they will find the agent.
    # We can export it for tools that might rely on it.
    # The '0:1' here is a placeholder for pid:protocol_version; GPG often ignores these if the socket is primary.
    export GPG_AGENT_INFO="${GPG_AGENT_SOCKET_PATH}:0:1"
    echo "GPG_AGENT_INFO set to: ${GPG_AGENT_INFO}"

    if [ -S "${SSH_AGENT_SOCKET_PATH}" ]; then
        echo "SSH agent socket found at ${SSH_AGENT_SOCKET_PATH}."
        export SSH_AUTH_SOCK="${SSH_AGENT_SOCKET_PATH}"
        echo "SSH_AUTH_SOCK set to: ${SSH_AUTH_SOCK}"
    else
        echo "Warning: SSH agent socket (${SSH_AGENT_SOCKET_PATH}) not found. SSH forwarding via GPG might not work as expected."
        # unset SSH_AUTH_SOCK # Or let it be if it was set by something else
    fi

    echo "Updating TTY for the agent..."
    # This command should now find the agent via the default socket or GPG_AGENT_INFO
    gpg-connect-agent updatestartuptty /bye
    gpg-connect-agent /bye # A simple ping to confirm connection
else
    echo "ERROR: gpg-agent socket not found at ${GPG_AGENT_SOCKET_PATH} after attempting to start."
    echo "gpg-agent may not have started correctly."
    unset GPG_AGENT_INFO
    unset SSH_AUTH_SOCK
fi

echo ""
echo "Script finished."
echo "Try your GPG command now (e.g., 'gpg --list-keys' or your original failing command)."
echo "If it asks for a passphrase, pinentry-curses should appear in this terminal."
