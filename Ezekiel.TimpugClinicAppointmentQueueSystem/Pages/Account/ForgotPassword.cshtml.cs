using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Helpers;
using Resend;

public class ForgotPassword : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;
    private readonly UserTokenService _userTokenService;
    private readonly IConfiguration _configuration;
    private readonly IResend _resend;
    private readonly ILogger<ForgotPassword> _logger;

    public ForgotPassword(
        ClinicAppointmentDbContext dbContext,
        UserTokenService userTokenService,
        IConfiguration configuration,
        IResend resend,
        ILogger<ForgotPassword> logger)
    {
        _dbContext = dbContext;
        _userTokenService = userTokenService;
        _configuration = configuration;
        _resend = resend;
        _logger = logger;
    }

    [BindProperty]
    public ForgotPasswordDto ForgotPasswordDto { get; set; }
        = new ForgotPasswordDto();

    public string? ResetToken { get; private set; }
    public string? SuccessMessage { get; private set; }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var username = ForgotPasswordDto.Username!.Trim();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.UserName != null &&
                u.UserName.ToLower() == username.ToLower());

        if (user == null)
        {
            ModelState.AddModelError(
                "ForgotPasswordDto.Username",
                "No account was found with this email address."
            );

            return Page();
        }

        var resetToken =
            _userTokenService.CreatePasswordResetToken(user.Id!.Value);

        ResetToken = resetToken;

        var resetUrl =
            $"http://localhost:5171/account/verify-forgot-password?token={resetToken}";

        try
        {
            var fromAddress =
                _configuration["Resend:From"]
                ?? "onboarding@resend.dev";

            await _resend.EmailSendAsync(
                new EmailMessage()
                {
                    From = fromAddress,

                    To = user.UserName!,

                    Subject = "Hello from School Events!",
                    HtmlBody = $@"
                        <h2>Password Reset</h2>
                        <p>You requested to reset your password.</p>
                        <p>
                            Click the button below to reset your password:
                        </p>
                        <p>
                            <a href='{resetUrl}'
                               style='
                                    display:inline-block;
                                    padding:10px 20px;
                                    background-color:#0d6efd;
                                    color:white;
                                    text-decoration:none;
                                    border-radius:5px;
                               '>
                                Reset Password
                            </a>
                        </p>
                        <p>
                            If you did not request a password reset,
                            you can safely ignore this email.
                        </p>
                    "
                });
        }
        catch (Exception ex)
        {
            // Log the actual error
            _logger.LogError(
                ex,
                "Failed to send forgot password email to {Recipient}.",
                user.UserName
            );

            // Show error to the user
            ModelState.AddModelError(
                "ForgotPasswordDto.Username",
                "We couldn't send the password reset email. Please try again."
            );

            return Page();
        }

        SuccessMessage = "We have sent you an email with a link to reset your password.";
        return Page();
    }
}

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Please input your email address.")]

    [EmailAddress(ErrorMessage = "Invalid email address.")]

    public string? Username { get; set; }
}

