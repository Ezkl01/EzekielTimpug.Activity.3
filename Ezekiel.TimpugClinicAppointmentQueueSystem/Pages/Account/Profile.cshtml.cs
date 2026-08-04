using System.Diagnostics;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;
public class Profile : PageModel
{  

    private readonly ClinicAppointmentDbContext _dbContext; 

    public Profile(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;        
    }

    [BindProperty]
    public UserDto UserDto { get; set; } = new UserDto();

    public IActionResult OnGet()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null)
        {
            return NotFound();
        }

        UserDto.Username = user.UserName;
        UserDto.FirstName = user.FirstName;
        UserDto.LastName = user.LastName;
        UserDto.DateOfBirth = user.DateOfBirth;

        var profileImagePath = $"/users/{user.Id}.png";
        Console.WriteLine(System.IO.File.Exists(profileImagePath));
        if (System.IO.File.Exists(profileImagePath))
        {
            UserDto.ProfileImage = profileImagePath;
        }
        else
        {
            UserDto.ProfileImage = $"/users/default.png";// Path to the default profile image
        }        
        return Page();
    }
}

public class UserDto
{
    
    [Required(ErrorMessage = "Username is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "First Name is required.")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required.")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime? DateOfBirth { get; set; }
    public string? ProfileImage { get; set; }

}