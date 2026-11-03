
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

[Authorize(Roles = "User")]
public class ReportsModel : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;

    public ReportsModel(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ==========================================
    // APPOINTMENT RECORDS
    // ==========================================

    public List<ClinicAppointment> ReportAppointments { get; set; } = new();


    // ==========================================
    // APPOINTMENT STATISTICS
    // ==========================================

    public int TotalAppointments { get; set; }

    public int TotalPending { get; set; }

    public int TotalConfirmed { get; set; }

    public int TotalCompleted { get; set; }

    public int TotalCancelled { get; set; }


    // ==========================================
    // LOAD REPORTS
    // ==========================================

    public async Task OnGetAsync()
    {
        // Get logged-in user's ID
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return;
        }


        // ==========================================
        // FIND USER ACCOUNT
        // ==========================================

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == userId &&
                !u.IsDeleted
            );

        if (user == null)
        {
            return;
        }


        // ==========================================
        // GET PATIENT FULL NAME
        // ==========================================

        var fullName = string.Join(
            " ",
            new[]
            {
                user.FirstName,
                user.LastName
            }.Where(name => !string.IsNullOrWhiteSpace(name))
        ).Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return;
        }


        // ==========================================
        // GET PATIENT APPOINTMENTS
        // ==========================================

        var appointments = _dbContext.Appointments
            .AsNoTracking()
            .Where(a =>
                a.Patient != null &&
                a.Patient.Trim().ToLower() == fullName.ToLower()
            );


        // ==========================================
        // TOTAL APPOINTMENTS
        // ==========================================

        TotalAppointments = await appointments.CountAsync();


        // ==========================================
        // PENDING APPOINTMENTS
        // ==========================================

        TotalPending = await appointments.CountAsync(a =>
            a.Status == "Pending"
        );


        // ==========================================
        // CONFIRMED APPOINTMENTS
        // ==========================================

        TotalConfirmed = await appointments.CountAsync(a =>
            a.Status == "Confirmed"
        );


        // ==========================================
        // COMPLETED APPOINTMENTS
        // ==========================================

        TotalCompleted = await appointments.CountAsync(a =>
            a.Status == "Completed"
        );


        // ==========================================
        // CANCELLED APPOINTMENTS
        // ==========================================

        TotalCancelled = await appointments.CountAsync(a =>
            a.Status == "Cancelled"
        );


        // ==========================================
        // LOAD TABLE DATA
        // ==========================================

        ReportAppointments = await appointments
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.Time)
            .ToListAsync();
    }
}
