using CoursePLatform.Data;
using CoursePLatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CoursePLatform.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses(
            [FromQuery] CourseLanguage? language = null,
            [FromQuery] CourseLevel? level = null,
            [FromQuery] string? search = null)
        {
            var query = _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .Where(c => c.IsPublished);

            // Apply filters
            if (language.HasValue)
            {
                query = query.Where(c => c.Language == language.Value);
            }

            if (level.HasValue)
            {
                query = query.Where(c => c.Level == level.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));
            }

            var courses = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Language = c.Language.ToString(),
                    Level = c.Level.ToString(),
                    Price = c.Price,
                    ThumbnailUrl = c.ThumbnailUrl,
                    InstructorName = $"{c.Instructor.FirstName} {c.Instructor.LastName}",
                    LessonCount = c.Lessons.Count,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(courses);
        }

        // GET: api/courses/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDetailDto>> GetCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);

            if (course == null)
            {
                return NotFound();
            }

            var courseDetail = new CourseDetailDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Language = course.Language.ToString(),
                Level = course.Level.ToString(),
                Price = course.Price,
                ThumbnailUrl = course.ThumbnailUrl,
                InstructorName = $"{course.Instructor.FirstName} {course.Instructor.LastName}",
                CreatedAt = course.CreatedAt,
                Lessons = course.Lessons.Select(l => new LessonDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    DurationMinutes = l.DurationMinutes,
                    OrderIndex = l.OrderIndex,
                    VideoUrl = l.VideoUrl
                }).ToList()
            };

            return Ok(courseDetail);
        }

        // POST: api/courses
        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<Course>> CreateCourse(CreateCourseDto createCourseDto)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var course = new Course
            {
                Title = createCourseDto.Title,
                Description = createCourseDto.Description,
                Language = createCourseDto.Language,
                Level = createCourseDto.Level,
                Price = createCourseDto.Price,
                ThumbnailUrl = createCourseDto.ThumbnailUrl,
                InstructorId = userId,
                IsPublished = false // New courses start as drafts
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        // PUT: api/courses/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> UpdateCourse(int id, CreateCourseDto updateCourseDto)
        {
            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            // Check if user owns this course or is admin
            if (course.InstructorId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            course.Title = updateCourseDto.Title;
            course.Description = updateCourseDto.Description;
            course.Language = updateCourseDto.Language;
            course.Level = updateCourseDto.Level;
            course.Price = updateCourseDto.Price;
            course.ThumbnailUrl = updateCourseDto.ThumbnailUrl;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/courses/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            // Check if user owns this course or is admin
            if (course.InstructorId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/courses/my-courses (for instructors)
        [HttpGet("my-courses")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetMyCourses()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .Where(c => c.InstructorId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Language = c.Language.ToString(),
                    Level = c.Level.ToString(),
                    Price = c.Price,
                    ThumbnailUrl = c.ThumbnailUrl,
                    InstructorName = $"{c.Instructor.FirstName} {c.Instructor.LastName}",
                    LessonCount = c.Lessons.Count,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(courses);
        }
    }
    
    // DTOs (Data Transfer Objects)
    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CourseDetailDto : CourseDto
    {
        public List<LessonDto> Lessons { get; set; } = new();
    }

    public class LessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int OrderIndex { get; set; }
        public string? VideoUrl { get; set; }
    }
    
    public class CreateCourseDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public CourseLanguage Language { get; set; }
        
        [Required]
        public CourseLevel Level { get; set; }
        
        [Required]
        [Range(0, 1000)]
        public decimal Price { get; set; }
        
        public string? ThumbnailUrl { get; set; }
    }
}