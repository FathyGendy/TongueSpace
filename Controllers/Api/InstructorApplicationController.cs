// Controllers/InstructorApplicationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoursePLatform.Data;
using CoursePLatform.Models;
using CoursePLatform.Services;

namespace CoursePLatform.Controllers
{
    [Route("api/instructor-application")]
    [ApiController]
    public class InstructorApplicationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<InstructorApplicationController> _logger;

        public InstructorApplicationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<InstructorApplicationController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Controller is working", timestamp = DateTime.UtcNow });
        }

        [HttpGet("status")]
        [Authorize]
        public async Task<IActionResult> GetApplicationStatus()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token.");
                    return Unauthorized("User not found.");
                }

                _logger.LogInformation("Checking application status for user: {UserId}", userId);

                var application = await _context.InstructorApplications
                    .Where(a => a.UserId == userId)
                    .Select(a => new
                    {
                        a.Status,
                        a.ApplicationDate,
                        a.ReviewedAt,
                        a.Id
                    })
                    .FirstOrDefaultAsync();

                if (application == null)
                {
                    _logger.LogInformation("No application found for user: {UserId}", userId);
                    return Ok(new { hasApplication = false });
                }

                _logger.LogInformation("Application found for user: {UserId}, Status: {Status}", userId, application.Status);

                return Ok(new
                {
                    hasApplication = true,
                    status = application.Status.ToString(),
                    applicationDate = application.ApplicationDate,
                    reviewedAt = application.ReviewedAt,
                    applicationId = application.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking application status for user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitApplication([FromBody] InstructorApplicationDto applicationDto)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token.");
                    return Unauthorized("User not found.");
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found in database.", userId);
                    return BadRequest("User not found");
                }

                _logger.LogInformation("Submitting application for user: {UserId} ({Email})", userId, user.Email);

                // Check if user already has an application
                var existingApplication = await _context.InstructorApplications
                    .AnyAsync(a => a.UserId == userId);

                if (existingApplication)
                {
                    _logger.LogWarning("User {UserId} already has an application", userId);
                    return BadRequest("You have already submitted an application");
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(applicationDto.Bio) ||
                    string.IsNullOrWhiteSpace(applicationDto.Expertise) ||
                    string.IsNullOrWhiteSpace(applicationDto.Experience) ||
                    string.IsNullOrWhiteSpace(applicationDto.MotivationReason))
                {
                    return BadRequest("All required fields must be filled");
                }

                var application = new InstructorApplication
                {
                    UserId = userId,
                    Bio = applicationDto.Bio.Trim(),
                    Expertise = applicationDto.Expertise.Trim(),
                    Experience = applicationDto.Experience.Trim(),
                    MotivationReason = applicationDto.MotivationReason.Trim(),
                    PhoneNumber = applicationDto.PhoneNumber?.Trim(),
                    Status = ApplicationStatus.Pending,
                    ApplicationDate = DateTime.UtcNow
                };

                _context.InstructorApplications.Add(application);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Application submitted successfully for user: {UserId}, Application ID: {ApplicationId}",
                    userId, application.Id);

                // Send confirmation email
                try
                {
                    if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(user.FirstName))
                    {
                        await _emailService.SendApplicationConfirmationEmailAsync(user.Email, user.FirstName);
                        _logger.LogInformation("Confirmation email sent to: {Email}", user.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Skipping confirmation email for user {UserId} due to missing email or first name.", userId);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send confirmation email to: {Email}", user.Email);
                    // Don't fail the application submission if email fails
                }

                return Ok(new
                {
                    message = "Application submitted successfully",
                    applicationId = application.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting instructor application");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("debug")]
        [Authorize]
        public async Task<IActionResult> DebugApplications()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token for debug endpoint.");
                    return Unauthorized("User not found.");
                }

                var applications = await _context.InstructorApplications
                    .Where(a => a.UserId == userId)
                    .Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        a.Status,
                        a.ApplicationDate,
                        a.Bio,
                        a.Expertise
                    })
                    .ToListAsync();

                var allApplicationsCount = await _context.InstructorApplications.CountAsync();

                return Ok(new
                {
                    userId = userId,
                    userApplications = applications,
                    totalApplicationsInDb = allApplicationsCount,
                    message = "Debug info retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug endpoint");
                return StatusCode(500, ex.Message);
            }
        }
        
        // For debugging
        [HttpGet("routes")]
        public IActionResult GetRoutes()
        {
            return Ok(new { 
                message = "InstructorApplication controller is accessible",
                availableEndpoints = new[] {
                    "GET /api/instructor-application/test",
                    "GET /api/instructor-application/status",
                    "POST /api/instructor-application/submit",
                    "GET /api/instructor-application/debug"
                }
            });
        }
    }

    public class InstructorApplicationDto
    {
        public string Bio { get; set; } = string.Empty;
        public string Expertise { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string MotivationReason { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}