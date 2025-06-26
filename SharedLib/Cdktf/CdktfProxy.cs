using System;
using Constructs;
using HashiCorp.Cdktf;

namespace AgentService.Cdktf
{
    public class CdktfProxy
    {
        public int RunCommand(string command, params string[] args)
        {
            // uses 
            return -1;
        }

        public int CreateDockerContainer(string image)
        {
            App app = new App();
            var dockerContainer = new DockerContainer(app, "learn-cdktf-docker");
            app.Synth();
            Console.WriteLine("App synth complete");

            return -1;
        }
    }

}