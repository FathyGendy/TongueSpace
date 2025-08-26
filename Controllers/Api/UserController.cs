// Controllers/UserController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CoursePLatform.Models;

namespace CoursePLatform.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<ApplicationUser> userManager,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var profile = new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    Role = user.Role.ToString() // Convert enum to string
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            return Ok(new { isAuthenticated = User.Identity?.IsAuthenticated ?? false });
        }
    }
}