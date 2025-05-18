using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ApiToken { get; set; } = string.Empty;  // REST API

    // public bool isAdmin { get; set; } = false;
    public string Role { get; set; } = "User"; // "Admin", "User"

    // public ICollection<Project> Projects { get; set; } = new List<Project>();
    // public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
}