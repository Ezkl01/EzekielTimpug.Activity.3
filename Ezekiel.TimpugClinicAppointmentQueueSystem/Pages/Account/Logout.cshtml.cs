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

public class Logout : PageModel
{    
    private readonly ClinicAppointmentDbContext _dbContext; 

    public Logout(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> OnGet()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToPage("/Account/Login");
    }
}
