namespace CoursePLatform.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public int WatchedSeconds { get; set; } = 0; // For video progress tracking

        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public int LessonId { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Lesson Lesson { get; set; } = null!;
    }
}