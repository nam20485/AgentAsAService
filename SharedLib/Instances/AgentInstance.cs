using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedLib.Model;

namespace SharedLib.Instances
{
    public abstract class AgentInstance : IAgentInstance
    {
        public readonly Model.Agent Agent;        
        protected readonly ILlmClient _llmClient;

        public readonly string Role;
        public readonly string Script;
        public readonly string Tools;

        public AgentInstance(Agent agent, string role, string script, string tools)
        {
            Agent = agent;
            Role = role;
            Script = script;
            Tools = tools;
        }

        // Implement IAgentInstance interface methods as abstract
        public abstract void Start();
        public abstract IAgentInstance.Status GetStatus();
        public abstract void Stop();

        public class List : List<AgentInstance> { }
        public class StringMap : Dictionary<string, AgentInstance> { }
    }
}
