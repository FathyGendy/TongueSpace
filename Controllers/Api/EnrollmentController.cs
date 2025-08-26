using CoursePLatform.Data;
using CoursePLatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoursePLatform.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrollmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/enrollment/status/{courseId} - Allow anonymous access
        [HttpGet("status/{courseId}")]
        [AllowAnonymous]
        public async Task<ActionResult<EnrollmentStatusDto>> GetEnrollmentStatus(int courseId)
        {
            // Check if course exists and is published in a single query.
            var course = await _context.Courses
                .Include(c => c.Lessons) // Include lessons to get the total count.
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsPublished);
            
            if (course == null)
            {
                return NotFound("Course not found or not published.");
            }
            
            var totalLessons = course.Lessons.Count;
            var userId = _userManager.GetUserId(User);

            // If user is not authenticated, return a DTO for anonymous users.
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new EnrollmentStatusDto
                {
                    IsEnrolled = false,
                    CanEnroll = true, // The course is published, so enrollment is possible.
                    NeedsLogin = true,
                    TotalLessons = totalLessons,
                });
            }

            // Check if user is enrolled and retrieve progress in one query.
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment != null)
            {
                // Count completed lessons for the enrolled user and course.
                var completedLessons = await _context.LessonProgresses
                    .Where(lp => lp.UserId == userId && lp.IsCompleted && course.Lessons.Select(l => l.Id).Contains(lp.LessonId))
                    .CountAsync();
                
                var progressPercentage = totalLessons > 0 ? (decimal)completedLessons / totalLessons * 100 : 0;
                
                return Ok(new EnrollmentStatusDto
                {
                    IsEnrolled = true,
                    CanEnroll = false,
                    NeedsLogin = false,
                    EnrollmentId = enrollment.Id,
                    EnrolledAt = enrollment.EnrolledAt,
                    ProgressPercentage = progressPercentage,
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons,
                    IsCompleted = enrollment.CompletedAt != null
                });
            }

            // User is authenticated but not enrolled.
            return Ok(new EnrollmentStatusDto
            {
                IsEnrolled = false,
                CanEnroll = true,
                NeedsLogin = false,
                TotalLessons = totalLessons,
            });
        }

        // POST: api/enrollment/enroll/{courseId}
        [HttpPost("enroll/{courseId}")]
        [Authorize]
        public async Task<IActionResult> EnrollInCourse(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            // Check if course exists and is published without unnecessary includes.
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsPublished);

            if (course == null)
            {
                // This message is more specific now.
                return NotFound("Course not found or not published.");
            }

            // Check if already enrolled in a single query.
            var existingEnrollment = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existingEnrollment)
            {
                return BadRequest("You are already enrolled in this course");
            }

            // Create new enrollment
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new EnrollmentResultDto
            {
                Success = true,
                Message = "You have successfully enrolled in this course!",
                EnrollmentId = enrollment.Id
            });
        }

        // DELETE: api/enrollment/unenroll/{courseId}
        [HttpDelete("unenroll/{courseId}")]
        [Authorize]
        public async Task<IActionResult> UnenrollFromCourse(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Use a single query to find and remove the enrollment.
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment == null)
            {
                return NotFound("Enrollment not found");
            }

            // Efficiently remove lesson progress by filtering on courseId and userId.
            var lessonProgressesToRemove = _context.LessonProgresses
                .Where(lp => lp.UserId == userId && _context.Lessons.Any(l => l.Id == lp.LessonId && l.CourseId == courseId));

            _context.LessonProgresses.RemoveRange(lessonProgressesToRemove);
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully unenrolled from course" });
        }

        // GET: api/enrollment/my-courses
        [HttpGet("my-courses")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MyEnrolledCourseDto>>> GetMyEnrolledCourses()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            // This refactored query is more efficient by pulling data from the database
            // and then materializing the results. It avoids the N+1 query problem.
            var enrollmentsWithProgress = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => new
                {
                    Enrollment = e,
                    Course = e.Course,
                    TotalLessons = e.Course.Lessons.Count,
                    InstructorName = $"{e.Course.Instructor.FirstName} {e.Course.Instructor.LastName}",
                    CompletedLessons = _context.LessonProgresses
                        .Count(lp => lp.UserId == userId && lp.IsCompleted && e.Course.Lessons.Any(l => l.Id == lp.LessonId))
                })
                .ToListAsync();

            var enrolledCourses = enrollmentsWithProgress
                .OrderByDescending(x => x.Enrollment.EnrolledAt)
                .Select(x => new MyEnrolledCourseDto
                {
                    EnrollmentId = x.Enrollment.Id,
                    CourseId = x.Enrollment.CourseId,
                    CourseName = x.Course.Title,
                    CourseDescription = x.Course.Description,
                    Language = x.Course.Language.ToString(),
                    Level = x.Course.Level.ToString(),
                    InstructorName = x.InstructorName,
                    ThumbnailUrl = x.Course.ThumbnailUrl,
                    EnrolledAt = x.Enrollment.EnrolledAt,
                    ProgressPercentage = x.Enrollment.ProgressPercentage,
                    TotalLessons = x.TotalLessons,
                    CompletedLessons = x.CompletedLessons,
                    IsCompleted = x.Enrollment.CompletedAt != null,
                    CompletedAt = x.Enrollment.CompletedAt
                })
                .ToList();

            return Ok(enrolledCourses);
        }

        // PUT: api/enrollment/update-progress/{courseId}
        [HttpPut("update-progress/{courseId}")]
        [Authorize]
        public async Task<IActionResult> UpdateCourseProgress(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment == null)
            {
                return NotFound("Enrollment not found");
            }

            var totalLessons = enrollment.Course.Lessons.Count;
            if (totalLessons == 0)
            {
                // This is a cleaner way to return progress for a course with no lessons.
                return Ok(new { progressPercentage = 100, isCompleted = true, completedLessons = 0, totalLessons = 0 });
            }

            // Count completed lessons efficiently for the specific course.
            var completedLessons = await _context.LessonProgresses
                .CountAsync(lp => lp.UserId == userId && 
                                  lp.IsCompleted && 
                                  enrollment.Course.Lessons.Any(l => l.Id == lp.LessonId));

            var progressPercentage = (decimal)completedLessons / totalLessons * 100;
            
            // Only update the enrollment if the progress has changed to avoid unnecessary database writes.
            if (enrollment.ProgressPercentage != progressPercentage)
            {
                enrollment.ProgressPercentage = progressPercentage;
            }
            
            // Mark as completed if 100% and not already completed.
            if (progressPercentage >= 100 && enrollment.CompletedAt == null)
            {
                enrollment.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                progressPercentage = enrollment.ProgressPercentage,
                isCompleted = enrollment.CompletedAt != null,
                completedLessons = completedLessons,
                totalLessons = totalLessons
            });
        }
    }

    // DTOs
    public class EnrollmentStatusDto
    {
        public bool IsEnrolled { get; set; }
        public bool CanEnroll { get; set; }
        public bool NeedsLogin { get; set; }
        public int? EnrollmentId { get; set; }
        public DateTime? EnrolledAt { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class EnrollmentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? EnrollmentId { get; set; }
    }

    public class MyEnrolledCourseDto
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseDescription { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public DateTime EnrolledAt { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}