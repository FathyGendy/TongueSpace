using Microsoft.AspNetCore.Mvc;

namespace CoursePLatform.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetAuthenticationStatus()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            
            return Ok(new { 
                isAuthenticated = isAuthenticated,
                username = isAuthenticated ? User.Identity?.Name : null
            });
        }
    }
}