using System.ComponentModel.DataAnnotations;

namespace CoursePLatform.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public CourseLanguage Language { get; set; }

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        [Range(0, 10000)]
        public decimal Price { get; set; }

        public string? ThumbnailUrl { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public string InstructorId { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser Instructor { get; set; } = null!;
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }

    public enum CourseLanguage
    {
        Arabic = 1,
        English = 2,
        German = 3,
        French = 4,
        Spanish = 5
    }

    public enum CourseLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3
    }
}