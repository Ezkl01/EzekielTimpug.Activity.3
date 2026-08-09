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
using Microsoft.EntityFrameworkCore;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;
using Resend;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Helpers;
public class VerifyForgotPassword : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IResend _resend;
    private readonly ILogger<VerifyForgotPassword> _logger;
    private readonly UserTokenService userTokenService;

    public VerifyForgotPassword(ClinicAppointmentDbContext dbContext, IConfiguration configuration, IResend resend, UserTokenService userTokenService, ILogger<VerifyForgotPassword> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _resend = resend;
        _logger = logger;
        this.userTokenService = userTokenService;
    }

    [BindProperty]
    public VerifyForgotPasswordDto? VerifyForgotPasswordDto { get; set; }

    public async Task<IActionResult> OnGet(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Forbidden");
        }

        var resetToken = userTokenService.ValidateResetToken(token);
        if (resetToken == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired reset token.");
            return RedirectToPage("/Account/Forbidden");
        }

        this.VerifyForgotPasswordDto = new VerifyForgotPasswordDto
        {
           UserId = resetToken.UserId
        };

        return Page();
    }


    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == VerifyForgotPasswordDto!.UserId);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            return Page();
        }
        var passwordErrors = PasswordValidator.Validate(VerifyForgotPasswordDto!.Password!);
        if (passwordErrors.Any())
        {
            foreach (var error in passwordErrors)
            {
                ModelState.AddModelError("Weak Password", error);
            }

            return Page();
        }        

        // Hash the password using BCrypt
        //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(VerifyForgotPasswordDto!.Password!);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(VerifyForgotPasswordDto!.Password!);
        Console.WriteLine($"Clear Text Password: {VerifyForgotPasswordDto!.Password!}"); 
        Console.WriteLine($"Hashed Password: {hashedPassword}"); 

        var userPassword = _dbContext.UserLoginInfos.FirstOrDefault(u => u.UserId == user.Id && u.Key == "password");
        if (userPassword != null)
        {
            userPassword.Value = hashedPassword;
        }
        else
        {
            userPassword = new UserLoginInfo(user.Id, "password", hashedPassword);
            _dbContext.UserLoginInfos.Add(userPassword);
        }
        
        await _dbContext.SaveChangesAsync();

        return RedirectToPage("/Account/Login");
    }
}

public class VerifyForgotPasswordDto
{
    [Required(ErrorMessage = "UserId is required.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }
    
    [Required(ErrorMessage = "Confirm Password is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }

}