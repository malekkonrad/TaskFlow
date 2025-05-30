using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class UserTask
{
    [Key]
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? Deadline { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    public int? StatusId { get; set; }
    public Status? Status { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}