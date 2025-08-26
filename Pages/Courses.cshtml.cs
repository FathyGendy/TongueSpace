using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoursePLatform.Pages
{
    public class CoursesModel : PageModel
    {
        public void OnGet()
        {
            // This page loads data via JavaScript/API, so we don't need much here
            // Authentication checks are now handled client-side via API calls
        }
    }
}