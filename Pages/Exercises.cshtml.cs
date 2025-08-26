using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoursePLatform.Pages
{
    public class ExercisesModel : PageModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";

        public void OnGet(int courseId = 0)
        {
            CourseId = courseId;
            
            // Simple course names based on ID (I can improve this later with database lookup)
            CourseName = courseId switch
            {
                5 => "Spanish for Travelers",
                4 => "French Conversation Practice", 
                2 => "German Grammar Mastery",
                3 => "Business English Communication",
                1 => "Arabic for Beginners",
                _ => "Advanced English Writing"
            };
        }
    }
}