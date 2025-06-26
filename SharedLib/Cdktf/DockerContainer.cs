
using Constructs;
using HashiCorp.Cdktf;

using HashiCorp.Cdktf.Providers.Docker.Provider;
using HashiCorp.Cdktf.Providers.Docker.Image;
using HashiCorp.Cdktf.Providers.Docker.Container;

namespace AgentService.Cdktf
{
    internal class DockerContainer : TerraformStack
    {
        public DockerContainer(Construct scope, string id) : base(scope, id)
        {

            new DockerProvider(this, "docker", new DockerProviderConfig { });

            Image dockerImage = new Image(this, "nginxImage", new ImageConfig
            {
                Name = "nginx:latest",
                KeepLocally = false,
            });

            new Container(this, "nginxContainer", new ContainerConfig
            {
                Image = dockerImage.Name,
                Name = "tutorial",
                Ports = new[] { new ContainerPorts 
                {
                Internal = 80,
                External = 8000
            }}
            });
        }
    }
}
