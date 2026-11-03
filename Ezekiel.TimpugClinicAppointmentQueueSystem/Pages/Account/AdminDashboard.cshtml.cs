
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

[Authorize(Roles = "Admin")]
public class AdminDashboardModel : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;

    public AdminDashboardModel(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Dashboard totals
    public int TotalDoctors { get; set; }
    public int TotalUsers { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalPendingAppointments { get; set; }
    public int TotalConfirmedAppointments { get; set; }
    public int TotalCompletedAppointments { get; set; }
    public int TotalCancelledAppointments { get; set; }

    // Today's schedule
    public List<ClinicAppointment> TodayAppointments { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalDoctors = await _dbContext.Doctors
            .AsNoTracking()
            .CountAsync();

        TotalUsers = await _dbContext.Users
            .AsNoTracking()
            .CountAsync();

        TotalAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .CountAsync();

        TotalPendingAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .CountAsync(a => a.Status == "Pending" ||
                             a.Status == null ||
                             a.Status == "");

        TotalConfirmedAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .CountAsync(a => a.Status == "Confirmed");

        TotalCompletedAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .CountAsync(a => a.Status == "Completed");

        TotalCancelledAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .CountAsync(a => a.Status == "Cancelled");

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        TodayAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Date >= today && a.Date < tomorrow)
            .OrderBy(a => a.Time)
            .ToListAsync();
    }

    // Pending -> Confirmed
    public async Task<IActionResult> OnPostConfirmAsync(Guid id)
    {
        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            return NotFound();
        }

        if (appointment.Status == "Pending" ||
            string.IsNullOrWhiteSpace(appointment.Status))
        {
            appointment.Status = "Confirmed";
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    // Confirmed -> Completed
    public async Task<IActionResult> OnPostCompleteAsync(Guid id)
    {
        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            return NotFound();
        }

        if (appointment.Status == "Confirmed")
        {
            appointment.Complete();
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    // Pending or Confirmed -> Cancelled
    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            return NotFound();
        }

        if (appointment.Status == "Pending" ||
            appointment.Status == "Confirmed" ||
            string.IsNullOrWhiteSpace(appointment.Status))
        {
            appointment.Cancel();
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
