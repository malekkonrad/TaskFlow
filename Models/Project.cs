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

    // public ICollection<User> Users { get; set; } = new List<User>();    

    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}