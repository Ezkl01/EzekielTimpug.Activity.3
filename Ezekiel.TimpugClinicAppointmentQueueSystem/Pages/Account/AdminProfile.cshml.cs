using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

[Authorize(Roles = "Admin")]
public class AdminProfile : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public AdminProfile(
        ClinicAppointmentDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }


    [BindProperty]
    public AdminUserDto UserDto { get; set; } = new AdminUserDto();


    public IActionResult OnGet()
    {
        // Get the currently logged-in admin ID
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );


        // If no user is logged in
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }


        // Find the admin in the database
        var user = _dbContext.Users
            .FirstOrDefault(u => u.Id.ToString() == userId);


        // User not found
        if (user == null)
        {
            return NotFound();
        }


        // Extra security check
        var role = User.FindFirstValue(
            ClaimTypes.Role
        );


        // Only allow Admin
        if (role != "Admin")
        {
            return Redirect("/user/dashboard");
        }


        // Load admin information
        UserDto.Username = user.UserName;

        UserDto.FirstName = user.FirstName;

        UserDto.LastName = user.LastName;

        UserDto.DateOfBirth = user.DateOfBirth;


        // =========================================
        // PROFILE IMAGE
        // =========================================

        var avatarDiskPath = Path.Combine(
            _environment.WebRootPath,
            "users",
            $"{user.Id}.png"
        );


        // If the admin has a profile picture
        if (System.IO.File.Exists(avatarDiskPath))
        {
            UserDto.ProfileImage =
                $"/users/{user.Id}.png?v={DateTime.UtcNow.Ticks}";
        }

        // Default profile picture
        else
        {
            UserDto.ProfileImage =
                "/users/default.png";
        }


        return Page();
    }
}


/* =========================================
   USER DTO
========================================= */

public class AdminUserDto
{
    [Required(ErrorMessage = "Username is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Username { get; set; }


    [Required(ErrorMessage = "First Name is required.")]
    public string? FirstName { get; set; }


    [Required(ErrorMessage = "Last Name is required.")]
    public string? LastName { get; set; }


    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(
        DataType.Date,
        ErrorMessage = "Invalid date format."
    )]
    public DateTime? DateOfBirth { get; set; }


    public string? ProfileImage { get; set; }
}