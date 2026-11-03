using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

[Authorize(Roles = "User")]
public class UserAppointmentsModel : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;

    public UserAppointmentsModel(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // =====================================================
    // APPOINTMENTS
    // =====================================================

    public List<ClinicAppointment> Appointments { get; set; } = new();

    public string PatientName { get; set; } = "";

    public int TotalRecords { get; set; }

    // =====================================================
    // PAGINATION
    // =====================================================

    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 6;

    public int TotalPages { get; set; }

    // =====================================================
    // SEARCH
    // =====================================================

    public string Keyword { get; set; } = "";

    // =====================================================
    // BOOKING INPUT
    // =====================================================

    [BindProperty]
    public BookingInput Input { get; set; } = new();

    public class BookingInput
    {
        [Required(ErrorMessage = "Please enter your patient code.")]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a valid patient code.")]
        [Display(Name = "Patient Code")]
        public int PatientCode { get; set; }

        [Required(ErrorMessage = "Please enter your contact number.")]
        [StringLength(50)]
        [Display(Name = "Contact Number")]
        public string ContactInfo { get; set; } = "";

        [Required(ErrorMessage = "Please enter or select a doctor.")]
        [StringLength(150)]
        [Display(Name = "Doctor")]
        public string Doctor { get; set; } = "";

        [Required(ErrorMessage = "Please select an appointment date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please select an appointment time.")]
        [DataType(DataType.Time)]
        [Display(Name = "Appointment Time")]
        public TimeSpan Time { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string Notes { get; set; } = "";
    }

    // =====================================================
    // GET CURRENT USER ID
    // =====================================================

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (Guid.TryParse(userIdValue, out Guid userId))
        {
            return userId;
        }

        return null;
    }

    // =====================================================
    // GET CURRENT USER
    // =====================================================

    private async Task<global::User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return null;
        }

        return await _dbContext.Set<global::User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == userId &&
                !u.IsDeleted
            );
    }

    // =====================================================
    // GET PAGE
    // =====================================================

    public async Task<IActionResult> OnGetAsync(
        int pageIndex = 1,
        string keyword = "")
    {
        var currentUser = await GetCurrentUserAsync();

        if (currentUser == null)
        {
            return Forbid();
        }

        PatientName = GetPatientName(currentUser);

        Keyword = keyword?.Trim() ?? "";

        PageIndex = Math.Max(1, pageIndex);

        var query = BuildAppointmentQuery();

        TotalRecords = await query.CountAsync();

        TotalPages = (int)Math.Ceiling(
            TotalRecords / (double)PageSize
        );

        if (TotalPages > 0 && PageIndex > TotalPages)
        {
            PageIndex = TotalPages;
        }

        if (TotalPages == 0)
        {
            PageIndex = 1;
        }

        await LoadAppointmentsAsync();

        return Page();
    }

    // =====================================================
    // BOOK APPOINTMENT
    // =====================================================

    public async Task<IActionResult> OnPostBookAsync()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Forbid();
        }

        var currentUser = await _dbContext.Set<global::User>()
            .FirstOrDefaultAsync(u =>
                u.Id == userId &&
                !u.IsDeleted
            );

        if (currentUser == null)
        {
            return Forbid();
        }

        PatientName = GetPatientName(currentUser);

        // Validate date
        if (Input.Date.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                "Input.Date",
                "Appointment date cannot be in the past."
            );
        }

        // Validate time
        if (Input.Time < TimeSpan.Zero ||
            Input.Time >= TimeSpan.FromDays(1))
        {
            ModelState.AddModelError(
                "Input.Time",
                "Please select a valid appointment time."
            );
        }

        // Check that the doctor field is not empty
        if (string.IsNullOrWhiteSpace(Input.Doctor))
        {
            ModelState.AddModelError(
                "Input.Doctor",
                "Please enter the doctor's name."
            );
        }

        if (!ModelState.IsValid)
        {
            Keyword = "";
            PageIndex = 1;

            await LoadPageDataAsync();

            return Page();
        }

        // Get patient name from the logged-in account
        string patientName = GetPatientName(currentUser);

        // Create appointment
        var appointment = new ClinicAppointment(
            patientCode: Input.PatientCode,
            date: Input.Date.Date,
            time: Input.Time,
            patient: patientName,
            contactInfo: Input.ContactInfo.Trim(),
            doctor: Input.Doctor.Trim(),
            notes: string.IsNullOrWhiteSpace(Input.Notes)
                ? null
                : Input.Notes.Trim(),
            status: "Pending"
        );

        await _dbContext.Set<ClinicAppointment>()
            .AddAsync(appointment);

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Your appointment request has been submitted successfully.";

        return RedirectToPage();
    }

    // =====================================================
    // CANCEL OWN APPOINTMENT
    // =====================================================

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Forbid();
        }

        var currentUser = await GetCurrentUserAsync();

        if (currentUser == null)
        {
            return Forbid();
        }

        string patientName = GetPatientName(currentUser);

        // ClinicAppointment has no user ID field, so scope by its patient name.
        var appointment = await _dbContext
            .Set<ClinicAppointment>()
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.Patient == patientName
            );

        if (appointment == null)
        {
            TempData["ErrorMessage"] =
                "Appointment not found.";

            return RedirectToPage();
        }

        bool isPending =
            string.IsNullOrWhiteSpace(appointment.Status) ||
            string.Equals(
                appointment.Status,
                "Pending",
                StringComparison.OrdinalIgnoreCase
            );

        bool isConfirmed = string.Equals(
            appointment.Status,
            "Confirmed",
            StringComparison.OrdinalIgnoreCase
        );

        if (isPending || isConfirmed)
        {
            appointment.Cancel();

            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your appointment has been cancelled.";
        }
        else
        {
            TempData["ErrorMessage"] =
                "This appointment can no longer be cancelled.";
        }

        return RedirectToPage();
    }

    // =====================================================
    // GET PATIENT NAME
    // =====================================================

    private static string GetPatientName(global::User user)
    {
        string name = string.Join(
            " ",
            new[]
            {
                user.FirstName,
                user.LastName
            }.Where(value =>
                !string.IsNullOrWhiteSpace(value))
        ).Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return user.UserName ?? "User";
        }

        return name;
    }

    // =====================================================
    // APPOINTMENT QUERY
    // =====================================================

    private IQueryable<ClinicAppointment> BuildAppointmentQuery()
    {
        var query = _dbContext
            .Set<ClinicAppointment>()
            .AsNoTracking()
            .Where(a => a.Patient == PatientName);

        if (string.IsNullOrWhiteSpace(Keyword))
        {
            return query;
        }

        string search = Keyword.ToLower();

        if (int.TryParse(search, out int patientCode))
        {
            query = query.Where(a =>
                a.PatientCode == patientCode ||
                (a.Doctor != null &&
                 a.Doctor.ToLower().Contains(search)) ||
                (a.Status != null &&
                 a.Status.ToLower().Contains(search))
            );
        }
        else
        {
            query = query.Where(a =>
                (a.Doctor != null &&
                 a.Doctor.ToLower().Contains(search)) ||
                (a.Status != null &&
                 a.Status.ToLower().Contains(search)) ||
                (a.Notes != null &&
                 a.Notes.ToLower().Contains(search))
            );
        }

        return query;
    }

    // =====================================================
    // LOAD APPOINTMENTS
    // =====================================================

    private async Task LoadAppointmentsAsync()
    {
        Appointments = await BuildAppointmentQuery()
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.Time)
            .Skip((PageIndex - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    // =====================================================
    // LOAD PAGE DATA
    // =====================================================

    private async Task LoadPageDataAsync()
    {
        var query = BuildAppointmentQuery();

        TotalRecords = await query.CountAsync();

        TotalPages = (int)Math.Ceiling(
            TotalRecords / (double)PageSize
        );

        await LoadAppointmentsAsync();
    }
}