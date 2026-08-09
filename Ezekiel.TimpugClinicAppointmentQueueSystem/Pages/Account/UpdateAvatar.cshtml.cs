using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using System.Text;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

public class UpdateAvatar : PageModel
{
    private readonly ClinicAppointmentDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public UpdateAvatar(
        ClinicAppointmentDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [BindProperty]
    public UserAvatarDto UserAvatarDto { get; set; } = new UserAvatarDto();

    private void LoadUserData()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return;
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);

        if (user == null)
        {
            return;
        }

        UserAvatarDto.Username = user.UserName;
        UserAvatarDto.FirstName = user.FirstName;
        UserAvatarDto.LastName = user.LastName;

        var avatarDiskPath = Path.Combine(_environment.WebRootPath,"users",$"{user.Id}.png");

        if (System.IO.File.Exists(avatarDiskPath))
        {
            UserAvatarDto.ProfileImage = $"/users/{user.Id}.png?v={DateTime.UtcNow.Ticks}";
        }
        else
        {
            UserAvatarDto.ProfileImage = "/users/default.png?v={DateTime.UtcNow.Ticks}";
        }
    }
    
private string ComputeHash(Stream stream)
{
    stream.Position = 0;

    using var sha = SHA256.Create();
    var hash = sha.ComputeHash(stream);

    return BitConverter.ToString(hash).Replace("-", "");
}
    public IActionResult OnGet()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        LoadUserData();

        return Page();
    }

    public IActionResult OnPost()
    {
        LoadUserData();
        
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);

        if (user == null)
        {
            return NotFound();
        }

        if (UserAvatarDto.AvatarImage != null &&
            UserAvatarDto.AvatarImage.Length > 0)
        {
            Image image;

            try
            {
                using (var uploadStream = UserAvatarDto.AvatarImage.OpenReadStream())
                {
                    image = Image.Load(uploadStream);
                }
            }
            catch (UnknownImageFormatException)
            {
                ModelState.AddModelError(
                    "UserAvatarDto.AvatarImage",
                    "Only image files are allowed.");

                LoadUserData();

                return Page();
            }
            catch (InvalidImageContentException)
            {
                ModelState.AddModelError(
                    "UserAvatarDto.AvatarImage",
                    "Only image files are allowed.");

                LoadUserData();

                return Page();
            }

            using (image)
            {
                var uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "users");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

               var filePath = Path.Combine(
                uploadsFolder,
                $"{user.Id}.png");

if (System.IO.File.Exists(filePath))
{
    // Save uploaded image into memory
    using var uploadedMemory = new MemoryStream();
    image.SaveAsPng(uploadedMemory);

    uploadedMemory.Position = 0;
    var uploadedHash = ComputeHash(uploadedMemory);

    string existingHash;
    using (var existingStream = System.IO.File.OpenRead(filePath))
    {
        existingHash = ComputeHash(existingStream);
    }

    // Compare hashes
    if (uploadedHash == existingHash)
    {
        ModelState.AddModelError(
            "UserAvatarDto.AvatarImage",
            "You are already using this profile picture.");

        LoadUserData();
        return Page();
    }

    // Reset memory stream
    uploadedMemory.Position = 0;

    // Delete old avatar
    System.IO.File.Delete(filePath);

    // Save new avatar
    using (var fileStream = new FileStream(filePath, FileMode.Create))
    {
        uploadedMemory.CopyTo(fileStream);
    }
}
else
{
    using (var fileStream = new FileStream(filePath, FileMode.Create))
    {
        image.SaveAsPng(fileStream);
    }
}
}
    }

        _dbContext.SaveChanges();

        return RedirectToPage("/Account/Profile");
    }
}

public class UserAvatarDto
{
    public string? Username { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? ProfileImage { get; set; }

    [Required(ErrorMessage = "Avatar Image is required.")]
    public IFormFile? AvatarImage { get; set; }
}