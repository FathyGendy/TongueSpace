// Controllers/DebugController.cs - TEMPORARY FOR TESTING
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoursePLatform.Data;

namespace CoursePLatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DebugController> _logger;

        public DebugController(ApplicationDbContext context, ILogger<DebugController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetAllApplications()
        {
            try
            {
                var applications = await _context.InstructorApplications
                    .Include(a => a.User)
                    .Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        UserName = a.User.FirstName + " " + a.User.LastName,
                        a.User.Email,
                        a.Status,
                        a.ApplicationDate,
                        a.Bio,
                        a.Expertise,
                        a.Experience
                    })
                    .ToListAsync();

                var totalCount = applications.Count;

                return Ok(new
                {
                    totalApplications = totalCount,
                    applications = applications,
                    message = "Debug: All applications retrieved"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Debug error getting applications");
                return StatusCode(500, "Debug error: " + ex.Message);
            }
        }

        [HttpGet("check-database")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var hasApplicationsTable = await _context.Database.ExecuteSqlRawAsync(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'InstructorApplications'") >= 0;

                var applicationCount = await _context.InstructorApplications.CountAsync();
                var userCount = await _context.Users.CountAsync();

                return Ok(new
                {
                    databaseConnected = true,
                    applicationsTableExists = hasApplicationsTable,
                    totalApplications = applicationCount,
                    totalUsers = userCount,
                    message = "Database check completed"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    databaseConnected = false,
                    error = ex.Message,
                    message = "Database connection failed"
                });
            }
        }

        [HttpPost("create-test-application")]
        public async Task<IActionResult> CreateTestApplication()
        {
            try
            {
                // Create a test user if needed
                var testUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
                
                if (testUser == null)
                {
                    testUser = new Models.ApplicationUser
                    {
                        UserName = "test@example.com",
                        Email = "test@example.com",
                        FirstName = "Test",
                        LastName = "User",
                        Role = Models.UserRole.Student,
                        EmailConfirmed = true
                    };
                    _context.Users.Add(testUser);
                    await _context.SaveChangesAsync();
                }

                // Check if test application already exists
                var existingApp = await _context.InstructorApplications
                    .FirstOrDefaultAsync(a => a.UserId == testUser.Id);

                if (existingApp != null)
                {
                    return Ok(new { message = "Test application already exists", applicationId = existingApp.Id });
                }

                // Create test application
                var testApplication = new Models.InstructorApplication
                {
                    UserId = testUser.Id,
                    Bio = "This is a test bio for debugging purposes.",
                    Expertise = "Test expertise in multiple languages",
                    Experience = "Test teaching experience for debugging",
                    MotivationReason = "Test motivation for debugging the application system",
                    PhoneNumber = "+1234567890",
                    Status = Models.ApplicationStatus.Pending,
                    ApplicationDate = DateTime.UtcNow
                };

                _context.InstructorApplications.Add(testApplication);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Test application created successfully",
                    applicationId = testApplication.Id,
                    userId = testUser.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test application");
                return StatusCode(500, "Error creating test application: " + ex.Message);
            }
        }
    }
}