using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int TaskItemId { get; set; }
    public UserTask Task { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; }
}