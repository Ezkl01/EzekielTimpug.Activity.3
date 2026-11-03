public class ClinicAppointment
{
    public Guid Id { get; set; }

    public int PatientCode { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public string? Patient { get; set; }

    public string? Doctor { get; set; }

    public string? ContactInfo { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = "Pending";


    // =========================================================
    // EF CORE CONSTRUCTOR
    // =========================================================

    public ClinicAppointment()
    {
    }


    // =========================================================
    // CREATE APPOINTMENT CONSTRUCTOR
    // =========================================================

    public ClinicAppointment(
        int patientCode,
        DateTime date,
        TimeSpan time,
        string? patient,
        string? doctor,
        string? notes,
        string? status,
        string? contactInfo = null)
    {
        Id = Guid.NewGuid();

        PatientCode = patientCode;

        Date = date;

        Time = time;

        Patient = patient;

        Doctor = doctor;

        Notes = notes;

        Status = string.IsNullOrWhiteSpace(status)
            ? "Pending"
            : status.Trim();

        ContactInfo = contactInfo;
    }


    // =========================================================
    // UPDATE NOTES
    // =========================================================

    public void UpdateNotes(string newNotes)
    {
        Notes = newNotes;
    }


    // =========================================================
    // CANCEL APPOINTMENT
    // =========================================================

    public void Cancel()
    {
        Status = "Cancelled";
    }


    // =========================================================
    // COMPLETE APPOINTMENT
    // =========================================================

    public void Complete()
    {
        Status = "Completed";
    }


    // =========================================================
    // UPDATE APPOINTMENT
    // =========================================================

    public void UpdateAppointment(
        int patientCode,
        DateTime date,
        TimeSpan time,
        string patient,
        string doctor,
        string? notes,
        string status,
        string? contactInfo)
    {
        PatientCode = patientCode;

        Date = date;

        Time = time;

        Patient = patient?.Trim();

        Doctor = doctor?.Trim();

        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();

        Status = string.IsNullOrWhiteSpace(status)
            ? "Pending"
            : status.Trim();

        ContactInfo = string.IsNullOrWhiteSpace(contactInfo)
            ? null
            : contactInfo.Trim();
    }


    // =========================================================
    // CHECK AVAILABILITY
    // =========================================================

    public bool IsAvailable(
        DateTime desiredDate,
        TimeSpan desiredTime)
    {
        return true;
    }


    // =========================================================
    // RESCHEDULE
    // =========================================================

    public void Reschedule(
        DateTime newDate,
        TimeSpan newTime)
    {
        Date = newDate;

        Time = newTime;
    }


    // =========================================================
    // EMAIL APPOINTMENT DETAILS
    // =========================================================

    public void EmailAppointmentDetails(string email)
    {
        Console.WriteLine(
            $"Emailing appointment details to {email}..."
        );
    }
}