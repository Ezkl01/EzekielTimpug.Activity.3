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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Helpers;


public class Invite : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IResend _resend;
    private readonly ILogger<Invite> _logger;
    private readonly UserTokenService userTokenService;
    public Invite(ClinicAppointmentDbContext dbContext, IConfiguration configuration, IResend resend, UserTokenService userTokenService, ILogger<Invite> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _resend = resend;
        _logger = logger;
         this.userTokenService = userTokenService;
    }

    [BindProperty]
   public UserInviteDto UserInviteDto { get; set; } = new UserInviteDto(); 

    public async Task<IActionResult> OnPost()
    {
       if(!ModelState.IsValid)
        {
            return Page();
        }

        var dateOfBirth = UserInviteDto!.DateOfBirth!.Value;
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        if(age < 18)
        {
            ModelState.AddModelError("Date Of Birth", "You must be 18 years old to Register.");
            return Page();
        }
        User? existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName!.ToLower() == UserInviteDto!.UserName!.ToLower());
        if (existingUser != null)
        {;;
            ModelState.AddModelError("Username", "Username is already taken.");
            return Page();
        }

        var newUser = new User(UserInviteDto!.UserName!, UserInviteDto!.DateOfBirth!.Value)
        {
            DateOfBirth = UserInviteDto!.DateOfBirth!.Value,
            UserName = UserInviteDto!.UserName,
            FirstName = UserInviteDto!.FirstName,
            LastName = UserInviteDto!.LastName
        };

        await _dbContext.Users.AddAsync(newUser);

        var userLoginStatus = new UserLoginInfo(newUser.Id, "loginstatus", "active");
        var loginRetries = new UserLoginInfo(newUser.Id, "loginretries", "0");
        var role = new UserLoginInfo(newUser.Id, "role", "user");

        await _dbContext.UserLoginInfos.AddRangeAsync(userLoginStatus, loginRetries, role);

        await _dbContext.SaveChangesAsync();

        var token = userTokenService.CreateInviteToken(newUser.Id!.Value);
        var inviteUrl = $"http://localhost:5171/account/accept-invite?token={token}";

        try
        {
            var fromAddress = _configuration["Resend:From"] ?? "onboarding@resend.dev";

          await _resend.EmailSendAsync(new EmailMessage()
            {
                From = fromAddress,
                To = newUser.UserName!,
                Subject = "Hello from ClinicAppointmentSystem!",
                HtmlBody = $@"
                            <p>Admin is inviting you to ClinicAppointmentSystem, <strong>{newUser.FirstName} {newUser.LastName}</strong>!</p>
                            <p>You can accept the invitation by clicking the link below:</p>
                            <p><a href='{inviteUrl}'>Accept Invitation</a></p>"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Recipient}.", newUser.UserName);
        }

        return RedirectToPage("/account/login");
        
    }
}

public class UserInviteDto
{
    [Required(ErrorMessage = "Username is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? UserName { get; set; }
  
    [Required(ErrorMessage = "First name is required")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date, ErrorMessage = "Invalid date format")]
    public DateTime? DateOfBirth { get; set; }
   
}