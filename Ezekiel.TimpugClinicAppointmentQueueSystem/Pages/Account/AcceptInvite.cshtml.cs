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
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Helpers;
using Resend;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;
public class AcceptInvite : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IResend _resend;
    private readonly ILogger<AcceptInvite> _logger;
    private readonly InviteTokenService inviteTokenService;

    public AcceptInvite (ClinicAppointmentDbContext dbContext, IConfiguration configuration, IResend resend, InviteTokenService inviteTokenService, ILogger<AcceptInvite> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _resend = resend;
        _logger = logger;
        this.inviteTokenService = inviteTokenService;
    }

    [BindProperty]
    public UserAcceptInviteDto? UserAcceptInviteDto { get; set; }

    public async Task<IActionResult> OnGet(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Forbidden");
        }

        var inviteToken = inviteTokenService.ValidateInviteToken(token);                     
        if (inviteToken == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired invite token.");
            return RedirectToPage("/Account/Forbidden");
        }

        this.UserAcceptInviteDto = new UserAcceptInviteDto
        {
           UserId = inviteToken.UserId
        };

        return Page();
    }


    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == UserAcceptInviteDto!.UserId);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            return Page();
        }
        var passwordErrors = PasswordValidator.Validate(UserAcceptInviteDto!.Password!);
        if (passwordErrors.Any())
        {
            foreach (var error in passwordErrors)
            {
                ModelState.AddModelError("Weak Password", error);
            }

            return Page();
        }        

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(UserAcceptInviteDto!.Password!);
        Console.WriteLine($"Clear Text Password: {UserAcceptInviteDto!.Password!}"); 
        Console.WriteLine($"Hashed Password: {hashedPassword}"); 

        var userPassword = new UserLoginInfo(user.Id, "password", hashedPassword);

        await _dbContext.UserLoginInfos.AddAsync(userPassword);
        await _dbContext.SaveChangesAsync();

        return RedirectToPage("/Account/Login");
    }
}

public class UserAcceptInviteDto
{
    [Required(ErrorMessage = "UserId is required.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }
    
    [Required(ErrorMessage = "Confirm Password is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }

}