using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoursePLatform.Pages
{
    public class CourseDetailModel : PageModel
    {
        public int CourseId { get; set; }
        public CourseDto? Course { get; set; }

        public void OnGet(int id)
        {
            CourseId = id;
        }
    }

    public class CourseDto
    {
        public string Title { get; set; } = string.Empty;
    }
}