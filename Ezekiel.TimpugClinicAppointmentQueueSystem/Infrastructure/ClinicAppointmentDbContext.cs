using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

public class ClinicAppointmentDbContext : DbContext
{
    public ClinicAppointmentDbContext(DbContextOptions<ClinicAppointmentDbContext> options)
        : base(options)
    {
    }

    public DbSet<ClinicAppointment> Appointments { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
   

    public DbSet<User> Users { get; set; } // Add this line to include the Users DbSet
    public DbSet<UserLoginInfo> UserLoginInfos { get; set; } // Add this line to include the UserLoginInfos DbSet
}
