using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

[Authorize]
public class AppointmentsModel : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;

    public AppointmentsModel(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // =========================================================
    // APPOINTMENTS
    // =========================================================

    public List<ClinicAppointment> Appointments { get; set; } = new();

    // =========================================================
    // PAGINATION
    // =========================================================

    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 15;

    public int TotalPages { get; set; }

    public int TotalRecords { get; set; }

    // =========================================================
    // SEARCH
    // =========================================================

    public string Keyword { get; set; } = "";

    // =========================================================
    // ADD APPOINTMENT INPUT
    // =========================================================

    [BindProperty]
    public AddAppointmentInput Input { get; set; } = new();

    public class AddAppointmentInput
    {
        [Required(ErrorMessage = "Please enter the patient code.")]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a valid patient code.")]
        [Display(Name = "Patient Code")]
        public int PatientCode { get; set; }

        [Required(ErrorMessage = "Please select an appointment date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please select an appointment time.")]
        [DataType(DataType.Time)]
        [Display(Name = "Appointment Time")]
        public TimeSpan Time { get; set; }

        [Required(ErrorMessage = "Please enter the patient's name.")]
        [StringLength(150)]
        [Display(Name = "Patient Name")]
        public string Patient { get; set; } = "";

        [Required(ErrorMessage = "Please enter the patient's contact number.")]
        [StringLength(50)]
        [Display(Name = "Contact Number")]
        public string ContactInfo { get; set; } = "";

        [Required(ErrorMessage = "Please enter the doctor's name.")]
        [StringLength(150)]
        public string Doctor { get; set; } = "";

        [StringLength(500)]
        public string Notes { get; set; } = "";
    }

    // =========================================================
    // GET APPOINTMENTS
    // =========================================================

    public async Task OnGetAsync(
        int pageIndex = 1,
        string keyword = "")
    {
        // Keep the search keyword
        Keyword = keyword?.Trim() ?? "";

        // Make sure page index is at least 1
        PageIndex = Math.Max(1, pageIndex);

        // Build the search query
        var query = BuildSearchQuery();

        // Count ALL matching records
        TotalRecords = await query.CountAsync();

        // Calculate total pages
        TotalPages = (int)Math.Ceiling(
            TotalRecords / (double)PageSize
        );

        // If there are records but requested page is too high,
        // move to the last available page.
        if (TotalPages > 0 && PageIndex > TotalPages)
        {
            PageIndex = TotalPages;
        }

        // If there are no records, keep page at 1
        if (TotalPages == 0)
        {
            PageIndex = 1;
        }

        // Load the current page
        await LoadAppointmentsAsync();
    }

    // =========================================================
    // ADD APPOINTMENT
    // =========================================================

    public async Task<IActionResult> OnPostAddAppointmentAsync()
    {
        // Prevent appointments in the past
        if (Input.Date.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                "Input.Date",
                "Appointment date cannot be in the past."
            );
        }

        // Validate appointment time
        if (Input.Time < TimeSpan.Zero ||
            Input.Time >= TimeSpan.FromDays(1))
        {
            ModelState.AddModelError(
                "Input.Time",
                "Please select a valid appointment time."
            );
        }

        // If validation failed
        if (!ModelState.IsValid)
        {
            Keyword = "";
            PageIndex = 1;

            TotalRecords = await _dbContext
                .Set<ClinicAppointment>()
                .AsNoTracking()
                .CountAsync();

            TotalPages = (int)Math.Ceiling(
                TotalRecords / (double)PageSize
            );

            await LoadAppointmentsAsync();

            return Page();
        }

        // Create appointment
        var appointment = new ClinicAppointment(
            patientCode: Input.PatientCode,
            date: Input.Date.Date,
            time: Input.Time,
            patient: Input.Patient.Trim(),
            contactInfo: Input.ContactInfo.Trim(),
            doctor: Input.Doctor.Trim(),
            notes: string.IsNullOrWhiteSpace(Input.Notes)
                ? null
                : Input.Notes.Trim(),
            status: "Pending"
        );

        await _dbContext
            .Set<ClinicAppointment>()
            .AddAsync(appointment);

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"{appointment.Patient} was successfully added.";

        return RedirectToPage();
    }

    // =========================================================
    // CONFIRM APPOINTMENT
    // PENDING -> CONFIRMED
    // =========================================================

    public async Task<IActionResult> OnPostConfirmAsync(
        Guid id,
        int pageIndex = 1,
        string keyword = "")
    {
        var appointment = await _dbContext
            .Set<ClinicAppointment>()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            TempData["ErrorMessage"] =
                "Appointment not found.";

            return RedirectToPage(
                new
                {
                    pageIndex,
                    keyword
                }
            );
        }

        if (IsPending(appointment.Status))
        {
            appointment.Status = "Confirmed";

            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Appointment for {appointment.Patient} has been confirmed.";
        }
        else
        {
            TempData["ErrorMessage"] =
                "Only pending appointments can be confirmed.";
        }

        return RedirectToPage(
            new
            {
                pageIndex,
                keyword
            }
        );
    }

    // =========================================================
    // COMPLETE APPOINTMENT
    // CONFIRMED -> COMPLETED
    // =========================================================

    public async Task<IActionResult> OnPostCompleteAsync(
        Guid id,
        int pageIndex = 1,
        string keyword = "")
    {
        var appointment = await _dbContext
            .Set<ClinicAppointment>()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            TempData["ErrorMessage"] =
                "Appointment not found.";

            return RedirectToPage(
                new
                {
                    pageIndex,
                    keyword
                }
            );
        }

        if (string.Equals(
            appointment.Status,
            "Confirmed",
            StringComparison.OrdinalIgnoreCase))
        {
            appointment.Complete();

            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Appointment for {appointment.Patient} has been completed.";
        }
        else
        {
            TempData["ErrorMessage"] =
                "Only confirmed appointments can be completed.";
        }

        return RedirectToPage(
            new
            {
                pageIndex,
                keyword
            }
        );
    }

    // =========================================================
    // CANCEL APPOINTMENT
    // PENDING OR CONFIRMED -> CANCELLED
    // =========================================================

    public async Task<IActionResult> OnPostCancelAsync(
        Guid id,
        int pageIndex = 1,
        string keyword = "")
    {
        var appointment = await _dbContext
            .Set<ClinicAppointment>()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            TempData["ErrorMessage"] =
                "Appointment not found.";

            return RedirectToPage(
                new
                {
                    pageIndex,
                    keyword
                }
            );
        }

        if (IsPending(appointment.Status) ||
            string.Equals(
                appointment.Status,
                "Confirmed",
                StringComparison.OrdinalIgnoreCase))
        {
            // IMPORTANT:
            // Cancel only changes the status.
            // It does NOT delete the appointment.

            appointment.Cancel();

            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Appointment for {appointment.Patient} has been cancelled.";
        }
        else
        {
            TempData["ErrorMessage"] =
                "This appointment can no longer be cancelled.";
        }

        return RedirectToPage(
            new
            {
                pageIndex,
                keyword
            }
        );
    }

    // =========================================================
    // DELETE APPOINTMENT
    // ONLY THIS ACTION REMOVES THE RECORD
    // =========================================================

    public async Task<IActionResult> OnPostDeleteAsync(
        Guid id,
        int pageIndex = 1,
        string keyword = "")
    {
        var appointment = await _dbContext
            .Set<ClinicAppointment>()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            TempData["ErrorMessage"] =
                "Appointment not found.";

            return RedirectToPage(
                new
                {
                    pageIndex,
                    keyword
                }
            );
        }

        string patientName = appointment.Patient ?? "Appointment";

        _dbContext
            .Set<ClinicAppointment>()
            .Remove(appointment);

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"{patientName} appointment has been deleted.";

        return RedirectToPage(
            new
            {
                pageIndex,
                keyword
            }
        );
    }

    // =========================================================
    // SEARCH QUERY
    // =========================================================

    private IQueryable<ClinicAppointment> BuildSearchQuery()
    {
        var query = _dbContext
            .Set<ClinicAppointment>()
            .AsNoTracking();

        if (string.IsNullOrWhiteSpace(Keyword))
        {
            return query;
        }

        string search = Keyword.Trim().ToLower();

      // Search by patient code separately.
        // This avoids using PatientCode.ToString()
        // inside the database query.  

        if (int.TryParse(search, out    int patientCode))
        {
            query = query.Where(a =>
                a.PatientCode == patientCode ||

                (a.Patient != null &&
                 a.Patient.ToLower().Contains(search)) ||

                (a.Doctor != null &&
                 a.Doctor.ToLower().Contains(search)) ||

                (a.ContactInfo != null &&
                 a.ContactInfo.ToLower().Contains(search)) ||

                (a.Status != null &&
                 a.Status.ToLower().Contains(search)) ||

                (a.Notes != null &&
                 a.Notes.ToLower().Contains(search))
            );
        }
        else
        {
            query = query.Where(a =>

                (a.Patient != null &&
                 a.Patient.ToLower().Contains(search)) ||

                (a.Doctor != null &&
                 a.Doctor.ToLower().Contains(search)) ||

                (a.ContactInfo != null &&
                 a.ContactInfo.ToLower().Contains(search)) ||

                (a.Status != null &&
                 a.Status.ToLower().Contains(search)) ||

                (a.Notes != null &&
                 a.Notes.ToLower().Contains(search))
            );
        }

        return query;
    }

    // =========================================================
    // LOAD CURRENT PAGE
    // =========================================================

    private async Task LoadAppointmentsAsync()
    {
        Appointments = await BuildSearchQuery()

            // Sort by date first
            .OrderBy(a => a.Date)

            // Then sort by time
            .ThenBy(a => a.Time)

            // Pagination
            .Skip((PageIndex - 1) * PageSize)

            .Take(PageSize)

            .ToListAsync();
    }

    // =========================================================
    // CHECK PENDING STATUS
    // =========================================================

    private static bool IsPending(string? status)
    {
        return string.IsNullOrWhiteSpace(status) ||
               string.Equals(
                   status,
                   "Pending",
                   StringComparison.OrdinalIgnoreCase);
    }
}