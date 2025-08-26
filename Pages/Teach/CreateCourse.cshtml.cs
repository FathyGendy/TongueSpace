// Pages/Teach/CreateCourse.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoursePLatform.Pages.Teach
{
    [Authorize(Roles = "Instructor,Admin")]
    public class CreateCourseModel : PageModel
    {
        public void OnGet()
        {
            // Course creation form
        }
    }
}