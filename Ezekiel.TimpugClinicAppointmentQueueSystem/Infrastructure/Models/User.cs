namespace Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Models;

public class Student
{
public int Id { get; set; }
public string FullName { get; set; } = "";
public int Age { get; set; }
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
