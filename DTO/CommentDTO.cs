using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class CommentDTO
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public int TaskItemId { get; set; }

    public int? AuthorId { get; set; }

}