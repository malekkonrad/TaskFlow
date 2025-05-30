using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;


public class ProjectMember
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        
        [Required]
        public string Role { get; set; } = "Member"; // Owner, Admin, Member, Viewer
        
        public DateTime JoinedAt { get; set; } = DateTime.Now;
        public DateTime? InvitedAt { get; set; }
        public string InvitationStatus { get; set; } = "Accepted"; // Pending, Accepted, Declined
        
    }