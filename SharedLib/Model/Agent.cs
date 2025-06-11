using System;

namespace SharedLib.Model;

public abstract class Agent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }

    public Agent(string name)
    {
        Name = name;        
    }
}
