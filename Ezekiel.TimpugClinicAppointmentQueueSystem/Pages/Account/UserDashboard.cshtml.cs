using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

[Authorize(Roles = "User")]
public class UserDashboardModel : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;

    public UserDashboardModel(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ==============================
    // DASHBOARD STATISTICS
    // ==============================

    public int TotalDoctors { get; set; }

    public int TotalUsers { get; set; }

    public int TotalAppointments { get; set; }

    public int TotalPendingAppointments { get; set; }

    public int TotalConfirmedAppointments { get; set; }

    public int TotalCompletedAppointments { get; set; }

    public int TotalCancelledAppointments { get; set; }


    // ==============================
    // TODAY'S APPOINTMENTS
    // ==============================

    public List<ClinicAppointment> TodayAppointments { get; set; } = new();


    // ==============================
    // PAGE LOAD
    // ==============================

    public async Task OnGetAsync()
    {
        // ==========================================
        // TOTAL DOCTORS
        // ==========================================

        TotalDoctors = await _dbContext.Doctors
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .CountAsync();


        // ==========================================
        // TOTAL USERS
        // ==========================================

        TotalUsers = await _dbContext.Users
            .AsNoTracking()
            .CountAsync();


        // ==========================================
        // ALL APPOINTMENTS
        // ==========================================

        var appointments = _dbContext.Appointments
            .AsNoTracking();


        // ==========================================
        // TOTAL APPOINTMENTS
        // ==========================================

        TotalAppointments = await appointments
            .CountAsync();


        // ==========================================
        // PENDING APPOINTMENTS
        // ==========================================

        TotalPendingAppointments = await appointments
            .CountAsync(a =>
                a.Status == "Pending" ||
                a.Status == null ||
                a.Status == ""
            );


        // ==========================================
        // CONFIRMED APPOINTMENTS
        // ==========================================

        TotalConfirmedAppointments = await appointments
            .CountAsync(a =>
                a.Status == "Confirmed"
            );


        // ==========================================
        // COMPLETED APPOINTMENTS
        // ==========================================

        TotalCompletedAppointments = await appointments
            .CountAsync(a =>
                a.Status == "Completed"
            );


        // ==========================================
        // CANCELLED APPOINTMENTS
        // ==========================================

        TotalCancelledAppointments = await appointments
            .CountAsync(a =>
                a.Status == "Cancelled"
            );


        // ==========================================
        // TODAY'S APPOINTMENTS
        // ==========================================

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        TodayAppointments = await appointments
            .Where(a =>
                a.Date >= today &&
                a.Date < tomorrow
            )
            .OrderBy(a => a.Time)
            .ToListAsync();
    }
}