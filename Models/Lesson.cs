using System.ComponentModel.DataAnnotations;

namespace CoursePLatform.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string? Content { get; set; } // Could be HTML, text, or references to files

        public string? VideoUrl { get; set; }

        public int OrderIndex { get; set; } // For lesson ordering within a course

        public int DurationMinutes { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int CourseId { get; set; }

        // Navigation properties
        public virtual Course Course { get; set; } = null!;
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }
}