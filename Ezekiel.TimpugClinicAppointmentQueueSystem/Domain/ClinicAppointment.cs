public class ClinicAppointment
{
    public Guid Id {get; set;}
    public int PatientCode {get; set;}
    public DateTime Date {get; set;}
    public TimeSpan Time {get; set;}
    public string? Patient {get; set;}
    public string? Doctor {get; set;}

    public string? Notes {get; set;}
    public void UpdateNotes(string newNotes)
    {
        Notes = newNotes;
    }

    public string? Status {get; set;}
    public void Cancel()
    {
        Status = "Cancelled";
    }
    public void Complete()
    {
        Status = "Completed";
    }



    public ClinicAppointment(int patientCode, DateTime date, TimeSpan time, string? patient, string? doctor, string? notes, string? status) // constructor
    {
        Id = Guid.NewGuid();
        PatientCode = patientCode;
        Date = date;
        Time = time;
        Patient = patient;
        Doctor = doctor;
        Notes = notes;
        Status = status;
    }

     public bool IsAvailable(DateTime desiredDate, TimeSpan desiredTime)
    {
        return true;
    }
    public void Reschedule(DateTime newDate, TimeSpan newTime)
    {
        Date = newDate;
        Time = newTime;
    }
    public void EmailAppointmentDetails(string email)
    {
        Console.WriteLine($"Emailing appointment details to {email}...");
    }

}