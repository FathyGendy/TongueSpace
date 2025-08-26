// Pages/InstructorDashboard.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoursePLatform.Pages
{
    [Authorize(Roles = "Instructor,Admin")]
    public class InstructorDashboardModel : PageModel
    {
        public void OnGet()
        {
            // Dashboard loads data via JavaScript
        }
    }
}