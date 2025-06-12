using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedLib.Model;

namespace SharedLib.Instances
{
    public class AgentInstance
    {
        public readonly Agent Agent;        

        public readonly string Role;
        public readonly string Script;
        public readonly string Tools;

        public AgentInstance(Agent agent, string role = null, string script = null, string tools = null)
        {
            Agent = agent;
            Role = role;
            Script = script;
            Tools = tools;
        }

        public class List : List<AgentInstance> { }
        public class StringMap : Dictionary<string, AgentInstance>
        {
            public StringMap() : base(StringComparer.OrdinalIgnoreCase) { }
            public StringMap(IEnumerable<AgentInstance> instances)
                : base(instances.ToDictionary(i => i.Agent.Id, StringComparer.OrdinalIgnoreCase)) { }
        }
    }
}
