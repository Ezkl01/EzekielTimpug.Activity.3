using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

public class ChangePassword : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ChangePassword(
        ClinicAppointmentDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [BindProperty]
    public ChangePasswordDto ChangePasswordDto { get; set; } = new();

    private IActionResult? LoadUserData()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return RedirectToPage("/Account/Login");

        var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);

        if (user == null)
            return NotFound();

        ChangePasswordDto.Username = user.UserName;

        var avatarDiskPath = Path.Combine(
            _environment.WebRootPath,
            "users",
            $"{user.Id}.png");

        if (System.IO.File.Exists(avatarDiskPath))
        {
            ChangePasswordDto.ProfileImage =
                $"/users/{user.Id}.png?v={DateTime.UtcNow.Ticks}";
        }
        else
        {
            ChangePasswordDto.ProfileImage =
                $"/users/default.png?v={DateTime.UtcNow.Ticks}";
        }

        return null;
    }

    public IActionResult OnGet()
    {
        var result = LoadUserData();

        if (result != null)
            return result;

        return Page();
    }

    public IActionResult OnPost()
    {
        var result = LoadUserData();

        if (result != null)
            return result;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = _dbContext.Users.FirstOrDefault(
            u => u.Id.ToString() == userId);

        if (user == null)
            return NotFound();

        // Password strength validation
        var passwordErrors =
            PasswordValidator.Validate(ChangePasswordDto.NewPassword!);

        if (passwordErrors.Any())
        {
            foreach (var error in passwordErrors)
            {
               ModelState.AddModelError("ChangePasswordDto.NewPassword", error);
            }

            return Page();
        }

        var userPassword = _dbContext.UserLoginInfos.FirstOrDefault(
            u => u.UserId == user.Id &&
                 u.Key == "password");

        if (userPassword == null)
        {
            ModelState.AddModelError(
                "",
                "Password record not found.");

            return Page();
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(
                ChangePasswordDto.CurrentPassword,
                userPassword.Value))
        {
            ModelState.AddModelError(
                "ChangePasswordDto.CurrentPassword",
                "Current password is incorrect.");

            return Page();
        }

        // Prevent using the same password
        if (BCrypt.Net.BCrypt.Verify(
                ChangePasswordDto.NewPassword,
                userPassword.Value))
        {
            ModelState.AddModelError(
                "ChangePasswordDto.NewPassword",
                "Your new password must be different from your current password.");

            return Page();
        }

        // Save new password
        userPassword.Value =
            BCrypt.Net.BCrypt.HashPassword(
                ChangePasswordDto.NewPassword);

        _dbContext.SaveChanges();

        return RedirectToPage("/Account/Profile");
    }
}

public class ChangePasswordDto
{
    public string? Username { get; set; }

    [Required(ErrorMessage = "Current Password is required.")]
    public string? CurrentPassword { get; set; }

    [Required(ErrorMessage = "New Password is required.")]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm Password is required.")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmNewPassword { get; set; }

    public string? ProfileImage { get; set; }
}