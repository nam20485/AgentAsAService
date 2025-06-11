using System;

namespace SharedLib.Model;

public class Collaborator : Agent
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Collaborator() : base("Collaborator")
    {
    }
    
    public Collaborator(string name) : base(name)
    {
    }
}
