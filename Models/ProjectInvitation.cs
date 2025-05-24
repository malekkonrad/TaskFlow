using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class ProjectInvitation
    {
        public int Id { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        
        [Required]
        public int InvitedUserId { get; set; }
        public User InvitedUser { get; set; }
        
        [Required]
        public int InvitedById { get; set; }
        public User InvitedBy { get; set; }
        
        [Required]
        public string Role { get; set; } = "Member";
        
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Declined, Expired
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? RespondedAt { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddDays(7);
        
        public string? Message { get; set; }
        public string? Token { get; set; } // Unikalny token do akceptacji zaproszenia
    }