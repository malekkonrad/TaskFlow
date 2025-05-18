using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class TaskHistoryEntry
{
    [Key]
    public int Id { get; set; }

    public int TaskId { get; set; }
    public Task Task { get; set; }

    public int ChangedById { get; set; }
    public User ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.Now;

    [Required]
    public string ChangeType { get; set; } // "StatusChange", "Comment", "AssignedUser", etc.

    public string Details { get; set; } // np. "Status zmieniony z 'To Do' na 'In Progress'"
}