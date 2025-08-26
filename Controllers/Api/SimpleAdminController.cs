// Controllers/SimpleAdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoursePLatform.Data;
using CoursePLatform.Models;
using CoursePLatform.Services;

namespace CoursePLatform.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin")]
    [ApiController]
    public class SimpleAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<SimpleAdminController> _logger;

        public SimpleAdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<SimpleAdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications()
        {
            try
            {
                _logger.LogInformation("Admin getting applications...");

                var applications = await _context.InstructorApplications
                    .Include(a => a.User)
                    .OrderByDescending(a => a.ApplicationDate)
                    .Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        UserName = (a.User != null) ? a.User.FirstName + " " + a.User.LastName : "N/A",
                        Email = a.User!.Email!,
                        a.PhoneNumber,
                        a.Expertise,
                        a.Experience,
                        a.Bio,
                        a.MotivationReason,
                        a.Status,
                        StatusNumber = (int)a.Status,
                        a.ApplicationDate,
                        a.ReviewedAt,
                        a.ReviewNotes
                    })
                    .ToListAsync();

                _logger.LogInformation("Found {Count} applications", applications.Count);

                return Ok(new
                {
                    applications = applications,
                    totalCount = applications.Count,
                    message = "Applications loaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications");
                return StatusCode(500, "Error getting applications: " + ex.Message);
            }
        }

        [HttpGet("applications/{id}")]
        public async Task<IActionResult> GetApplication(int id)
        {
            try
            {
                var application = await _context.InstructorApplications
                    .Include(a => a.User)
                    .Where(a => a.Id == id)
                    .Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        a.User!.FirstName,
                        a.User!.LastName,
                        a.User!.Email,
                        a.PhoneNumber,
                        a.Expertise,
                        a.Experience,
                        a.Bio,
                        a.MotivationReason,
                        a.Status,
                        a.ApplicationDate,
                        a.ReviewedAt,
                        a.ReviewNotes
                    })
                    .FirstOrDefaultAsync();

                if (application == null)
                {
                    return NotFound("Application not found");
                }

                return Ok(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application {Id}", id);
                return StatusCode(500, "Error getting application: " + ex.Message);
            }
        }

        [HttpPost("applications/{id}/approve")]
        public async Task<IActionResult> ApproveApplication(int id, [FromBody] ReviewRequest request)
        {
            try
            {
                var application = await _context.InstructorApplications
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound();
                }

                // Update application
                application.Status = ApplicationStatus.Approved;
                application.ReviewedAt = DateTime.UtcNow;
                application.ReviewNotes = request?.Notes ?? "Approved by admin";

                // Ensure the user object is not null before proceeding
                if (application.User == null)
                {
                    _logger.LogError("User not found for application {Id}", id);
                    return StatusCode(500, "User not found for this application.");
                }

                // Update user role
                await _userManager.AddToRoleAsync(application.User, "Instructor");
                application.User.Role = UserRole.Instructor;

                await _context.SaveChangesAsync();

                // Send email
                try
                {
                    // Add null-conditional checks or null-forgiving operator as appropriate
                    await _emailService.SendInstructorApprovalEmailAsync(
                        application.User.Email!,
                        application.User.FirstName!);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Email failed for application {Id}", id);
                }

                _logger.LogInformation("Application {Id} approved", id);
                return Ok(new { message = "Application approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving application {Id}", id);
                return StatusCode(500, "Error approving application: " + ex.Message);
            }
        }

        [HttpPost("applications/{id}/reject")]
        public async Task<IActionResult> RejectApplication(int id, [FromBody] ReviewRequest request)
        {
            try
            {
                var application = await _context.InstructorApplications
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound();
                }

                application.Status = ApplicationStatus.Rejected;
                application.ReviewedAt = DateTime.UtcNow;
                application.ReviewNotes = request?.Notes ?? "Rejected by admin";

                await _context.SaveChangesAsync();

                // Ensure the user object is not null before sending email
                if (application.User == null)
                {
                    _logger.LogError("User not found for application {Id}", id);
                    return StatusCode(500, "User not found for this application.");
                }

                // Send email
                try
                {
                    await _emailService.SendInstructorRejectionEmailAsync(
                        application.User.Email!,
                        application.User.FirstName!,
                        request?.Reason ?? "Application requirements not met",
                        request?.Notes);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Email failed for application {Id}", id);
                }

                _logger.LogInformation("Application {Id} rejected", id);
                return Ok(new { message = "Application rejected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting application {Id}", id);
                return StatusCode(500, "Error rejecting application: " + ex.Message);
            }
        }

        [HttpPost("applications/{id}/review")]
        public async Task<IActionResult> SetUnderReview(int id, [FromBody] ReviewRequest request)
        {
            try
            {
                var application = await _context.InstructorApplications
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound();
                }

                application.Status = ApplicationStatus.UnderReview;
                application.ReviewNotes = request?.Notes ?? "Under review";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Application set to under review" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting application under review {Id}", id);
                return StatusCode(500, "Error updating application: " + ex.Message);
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var applications = await _context.InstructorApplications.ToListAsync();

                var stats = new
                {
                    pending = applications.Count(a => a.Status == ApplicationStatus.Pending),
                    underReview = applications.Count(a => a.Status == ApplicationStatus.UnderReview),
                    approved = applications.Count(a => a.Status == ApplicationStatus.Approved),
                    rejected = applications.Count(a => a.Status == ApplicationStatus.Rejected)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats");
                return StatusCode(500, "Error getting stats: " + ex.Message);
            }
        }
    }

    public class ReviewRequest
    {
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }
}