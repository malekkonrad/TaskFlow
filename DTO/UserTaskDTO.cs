using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class UserTaskDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }

    public string? Description { get; set; }

    public int ProjectId { get; set; }

    public int? AssigneeId { get; set; }

}