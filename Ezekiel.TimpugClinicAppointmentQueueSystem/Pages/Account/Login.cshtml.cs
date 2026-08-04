using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

public class Login : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;

    public Login(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
   public UserLoginDto UserLoginDto { get; set; } = new UserLoginDto(); 

    public async Task<IActionResult> OnPost()
    {
        
        if (!ModelState.IsValid)
        {
            ModelState.Clear();
            ModelState.AddModelError("", "Invalid Login");
            return Page();
        }

        var user = _dbContext.Set<User>().FirstOrDefault(u =>
                            u.UserName != null
                        && u.UserName!.ToLower() == UserLoginDto!.UserName!.ToLower());
        
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid Login");
            return Page();
        }

        var Password = _dbContext.Set<UserLoginInfo>().FirstOrDefault( ls =>
                            ls.UserId! == user.Id
                        && ls!.Key!.ToLower() == "password"
        );

        if (Password == null)
        {
            ModelState.AddModelError("", "Invalid Login.");
            return Page();
        }
        
        var loginStatus = _dbContext.Set<UserLoginInfo>().FirstOrDefault( ls =>
                            ls.UserId! == user.Id
                        && ls!.Key!.ToLower() == "LoginStatus"
        );

        if (loginStatus == null || loginStatus.Value!.ToLower() != "active")
        {
            ModelState.AddModelError("", "Your account is locked out. Please contact the administrator.");
            return Page();
        }
        
        if (BCrypt.Net.BCrypt.Verify(UserLoginDto.Password, Password!.Value!))
        {
            //login success
            if (loginStatus != null)
            {
                loginStatus.Value = "Active";
            }
            else
            {
                loginStatus = new UserLoginInfo(user.Id, "LoginStatus", "Active");
                _dbContext.Set<UserLoginInfo>().Add(loginStatus);
            }

            _dbContext.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? user.FirstName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id!.ToString()!)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );
            

            HttpContext.Session.SetString("UserLoginTime",DateTime.UtcNow.ToString()); // Store the UserId in session
            return RedirectToPage("/index"); // Redirect to the desired page after successful login
            
        }


        //login failed  
         var loginRetries = _dbContext.UserLoginInfos.FirstOrDefault( lr => 
                            lr.UserId == user.Id
                         && lr!.Key!.ToLower() == "loginretries"
        );

        if(loginRetries == null)
        {
            loginRetries = new UserLoginInfo(user.Id, "loginretries", "1");
            _dbContext.UserLoginInfos.Add(loginRetries);
            _dbContext.SaveChanges();
        }
        else
        {
            loginRetries.Value = (int.Parse(loginRetries.Value ?? "0") + 1).ToString();
            _dbContext.SaveChanges();
        }

        if(int.Parse(loginRetries.Value ?? "0") >= 3)
        {
            if(loginStatus != null)
            {
                loginStatus.Value = "LockedOut";
            }
            else
            {
                loginStatus = new UserLoginInfo(user.Id, "loginstatus", "LockedOut");
                _dbContext.UserLoginInfos.Add(loginStatus);
            }

            _dbContext.SaveChanges();
        }

            ModelState.AddModelError("","Invalid Login");
            return Page(); 
                }
}

public class UserLoginDto
{
    [Required(ErrorMessage = "Invalid Login")]
    public string? UserName { get; set; }
    [Required(ErrorMessage = "Invalid Login")]
    public string? Password { get; set; }
}