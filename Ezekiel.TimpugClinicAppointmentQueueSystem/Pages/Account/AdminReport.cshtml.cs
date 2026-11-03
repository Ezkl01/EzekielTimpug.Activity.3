
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

[Authorize(Roles = "Admin")]
public class AdminReportsModel : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;

    public AdminReportsModel(ClinicAppointmentDbContext dbContext)
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
    // LOAD ADMIN REPORTS
    // ==========================================

    public async Task OnGetAsync()
    {
        // Get all appointments
        var appointments = _dbContext.Appointments
            .AsNoTracking();


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
        // LOAD ALL APPOINTMENT RECORDS
        // ==========================================

        ReportAppointments = await appointments
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.Time)
            .ToListAsync();
    }
}
