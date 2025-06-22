using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.DTOs
{
    public class UpdateAgentStateRequest
    {
        public string AgentId { get; set; } = "";
        public string State { get; set; } = "";
    }
}
