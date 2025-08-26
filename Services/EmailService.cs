// Services/EmailService.cs
using CoursePLatform.Services;

namespace CoursePLatform.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        
        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendInstructorApprovalEmailAsync(string email, string firstName)
        {
            try
            {
                var subject = "🎉 Congratulations! Your TongueSpace Instructor Application is Approved";
                var body = GetApprovalEmailTemplate(firstName);

                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Approval email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending approval email to: {Email}", email);
                throw;
            }
        }

        public async Task SendInstructorRejectionEmailAsync(string email, string firstName, string reason, string? notes)
        {
            try
            {
                var subject = "TongueSpace Instructor Application Update";
                var body = GetRejectionEmailTemplate(firstName, reason, notes);

                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Rejection email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending rejection email to: {Email}", email);
                throw;
            }
        }

        public async Task SendApplicationConfirmationEmailAsync(string email, string firstName)
        {
            try
            {
                var subject = "Application Received - TongueSpace Instructor Program";
                var body = GetConfirmationEmailTemplate(firstName);

                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Confirmation email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending confirmation email to: {Email}", email);
                throw;
            }
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            // For demo purposes, we'll just log the email content
            // In production, you'd use SendGrid, SMTP, or another email service
            
            _logger.LogInformation("=== EMAIL NOTIFICATION ===");
            _logger.LogInformation("To: {Email}", email);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {Body}", body);
            _logger.LogInformation("========================");

            // Simulate sending delay
            await Task.Delay(100);
            
            // TODO: Implement actual email sending
            // Example with System.Net.Mail:
            /*
            using var client = new SmtpClient(_configuration["SMTP:Host"], int.Parse(_configuration["SMTP:Port"]));
            client.Credentials = new NetworkCredential(_configuration["SMTP:Username"], _configuration["SMTP:Password"]);
            client.EnableSsl = true;
            
            var message = new MailMessage("noreply@tonguespace.com", email, subject, body)
            {
                IsBodyHtml = true
            };
            
            await client.SendMailAsync(message);
            */
        }

        private string GetApprovalEmailTemplate(string firstName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Congratulations, {firstName}!</h1>
            <p>Welcome to TongueSpace Instructors</p>
        </div>
        <div class='content'>
            <p>Dear {firstName},</p>
            
            <p>We are delighted to inform you that your instructor application has been <strong>approved</strong>!</p>
            
            <p>Thank you for your interest in joining our platform. After careful review of your qualifications and experience, we believe you will be a valuable addition to our community of language instructors.</p>
            
            <h3>What's Next?</h3>
            <ul>
                <li>✅ You now have instructor access to your dashboard</li>
                <li>✅ You can start creating and publishing courses</li>
                <li>✅ Begin earning from student enrollments</li>
                <li>✅ Access instructor resources and support</li>
            </ul>
            
            <p>Get started by accessing your instructor dashboard:</p>
            <a href='https://tonguespace.com/instructor-dashboard' class='button'>Access Instructor Dashboard</a>
            
            <p>We're excited to have you as part of the TongueSpace instructor community and look forward to seeing the amazing courses you'll create!</p>
            
            <p>If you have any questions, please don't hesitate to contact our support team.</p>
            
            <p>Best regards,<br>The TongueSpace Team</p>
        </div>
        <div class='footer'>
            <p>© 2025 TongueSpace. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetRejectionEmailTemplate(string firstName, string reason, string? notes)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #6c757d; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .feedback-box {{ background: #fff; padding: 20px; border-left: 4px solid #dc3545; margin: 20px 0; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>TongueSpace Instructor Application</h1>
            <p>Application Status Update</p>
        </div>
        <div class='content'>
            <p>Dear {firstName},</p>
            
            <p>Thank you for your interest in becoming a TongueSpace instructor and for taking the time to submit your application.</p>
            
            <p>After careful review of your application, we regret to inform you that we are unable to approve your instructor application at this time.</p>
            
            <div class='feedback-box'>
                <h3>Reason for Decision:</h3>
                <p><strong>{GetReasonText(reason)}</strong></p>
                
                {(!string.IsNullOrEmpty(notes) ? $@"
                <h3>Additional Feedback:</h3>
                <p>{notes}</p>
                " : "")}
            </div>
            
            <p>Please don't be discouraged by this decision. We encourage you to:</p>
            <ul>
                <li>📚 Continue developing your teaching skills and qualifications</li>
                <li>🎓 Consider obtaining relevant certifications</li>
                <li>⏰ Reapply in the future when you have gained more experience</li>
            </ul>
            
            <p>You are welcome to submit a new application once you have addressed the areas mentioned above.</p>
            
            <a href='https://tonguespace.com/apply-instructor' class='button'>Apply Again in the Future</a>
            
            <p>We appreciate your understanding and wish you the best in your teaching endeavors.</p>
            
            <p>Best regards,<br>The TongueSpace Team</p>
        </div>
        <div class='footer'>
            <p>© 2025 TongueSpace. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetConfirmationEmailTemplate(string firstName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .status-box {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📝 Application Received!</h1>
            <p>Thank you for applying to TongueSpace</p>
        </div>
        <div class='content'>
            <p>Dear {firstName},</p>
            
            <p>Thank you for your interest in becoming a TongueSpace instructor!</p>
            
            <p>We have successfully received your instructor application and our team is excited to review your qualifications.</p>
            
            <div class='status-box'>
                <h3>⏰ What happens next?</h3>
                <ul>
                    <li><strong>Review Period:</strong> 2-3 business days</li>
                    <li><strong>Evaluation:</strong> Our team will carefully review your experience and qualifications</li>
                    <li><strong>Communication:</strong> We'll contact you via email or WhatsApp with our decision</li>
                    <li><strong>Next Steps:</strong> If approved, we'll provide onboarding materials and dashboard access</li>
                </ul>
            </div>
            
            <p>During the review process, please ensure:</p>
            <ul>
                <li>📧 Keep an eye on your email inbox (including spam folder)</li>
                <li>📱 Your WhatsApp is available for contact</li>
                <li>⏳ Be patient as we carefully review each application</li>
            </ul>
            
            <p>We appreciate your patience and look forward to potentially welcoming you to our instructor community!</p>
            
            <p>Best regards,<br>The TongueSpace Team</p>
        </div>
        <div class='footer'>
            <p>© 2025 TongueSpace. All rights reserved.</p>
            <p>Need help? Contact us at support@tonguespace.com</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetReasonText(string reason)
        {
            return reason switch
            {
                "insufficient-qualifications" => "Insufficient teaching qualifications or certifications for the platform requirements.",
                "incomplete-application" => "Application was incomplete or missing required information.",
                "language-proficiency" => "Language proficiency level does not meet our current standards.",
                "inappropriate-content" => "Application content did not meet our community guidelines.",
                _ => reason ?? "Application did not meet current requirements."
            };
        }
    }
}