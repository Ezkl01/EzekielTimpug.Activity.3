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
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]
public class DoctorsModel : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;

    public DoctorsModel(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // =========================================================
    // DOCTORS
    // =========================================================

    public List<Doctor> Doctors { get; set; } = new();

    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; }

    public int TotalRecords { get; set; }

    public string Keyword { get; set; } = "";


    // =========================================================
    // ADD DOCTOR FORM
    // =========================================================

    [BindProperty]
    public AddDoctorInput Input { get; set; } = new();


    public class AddDoctorInput
    {
        [Required(ErrorMessage = "Please enter the doctor's full name.")]
        [StringLength(150)]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Please enter the doctor's specialty.")]
        [StringLength(150)]
        public string Specialty { get; set; } = "";
    }


    // =========================================================
    // LOAD DOCTORS
    // =========================================================

    public async Task OnGetAsync(
        int pageIndex = 1,
        string keyword = "")
    {
        Keyword = keyword?.Trim() ?? "";

        if (pageIndex < 1)
        {
            pageIndex = 1;
        }


        // =====================================================
        // COUNT DOCTORS
        // Only count doctors that are NOT deleted
        // =====================================================

        var countQuery = _dbContext.Doctors
            .AsNoTracking()
            .Where(d => !d.IsDeleted);


        // =====================================================
        // SEARCH
        // =====================================================

        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            var search = Keyword.ToLower();

            countQuery = countQuery.Where(d =>
                (d.FullName != null &&
                 d.FullName.ToLower().Contains(search))
                ||
                (d.Specialty != null &&
                 d.Specialty.ToLower().Contains(search))
            );
        }


        // =====================================================
        // TOTAL RECORDS
        // =====================================================

        TotalRecords = await countQuery.CountAsync();


        // =====================================================
        // TOTAL PAGES
        // =====================================================

        TotalPages = (int)Math.Ceiling(
            TotalRecords / (double)PageSize
        );


        // =====================================================
        // CHECK PAGE NUMBER
        // =====================================================

        if (TotalPages > 0 && pageIndex > TotalPages)
        {
            pageIndex = TotalPages;
        }


        PageIndex = pageIndex;


        // =====================================================
        // LOAD DOCTORS
        // =====================================================

        await LoadDoctorsAsync();
    }


    // =========================================================
    // ADD DOCTOR
    // =========================================================

    public async Task<IActionResult> OnPostAddDoctorAsync(
        int pageIndex = 1,
        string keyword = "")
    {
        // =====================================================
        // VALIDATE FORM
        // =====================================================

        if (!ModelState.IsValid)
        {
            Keyword = keyword?.Trim() ?? "";

            PageIndex = pageIndex < 1 ? 1 : pageIndex;

            await LoadDoctorsAsync();

            return Page();
        }


        // =====================================================
        // CREATE NEW DOCTOR
        // =====================================================

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),

            FullName = Input.FullName.Trim(),

            Specialty = Input.Specialty.Trim(),

            // New doctors are active
            IsDeleted = false
        };


        // =====================================================
        // SAVE TO DATABASE
        // =====================================================

        _dbContext.Doctors.Add(doctor);

        await _dbContext.SaveChangesAsync();


        // =====================================================
        // SUCCESS MESSAGE
        // =====================================================

        TempData["SuccessMessage"] =
            $"{doctor.FullName} was successfully added.";


        // =====================================================
        // RETURN TO DOCTORS PAGE
        // =====================================================

        return RedirectToPage(
            new
            {
                pageIndex = pageIndex,
                keyword = keyword
            }
        );
    }


    // =========================================================
    // SOFT DELETE DOCTOR
    // =========================================================

    public async Task<IActionResult> OnPostDeleteDoctorAsync(
        Guid id,
        int pageIndex = 1,
        string keyword = "")
    {
        // =====================================================
        // FIND DOCTOR
        // =====================================================

        var doctor = await _dbContext.Doctors
            .FirstOrDefaultAsync(d => d.Id == id);


        // =====================================================
        // DOCTOR NOT FOUND
        // =====================================================

        if (doctor == null)
        {
            TempData["ErrorMessage"] =
                "The doctor could not be found.";

            return RedirectToPage(
                new
                {
                    pageIndex = pageIndex,
                    keyword = keyword
                }
            );
        }


        // =====================================================
        // SOFT DELETE
        // =====================================================

        doctor.IsDeleted = true;

        await _dbContext.SaveChangesAsync();


        // =====================================================
        // SUCCESS MESSAGE
        // =====================================================

        TempData["SuccessMessage"] =
            $"{doctor.FullName} was successfully deleted.";


        // =====================================================
        // RETURN TO DOCTORS PAGE
        // =====================================================

        return RedirectToPage(
            new
            {
                pageIndex = pageIndex,
                keyword = keyword
            }
        );
    }


    // =========================================================
    // LOAD DOCTORS FROM DATABASE
    // =========================================================

    private async Task LoadDoctorsAsync()
    {
        // =====================================================
        // ONLY LOAD ACTIVE DOCTORS
        // =====================================================

        var doctorsQuery = _dbContext.Doctors
            .AsNoTracking()
            .Where(d => !d.IsDeleted);


        // =====================================================
        // SEARCH
        // =====================================================

        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            var search = Keyword.ToLower();

            doctorsQuery = doctorsQuery.Where(d =>
                (d.FullName != null &&
                 d.FullName.ToLower().Contains(search))
                ||
                (d.Specialty != null &&
                 d.Specialty.ToLower().Contains(search))
            );
        }


        // =====================================================
        // PAGINATION
        // =====================================================

        var skip = (PageIndex - 1) * PageSize;


        // =====================================================
        // GET DOCTORS
        // =====================================================

        Doctors = await doctorsQuery
            .OrderBy(d => d.FullName)
            .Skip(skip)
            .Take(PageSize)
            .ToListAsync();
    }
}