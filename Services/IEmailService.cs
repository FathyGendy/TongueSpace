// Services/IEmailService.cs
namespace CoursePLatform.Services
{
    public interface IEmailService
    {
        Task SendInstructorApprovalEmailAsync(string email, string firstName);
        Task SendInstructorRejectionEmailAsync(string email, string firstName, string reason, string? notes);
        Task SendApplicationConfirmationEmailAsync(string email, string firstName);
    }
}