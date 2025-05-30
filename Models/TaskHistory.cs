using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class TaskHistory
{
    [Key]
    public int Id { get; set; }
        
    [Required]
    public int TaskId { get; set; }
    public UserTask? Task { get; set; }
    
    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }
    
    [Required]
    public string? Action { get; set; } // Created, Updated, Completed, Assigned, CommentAdded, StatusChanged, etc.
    // można też użyć enum, ale dla prostoty zostawiamy string    
    
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? FieldName { get; set; } // Title, Description, Status, Priority, AssignedTo, etc.
    
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Dodatkowe pola dla statystyk
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public TimeSpan? TimeSpent { get; set; } // Czas spędzony na zadaniu
    public string? Category { get; set; } // Task, Comment, Project, etc.


}