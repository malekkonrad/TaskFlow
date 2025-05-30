using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsPublic { get; set; } = false;

    [Required]
    public int OwnerId { get; set; }    // czy to jest potrzebne?
    public User Owner { get; set; }
   

    // Relacja z członkami projektu
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

    public ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
}