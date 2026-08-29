using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(Roles = "Admin")]
public class AdminDashboardModel : PageModel
{
    public void OnGet()
    {
    }
}