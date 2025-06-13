using System;

namespace SharedLib.Model;

public class Orchestrator : Agent
{
    public Orchestrator() : base("Orchestrator")
    {
    }
    
    public Orchestrator(string name) : base(name)
    {
    }

    public DateTime CreatedAt { get; set; }
}
