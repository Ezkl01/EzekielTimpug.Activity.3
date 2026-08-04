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
public class UpdateProfile : PageModel
{  

    private readonly ClinicAppointmentDbContext _dbContext; 

    public UpdateProfile(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;        
    }

    [BindProperty]
    public UserUpdateDto UserUpdateDto { get; set; } = new UserUpdateDto();

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

        UserUpdateDto.Username = user.UserName;
        UserUpdateDto.FirstName = user.FirstName;
        UserUpdateDto.LastName = user.LastName;
        UserUpdateDto.DateOfBirth = user.DateOfBirth;

        var profileImagePath = $"/users/{user.Id}.png";
        Console.WriteLine(System.IO.File.Exists(profileImagePath));
        if (System.IO.File.Exists(profileImagePath))
        {
            UserUpdateDto.ProfileImage = profileImagePath;
        }
        else
        {
            UserUpdateDto.ProfileImage = $"/users/default.png";// Path to the default profile image
        }        
        return Page();
    }

    public IActionResult OnPost()
    {
        if(!ModelState.IsValid)
        {
            return Page();
        }

        var today = DateTime.Today;
        var birthDate = UserUpdateDto.DateOfBirth!.Value;

        int age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        if (age <= 17)
        {
            ModelState.AddModelError(nameof(UserUpdateDto.DateOfBirth),
                "User must be more than 17 years old.");

            return Page();
        }


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
 
        user.FirstName = UserUpdateDto.FirstName;
        user.LastName = UserUpdateDto.LastName;
        user.DateOfBirth = (DateTime)UserUpdateDto.DateOfBirth!;

        _dbContext.SaveChanges();
        return RedirectToPage("/Account/Profile");
    }

}


public class UserUpdateDto
{    
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