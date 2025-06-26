namespace AgentService.Services
{
    public interface IAgentSessionProviderService
    {
        int StartSession(int agentId);
        int StopSession(int sessionId);

        int PauseSession(int sessionId);
        int ResumeSession(int sessionId);



    }
}
