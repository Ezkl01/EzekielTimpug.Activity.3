using Ezekiel.TimpugClinicAppointmentQueueSystem.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(ClinicAppointmentDbContext context)
    {
        // =========================================
        // SEED APPOINTMENTS ONLY
        // =========================================

        var clinicAppointments = context.Set<ClinicAppointment>();

        if (!clinicAppointments.Any())
        {
            var appointments = new List<ClinicAppointment>
            {
                new ClinicAppointment(
                    patientCode: 1001,
                    date: DateTime.Today.AddDays(1),
                    time: new TimeSpan(9, 0, 0),
                    patient: "Juan Dela Cruz",
                    contactInfo: "09171234567",
                    doctor: "Dr. Maria Santos",
                    notes: "General check-up",
                    status: "Pending"
                ),

                new ClinicAppointment(
                    patientCode: 1002,
                    date: DateTime.Today.AddDays(1),
                    time: new TimeSpan(10, 0, 0),
                    patient: "Maria Garcia",
                    contactInfo: "09171234568",
                    doctor: "Dr. Juan Dela Cruz",
                    notes: "Child consultation",
                    status: "Confirmed"
                ),

                new ClinicAppointment(
                    patientCode: 1003,
                    date: DateTime.Today.AddDays(2),
                    time: new TimeSpan(8, 30, 0),
                    patient: "Carlos Reyes",
                    contactInfo: "09171234569",
                    doctor: "Dr. Angela Reyes",
                    notes: "Heart consultation",
                    status: "Pending"
                ),

                new ClinicAppointment(
                    patientCode: 1004,
                    date: DateTime.Today.AddDays(2),
                    time: new TimeSpan(11, 0, 0),
                    patient: "Ana Santos",
                    contactInfo: "09171234570",
                    doctor: "Dr. Michael Garcia",
                    notes: "Skin consultation",
                    status: "Confirmed"
                ),

                new ClinicAppointment(
                    patientCode: 1005,
                    date: DateTime.Today.AddDays(3),
                    time: new TimeSpan(9, 30, 0),
                    patient: "Mark Cruz",
                    contactInfo: "09171234571",
                    doctor: "Dr. Sofia Cruz",
                    notes: "Routine consultation",
                    status: "Pending"
                ),

                new ClinicAppointment(
                    patientCode: 1006,
                    date: DateTime.Today.AddDays(3),
                    time: new TimeSpan(13, 0, 0),
                    patient: "Jennifer Cruz",
                    contactInfo: "09171234572",
                    doctor: "Dr. Maria Santos",
                    notes: "Follow-up check-up",
                    status: "Completed"
                ),

                new ClinicAppointment(
                    patientCode: 1007,
                    date: DateTime.Today.AddDays(4),
                    time: new TimeSpan(14, 30, 0),
                    patient: "Robert Garcia",
                    contactInfo: "09171234573",
                    doctor: "Dr. Juan Dela Cruz",
                    notes: "Pediatric consultation",
                    status: "Pending"
                ),

                new ClinicAppointment(
                    patientCode: 1008,
                    date: DateTime.Today.AddDays(5),
                    time: new TimeSpan(10, 30, 0),
                    patient: "Sofia Reyes",
                    contactInfo: "09171234574",
                    doctor: "Dr. Angela Reyes",
                    notes: "Cardiology follow-up",
                    status: "Confirmed"
                )
            };


            // =========================================
            // SAVE APPOINTMENTS TO DATABASE
            // =========================================

            context.Set<ClinicAppointment>().AddRange(appointments);

            context.SaveChanges();
        }
    }
}