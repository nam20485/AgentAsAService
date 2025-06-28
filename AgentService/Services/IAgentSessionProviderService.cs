using SharedLib.Model;

namespace AgentService.Services
{
    /// <summary>
    /// Service for managing the lifecycle of agent sessions including starting, stopping, pausing, and resuming operations
    /// </summary>
    public interface IAgentSessionProviderService
    {
        /// <summary>
        /// Start a new agent session for the specified session
        /// </summary>
        /// <param name="session">The agent session to start</param>
        /// <returns>Task that completes when the session is started successfully</returns>
        /// <exception cref="InvalidOperationException">Thrown when session cannot be started</exception>
        Task<bool> StartSessionAsync(AgentSession session);

        /// <summary>
        /// Stop an active agent session
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to stop</param>
        /// <returns>Task that completes when the session is stopped successfully</returns>
        /// <exception cref="ArgumentException">Thrown when session ID is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when session cannot be stopped</exception>
        Task<bool> StopSessionAsync(string sessionId);

        /// <summary>
        /// Pause an active agent session
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to pause</param>
        /// <returns>Task that completes when the session is paused successfully</returns>
        /// <exception cref="ArgumentException">Thrown when session ID is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when session cannot be paused</exception>
        Task<bool> PauseSessionAsync(string sessionId);

        /// <summary>
        /// Resume a paused agent session
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to resume</param>
        /// <returns>Task that completes when the session is resumed successfully</returns>
        /// <exception cref="ArgumentException">Thrown when session ID is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when session cannot be resumed</exception>
        Task<bool> ResumeSessionAsync(string sessionId);

        /// <summary>
        /// Get the current status of an agent session
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session</param>
        /// <returns>The current status of the session, or null if session not found</returns>
        Task<string?> GetSessionStatusAsync(string sessionId);

        /// <summary>
        /// Check if a session is currently running
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session</param>
        /// <returns>True if the session is active, false otherwise</returns>
        Task<bool> IsSessionActiveAsync(string sessionId);
    }
}
