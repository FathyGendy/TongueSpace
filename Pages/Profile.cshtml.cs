using CoursePLatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace CoursePLatform.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _hostingEnvironment; // Inject the hosting environment

        public ProfileModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _hostingEnvironment = hostingEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public IFormFile? ProfilePicture { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = "/images/default-avatar.jpg";
        public int CoursesEnrolled { get; set; }
        public int StreakDays { get; set; }
        public int CertificatesEarned { get; set; }
        public int AverageScore { get; set; }
        public DateTime? MemberSince { get; set; }
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            [StringLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Last Name")]
            [StringLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Phone]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [StringLength(100)]
            [Display(Name = "Location")]
            public string? Location { get; set; }

            [StringLength(500)]
            [Display(Name = "Bio")]
            public string? Bio { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadUserDataAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdatePersonalInfoAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors and try again.";
                await LoadUserDataAsync(user);
                return Page();
            }
            
            try
            {
                // Update user properties, but NOT the profile picture
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.PhoneNumber = Input.PhoneNumber;

                if (HasProperty(user, "Location"))
                {
                    SetProperty(user, "Location", Input.Location);
                }
                if (HasProperty(user, "Bio"))
                {
                    SetProperty(user, "Bio", Input.Bio);
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    ErrorMessage = "Failed to update personal information.";
                    await LoadUserDataAsync(user);
                    return Page();
                }

                if (Input.Email != user.Email)
                {
                    var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                    if (!setEmailResult.Succeeded)
                    {
                        foreach (var error in setEmailResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        ErrorMessage = "Failed to update email.";
                        await LoadUserDataAsync(user);
                        return Page();
                    }

                    var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.Email);
                    if (!setUserNameResult.Succeeded)
                    {
                        foreach (var error in setUserNameResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        ErrorMessage = "Failed to update username.";
                        await LoadUserDataAsync(user);
                        return Page();
                    }
                }

                SuccessMessage = "Your personal information has been updated successfully.";
                await _signInManager.RefreshSignInAsync(user);
                await LoadUserDataAsync(user);
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during personal info update: {ex.Message}");
                ErrorMessage = "An error occurred while updating your personal information.";
                await LoadUserDataAsync(user);
                return Page();
            }
        }

        // New handler for just the profile picture upload
        public async Task<IActionResult> OnPostUpdateProfilePictureAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Check if a file was uploaded and it's valid
            if (ProfilePicture == null || ProfilePicture.Length == 0)
            {
                ErrorMessage = "No profile picture was selected.";
                await LoadUserDataAsync(user);
                return Page();
            }

            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "profiles");

            try
            {
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(ProfilePicture.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(fileStream);
                }

                // Delete old profile picture if it exists
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl != "/images/default-avatar.jpg")
                {
                    var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.ProfilePictureUrl = $"/uploads/profiles/{uniqueFileName}";
                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    SuccessMessage = "Your profile picture has been updated successfully.";
                    await _signInManager.RefreshSignInAsync(user);
                }
                else
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    ErrorMessage = "Failed to update profile picture.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during profile picture update: {ex.Message}");
                ErrorMessage = "An error occurred while updating your profile picture.";
            }

            await LoadUserDataAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadUserDataAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnGetExportDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userData = new
            {
                PersonalInfo = new
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Location = GetProperty<string?>(user, "Location"),
                    Bio = GetProperty<string?>(user, "Bio"),
                    MemberSince = MemberSince,
                    Role = Role
                },
                Statistics = new
                {
                    CoursesEnrolled,
                    StreakDays,
                    CertificatesEarned,
                    AverageScore
                },
                ExportDate = DateTime.UtcNow
            };

            var jsonData = JsonSerializer.Serialize(userData, new JsonSerializerOptions { WriteIndented = true });
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

            return File(bytes, "application/json", $"tonguespace-data-{user.FirstName}-{DateTime.UtcNow:yyyyMMdd}.json");
        }

        private async Task LoadUserDataAsync(ApplicationUser user)
        {
            FirstName = user.FirstName ?? string.Empty;
            LastName = user.LastName ?? string.Empty;
            Role = GetProperty<string>(user, "Role") ?? string.Empty;
            MemberSince = GetProperty<DateTime?>(user, "CreatedAt");
            ProfilePictureUrl = GetProperty<string?>(user, "ProfilePictureUrl") ?? "/images/default-avatar.jpg";

            Input.FirstName = user.FirstName ?? string.Empty;
            Input.LastName = user.LastName ?? string.Empty;
            Input.Email = user.Email ?? string.Empty;
            Input.PhoneNumber = user.PhoneNumber;

            Input.Location = GetProperty<string?>(user, "Location");
            Input.Bio = GetProperty<string?>(user, "Bio");

            CoursesEnrolled = await GetCoursesEnrolledCountAsync(user.Id);
            StreakDays = await GetStreakDaysAsync(user.Id);
            CertificatesEarned = await GetCertificatesEarnedAsync(user.Id);
            AverageScore = await GetAverageScoreAsync(user.Id);
        }

        private bool HasProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        private void SetProperty(object obj, string propertyName, object? value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
        }

        private T? GetProperty<T>(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null || !property.CanRead)
            {
                return default;
            }

            var value = property.GetValue(obj);
            if (value is T)
            {
                return (T)value;
            }

            if (value == null)
            {
                return default;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        private async Task<int> GetCoursesEnrolledCountAsync(string userId)
        {
            await Task.Delay(1);
            return 5;
        }

        private async Task<int> GetStreakDaysAsync(string userId)
        {
            await Task.Delay(1);
            return 127;
        }

        private async Task<int> GetCertificatesEarnedAsync(string userId)
        {
            await Task.Delay(1);
            return 3;
        }

        private async Task<int> GetAverageScoreAsync(string userId)
        {
            await Task.Delay(1);
            return 89;
        }
    }
}