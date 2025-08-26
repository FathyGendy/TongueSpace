using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;

namespace CoursePLatform.Pages;

public class ContactModel : PageModel
{
    private readonly ILogger<ContactModel> _logger;
    private readonly IConfiguration _configuration;

    public ContactModel(ILogger<ContactModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [BindProperty]
    public ContactFormModel ContactForm { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Initialize form with default values if needed
        ContactForm.PreferredLanguage = "English";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fill in all required fields correctly.";
            return Page();
        }

        try
        {
            // Process the contact form
            await ProcessContactFormAsync(ContactForm);
            
            SuccessMessage = $"Thank you {ContactForm.FirstName}! Your message has been sent successfully. " +
                           $"We'll respond to {ContactForm.Email} within 24 hours.";
            
            // Clear the form after successful submission
            ContactForm = new ContactFormModel { PreferredLanguage = "English" };
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact form submission");
            ErrorMessage = "There was an error sending your message. Please try again or contact us directly.";
            return Page();
        }
    }

    private async Task ProcessContactFormAsync(ContactFormModel form)
    {
        // Log the contact form submission
        _logger.LogInformation("Contact form submitted by {Email} - Category: {Category}, Subject: {Subject}", 
            form.Email, form.Category, form.Subject);

        // Send email notification
        try
        {
            await SendEmailAsync(form);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contact email for {Email}: {Error}", form.Email, ex.Message);
            // Still store the form data even if email fails
            await StoreContactFormAsync(form);
            throw; // Re-throw to show error to user
        }

        // For demonstration, we'll just store the form data in logs
        // In a real application, you'd want to store this in a database
        await StoreContactFormAsync(form);
    }

    private async Task SendEmailAsync(ContactFormModel form)
    {
        // This is an example email sending implementation
        // You'll need to configure your SMTP settings in appsettings.json
        
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var smtpHost = smtpSettings["Host"];
        var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
        var smtpUsername = smtpSettings["Username"];
        var smtpPassword = smtpSettings["Password"];
        var fromEmail = smtpSettings["FromEmail"];

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail!, "TongueSpace Contact Form"),
            Subject = $"[TongueSpace] {form.Category}: {form.Subject}",
            Body = BuildEmailBody(form),
            IsBodyHtml = true
        };

        // Determine recipient based on category
        var recipientEmail = form.Category switch
        {
            "Technical" => "tonguespacesuppor1@gmail.com",
            "Teaching" => "tonguespaceinstructor@gmail.com",
            "Billing" => "tonguespaceinquiry@gmail.com", // Using general inquiry email for billing
            "Courses" => "tonguespaceinquiry@gmail.com",
            "Feedback" => "tonguespaceinquiry@gmail.com",
            _ => "tonguespaceinquiry@gmail.com" // Default to general inquiries
        };

        mailMessage.To.Add(recipientEmail);
        mailMessage.ReplyToList.Add(new MailAddress(form.Email, $"{form.FirstName} {form.LastName}"));

        await client.SendMailAsync(mailMessage);
    }

    private string BuildEmailBody(ContactFormModel form)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #667eea; border-bottom: 2px solid #667eea; padding-bottom: 10px;'>
                        New Contact Form Submission
                    </h2>
                    
                    <div style='background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='margin-top: 0; color: #333;'>Contact Information</h3>
                        <p><strong>Name:</strong> {form.FirstName} {form.LastName}</p>
                        <p><strong>Email:</strong> {form.Email}</p>
                        <p><strong>Preferred Language:</strong> {form.PreferredLanguage}</p>
                        <p><strong>Category:</strong> {form.Category}</p>
                        <p><strong>Subject:</strong> {form.Subject}</p>
                    </div>
                    
                    <div style='margin: 20px 0;'>
                        <h3 style='color: #333;'>Message</h3>
                        <div style='background: white; padding: 15px; border-left: 4px solid #667eea; border-radius: 4px;'>
                            {form.Message.Replace("\n", "<br>")}
                        </div>
                    </div>
                    
                    <div style='margin-top: 30px; padding: 15px; background: #e9ecef; border-radius: 8px; font-size: 12px; color: #6c757d;'>
                        <p style='margin: 0;'><strong>Submitted:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC</p>
                        <p style='margin: 5px 0 0 0;'><strong>User Agent:</strong> {HttpContext.Request.Headers.UserAgent}</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private async Task StoreContactFormAsync(ContactFormModel form)
    {
        // In a real application, you would store this in a database
        // For now, we'll just log the detailed information
        
        _logger.LogInformation("Contact Form Details: {@ContactForm}", new
        {
            form.FirstName,
            form.LastName,
            form.Email,
            form.Category,
            form.PreferredLanguage,
            form.Subject,
            MessageLength = form.Message?.Length ?? 0,
            SubmissionTime = DateTime.UtcNow,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        // Simulate async database operation
        await Task.Delay(50);
    }
}

public class ContactFormModel
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an inquiry type")]
    [Display(Name = "Inquiry Type")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Preferred Language")]
    public string PreferredLanguage { get; set; } = "English";

    [Required(ErrorMessage = "Subject is required")]
    [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
    [Display(Name = "Subject")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    [MinLength(10, ErrorMessage = "Message must be at least 10 characters long")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;
}