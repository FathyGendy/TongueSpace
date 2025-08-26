using CoursePLatform.Data;
using CoursePLatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace CoursePLatform.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestDataController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/testdata/seed
        [HttpPost("seed")]
        public async Task<IActionResult> SeedData()
        {
            // Check if we already have courses
            if (_context.Courses.Any())
            {
                return BadRequest("Sample data already exists");
            }

            // Create an instructor user if none exists
            var instructor = await _userManager.FindByEmailAsync("instructor@test.com");
            if (instructor == null)
            {
                instructor = new ApplicationUser
                {
                    UserName = "instructor@test.com",
                    Email = "instructor@test.com",
                    FirstName = "Ahmed",
                    LastName = "Hassan",
                    Role = UserRole.Instructor,
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(instructor, "Instructor123!");
            }

            // Create sample courses
            var courses = new List<Course>
            {
                new Course
                {
                    Title = "Learn Arabic for Beginners",
                    Description = "Complete course for learning Arabic from scratch. Perfect for beginners who want to master the basics.",
                    Language = CourseLanguage.Arabic,
                    Level = CourseLevel.Beginner,
                    Price = 49.99m,
                    InstructorId = instructor.Id,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Course
                {
                    Title = "German Grammar Mastery",
                    Description = "Master German grammar with practical exercises and real-world examples.",
                    Language = CourseLanguage.German,
                    Level = CourseLevel.Intermediate,
                    Price = 79.99m,
                    InstructorId = instructor.Id,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Course
                {
                    Title = "Business English Communication",
                    Description = "Improve your professional English skills for workplace success.",
                    Language = CourseLanguage.English,
                    Level = CourseLevel.Advanced,
                    Price = 99.99m,
                    InstructorId = instructor.Id,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            _context.Courses.AddRange(courses);
            await _context.SaveChangesAsync();

            // Add some lessons to the first course
            var arabicCourse = courses.First();
            var lessons = new List<Lesson>
            {
                new Lesson
                {
                    Title = "Arabic Alphabet - Part 1",
                    Description = "Learn the first 10 letters of the Arabic alphabet",
                    CourseId = arabicCourse.Id,
                    OrderIndex = 1,
                    DurationMinutes = 15,
                    IsPublished = true,
                    Content = "Introduction to Arabic letters: أ ب ت ث ج ح خ د ذ ر"
                },
                new Lesson
                {
                    Title = "Arabic Alphabet - Part 2",
                    Description = "Continue learning the Arabic alphabet",
                    CourseId = arabicCourse.Id,
                    OrderIndex = 2,
                    DurationMinutes = 20,
                    IsPublished = true,
                    Content = "More Arabic letters: ز س ش ص ض ط ظ ع غ ف"
                },
                new Lesson
                {
                    Title = "Basic Arabic Words",
                    Description = "Learn your first Arabic vocabulary",
                    CourseId = arabicCourse.Id,
                    OrderIndex = 3,
                    DurationMinutes = 25,
                    IsPublished = true,
                    Content = "Basic words: مرحبا (Hello), شكرا (Thank you), نعم (Yes), لا (No)"
                }
            };

            _context.Lessons.AddRange(lessons);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Sample data created successfully", 
                coursesCreated = courses.Count,
                lessonsCreated = lessons.Count
            });
        }
    }
}