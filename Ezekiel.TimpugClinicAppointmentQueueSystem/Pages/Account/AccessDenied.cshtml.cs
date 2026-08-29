
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

public class AccessDenied : PageModel
{
    public AccessDenied(ClinicAppointmentDbContext dbContext)
    {
    }
}