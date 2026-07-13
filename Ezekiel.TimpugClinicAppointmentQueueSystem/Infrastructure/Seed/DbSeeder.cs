using BCrypt.Net;
using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(ClinicAppointmentDbContext context)
    {
        if (context.Users.Any(u => u.UserName == "admin123@gmail.com"))
            return;

        var admin = new User("admin123@gmail.com", new DateTime(2000, 1, 1))
        {
            FirstName = "System",
            LastName = "Administrator"
        };

        context.Users.Add(admin);
        context.SaveChanges();

        context.UserLoginInfos.AddRange(
            new UserLoginInfo(
                admin.Id,
                "Password",
                BCrypt.Net.BCrypt.HashPassword("admin123")
            ),

            new UserLoginInfo(
                admin.Id,
                "LoginStatus",
                "Active"
            ),

            new UserLoginInfo(
                admin.Id,
                "LoginRetries",
                "0"
            )
        );

        context.SaveChanges();
    }
}