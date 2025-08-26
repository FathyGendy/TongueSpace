using System.ComponentModel.DataAnnotations;

namespace CoursePLatform.Models
{
    public class InstructorApplication
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Expertise { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Experience { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string MotivationReason { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewNotes { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public enum ApplicationStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        UnderReview = 4
    }
}