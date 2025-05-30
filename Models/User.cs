using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;  // REST API

    public string Role { get; set; } = "User"; // "Admin", "User" - pomyśleć o enumie


    // Projekty jako właściciel
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();

    // Członkostwo w projektach
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    

    // Taski 
    public ICollection<UserTask> AssignedTasks { get; set; } = new List<UserTask>();

    // Komentarze
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    // Historia aktywności
    public ICollection<TaskHistory> ActivityHistory { get; set; } = new List<TaskHistory>();





}