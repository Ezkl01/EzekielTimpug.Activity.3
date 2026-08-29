using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(Roles = "User")]
public class UserDashboardModel : PageModel
{
    public void OnGet()
    {
    }
}