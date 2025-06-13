using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedLib.Model;

namespace SharedLib.Instances
{
    public interface IAgentInstance // Changed from internal to public
    {
        public enum Status
        {
            Stopped,
            Starting,
            Running,
            Stopping,
            Error
        }        

        void Start();
        Status GetStatus();
        void Stop();
    }
}
