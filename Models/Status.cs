using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class Status
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
}