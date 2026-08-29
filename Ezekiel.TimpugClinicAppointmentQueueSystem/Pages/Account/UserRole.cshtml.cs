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

        public async Task OnGetAsync(int pageIndex = 1,string keyword = "")   
        {

        Keyword = keyword?.Trim() ?? "";
        if (pageIndex < 1)
        {
            pageIndex = 1;
        }

        // Count users based on search
        var countQuery = _dbContext.Users
            .AsNoTracking();

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


        // Calculate total pages
        TotalPages = (int)Math.Ceiling(
            TotalRecords / (double)PageSize
        );


        // Make sure page number does not exceed total pages
        if (TotalPages > 0 && pageIndex > TotalPages)
        {
            pageIndex = TotalPages;
        }


        PageIndex = pageIndex;


        // Load users
        await LoadUsersAsync();
    }

    // CHANGE THE USER'S ROLE
    public async Task<IActionResult> OnPostChangeRoleAsync(
        Guid userId,
        string role)
    {
        if (role != "Admin" && role != "User")
        {
            TempData["ErrorMessage"] = "Invalid role selected.";
            return RedirectToPage();
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToPage();
        }

        var roleRecord = await _dbContext.UserLoginInfos
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.Key != null &&
                r.Key.ToLower() == "role");

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
            roleRecord.Value = role;
        }

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Role for {user.UserName} has been changed to {role}.";

        return RedirectToPage();
    }

    private async Task LoadUsersAsync()
    {
        var usersQuery = _dbContext.Users
            .AsNoTracking();

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

        var skip = (PageIndex - 1) * PageSize;


        var users = await usersQuery
            .Skip(skip)
            .Take(PageSize)
            .ToListAsync();


        var roleRecords = await _dbContext.UserLoginInfos
            .AsNoTracking()
            .Where(r =>
                r.Key != null &&
                r.Key.ToLower() == "role")
            .ToListAsync();


        Users = users.Select(user =>
        {
            var roleRecord = roleRecords
                .FirstOrDefault(r => r.UserId == user.Id);

            var roleValue = roleRecord?.Value ?? "User";

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


public class UserRoleViewModel
{
    public Guid? Id { get; set; }

    public string? UserName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string Role { get; set; } = "User";
}