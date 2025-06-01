using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class ProjectDTO
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsPublic { get; set; } = false;

}