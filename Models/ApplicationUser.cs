using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CoursePLatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        // New properties for user profile
        // Ensure default is set if null
        private string? _profilePictureUrl;
        public string? ProfilePictureUrl
        {
            get => string.IsNullOrEmpty(_profilePictureUrl) ? "/images/default-avatar.jpg" : _profilePictureUrl;
            set => _profilePictureUrl = value;
        }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public UserRole Role { get; set; } = UserRole.Student;

        // Navigation properties
        public virtual ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }

    public enum UserRole
    {
        Student = 1,
        Instructor = 2,
        Admin = 3
    }
}
