namespace CoursePLatform.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }

        public decimal ProgressPercentage { get; set; } = 0;

        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public int CourseId { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
    }
}