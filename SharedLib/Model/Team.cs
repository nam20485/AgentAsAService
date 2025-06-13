using System;
using System.Collections.Generic;

namespace SharedLib.Model;

public class Team
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<Collaborator> Members { get; set; } = new List<Collaborator>();
}
