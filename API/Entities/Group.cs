using System;
using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Group(string name)
{
    [Key]
    public string Name { get; set; } = name;

    // nav properties
    public ICollection<Connection> Connections { get; set; } = [];
}
