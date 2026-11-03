using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

[Authorize(Roles = "Admin")]
public class UserRoleModel : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;

    public UserRoleModel(ClinicAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<UserRoleViewModel> Users { get; set; } = new();

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public string Keyword { get; set; } = "";

    // =========================================================
    // GET USERS
    // =========================================================
    public async Task OnGetAsync(
        int pageIndex = 1,
        string keyword = "")
    {
        Keyword = keyword?.Trim() ?? "";

        if (pageIndex < 1)
            pageIndex = 1;

        // =====================================================
        // COUNT ONLY ACTIVE / NON-DELETED USERS
        // =====================================================
        var countQuery = _dbContext.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        // =====================================================
        // SEARCH
        // =====================================================
        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            var search = Keyword.ToLower();

            countQuery = countQuery.Where(u =>
                (u.FirstName != null &&
                 u.FirstName.ToLower().Contains(search))
                ||
                (u.LastName != null &&
                 u.LastName.ToLower().Contains(search))
            );
        }

        TotalRecords = await countQuery.CountAsync();

        // =====================================================
        // PAGINATION
        // =====================================================
        TotalPages = (int)Math.Ceiling(
            TotalRecords / (double)PageSize
        );

        if (TotalPages > 0 && pageIndex > TotalPages)
            pageIndex = TotalPages;

        PageIndex = pageIndex;

        await LoadUsersAsync();
    }

    // =========================================================
    // CHANGE USER ROLE
    // =========================================================
    public async Task<IActionResult> OnPostChangeRoleAsync(
        Guid userId,
        string role)
    {
        // Only allow Admin or User
        if (role != "Admin" && role != "User")
        {
            TempData["ErrorMessage"] =
                "Invalid role selected.";

            return RedirectToPage();
        }

        // Find user
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.Id == userId &&
                !u.IsDeleted);

        if (user == null)
        {
            TempData["ErrorMessage"] =
                "User not found.";

            return RedirectToPage();
        }

        // Find existing role record
        var roleRecord = await _dbContext.UserLoginInfos
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.Key != null &&
                r.Key.ToLower() == "role");

        // Create role if it doesn't exist
        if (roleRecord == null)
        {
            roleRecord = new UserLoginInfo(
                userId,
                "role",
                role
            );

            _dbContext.UserLoginInfos.Add(roleRecord);
        }
        else
        {
            // Update existing role
            roleRecord.Value = role;
        }

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Role for {user.UserName} has been changed to {role}.";

        return RedirectToPage();
    }

    // =========================================================
    // SOFT DELETE USER
    // =========================================================
    public async Task<IActionResult> OnPostDeleteAsync(
        Guid userId)
    {
        // =====================================================
        // GET CURRENT LOGGED-IN USER
        // =====================================================
        var currentUserIdString = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        // =====================================================
        // PREVENT ADMIN FROM DELETING THEIR OWN ACCOUNT
        // =====================================================
        if (Guid.TryParse(
            currentUserIdString,
            out Guid currentUserId))
        {
            if (currentUserId == userId)
            {
                TempData["ErrorMessage"] =
                    "You cannot delete your own account.";

                return RedirectToPage();
            }
        }

        // =====================================================
        // FIND USER
        // ONLY FIND NON-DELETED USER
        // =====================================================
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.Id == userId &&
                !u.IsDeleted);

        if (user == null)
        {
            TempData["ErrorMessage"] =
                "User not found.";

            return RedirectToPage();
        }

        // =====================================================
        // REMOVE LOGIN INFORMATION
        // =====================================================
        var loginInfo = await _dbContext.UserLoginInfos
            .Where(r => r.UserId == userId)
            .ToListAsync();

        if (loginInfo.Any())
        {
            _dbContext.UserLoginInfos.RemoveRange(loginInfo);
        }

        // =====================================================
        // SOFT DELETE
        // DO NOT REMOVE THE USER FROM THE USERS TABLE
        // =====================================================
        user.IsDeleted = true;

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"User {user.UserName} has been deleted successfully.";

        return RedirectToPage();
    }

    // =========================================================
    // LOAD USERS
    // =========================================================
    private async Task LoadUsersAsync()
    {
        // =====================================================
        // ONLY LOAD NON-DELETED USERS
        // =====================================================
        var usersQuery = _dbContext.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        // =====================================================
        // SEARCH
        // =====================================================
        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            var search = Keyword.ToLower();

            usersQuery = usersQuery.Where(u =>
                (u.FirstName != null &&
                 u.FirstName.ToLower().Contains(search))
                ||
                (u.LastName != null &&
                 u.LastName.ToLower().Contains(search))
            );
        }

        // =====================================================
        // PAGINATION
        // =====================================================
        var skip = (PageIndex - 1) * PageSize;

        var users = await usersQuery
            .Skip(skip)
            .Take(PageSize)
            .ToListAsync();

        // =====================================================
        // GET ROLE RECORDS
        // =====================================================
        var roleRecords = await _dbContext.UserLoginInfos
            .AsNoTracking()
            .Where(r =>
                r.Key != null &&
                r.Key.ToLower() == "role")
            .ToListAsync();

        // =====================================================
        // CONVERT TO VIEW MODEL
        // =====================================================
        Users = users.Select(user =>
        {
            var roleRecord = roleRecords
                .FirstOrDefault(r =>
                    r.UserId == user.Id);

            var roleValue =
                roleRecord?.Value ?? "User";

            // Normalize role
            if (roleValue.Equals(
                "admin",
                StringComparison.OrdinalIgnoreCase))
            {
                roleValue = "Admin";
            }
            else
            {
                roleValue = "User";
            }

            return new UserRoleViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roleValue
            };

        }).ToList();
    }
}


// =============================================================
// USER ROLE VIEW MODEL
// =============================================================
public class UserRoleViewModel
{
    public Guid? Id { get; set; }

    public string? UserName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string Role { get; set; } = "User";
}