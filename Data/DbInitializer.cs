using MediCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        // Ensure database schema is created
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Roles
        string[] roles = { "Admin", "Patient" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Admin User
        var adminEmail = configuration["AdminUser:Email"] ?? "admin@medicare.com";
        var adminPassword = configuration["AdminUser:Password"] ?? "Admin@123MediCare!";
        var adminFullName = configuration["AdminUser:FullName"] ?? "System Administrator";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 3. Seed Doctors
        if (!await context.Doctors.AnyAsync())
        {
            var doctors = new List<Doctor>
            {
                new Doctor
                {
                    Name = "Dr. Rahul Sharma",
                    Specialization = "Cardiologist",
                    Experience = 14,
                    ConsultationFee = 800.00m,
                    AvailableDays = "Monday - Friday",
                    ProfilePictureUrl = "/images/doctors/dr-rahul.svg",
                    Bio = "Senior Consultant Cardiologist specializing in adult cardiology, heart rhythm disorders, and preventive cardiac wellness.",
                    IsActive = true
                },
                new Doctor
                {
                    Name = "Dr. Priya Patel",
                    Specialization = "Dermatologist",
                    Experience = 9,
                    ConsultationFee = 600.00m,
                    AvailableDays = "Monday - Saturday",
                    ProfilePictureUrl = "/images/doctors/dr-priya.svg",
                    Bio = "Expert in clinical and aesthetic dermatology, hair and skin treatment, and pediatric dermatological care.",
                    IsActive = true
                },
                new Doctor
                {
                    Name = "Dr. Amit Verma",
                    Specialization = "General Physician",
                    Experience = 16,
                    ConsultationFee = 500.00m,
                    AvailableDays = "Monday - Saturday",
                    ProfilePictureUrl = "/images/doctors/dr-amit.svg",
                    Bio = "Experienced family physician offering comprehensive care for chronic illnesses, seasonal infections, and wellness screenings.",
                    IsActive = true
                },
                new Doctor
                {
                    Name = "Dr. Neha Shah",
                    Specialization = "Pediatrician",
                    Experience = 11,
                    ConsultationFee = 650.00m,
                    AvailableDays = "Monday - Friday",
                    ProfilePictureUrl = "/images/doctors/dr-neha.svg",
                    Bio = "Compassionate child healthcare specialist focusing on infant nutrition, immunization, and pediatric developmental health.",
                    IsActive = true
                },
                new Doctor
                {
                    Name = "Dr. Karan Mehta",
                    Specialization = "Orthopedic",
                    Experience = 13,
                    ConsultationFee = 900.00m,
                    AvailableDays = "Tuesday - Saturday",
                    ProfilePictureUrl = "/images/doctors/dr-karan.svg",
                    Bio = "Renowned orthopedic surgeon focused on joint preservation, sports injury rehabilitation, and arthritis management.",
                    IsActive = true
                }
            };

            await context.Doctors.AddRangeAsync(doctors);
            await context.SaveChangesAsync();

            // 4. Seed Demo Appointments for verification
            var drSharma = doctors[0];
            var drPatel = doctors[1];
            var drVerma = doctors[2];

            var tomorrow = DateTime.Today.AddDays(1);
            var dayAfter = DateTime.Today.AddDays(2);

            var demoAppointments = new List<Appointment>
            {
                new Appointment
                {
                    PatientName = "Ananya Iyer",
                    PatientEmail = "ananya.iyer@example.com",
                    PatientPhone = "9876543210",
                    DoctorId = drSharma.Id,
                    AppointmentDate = tomorrow,
                    AppointmentTime = "10:00 AM",
                    Reason = "Routine annual cardiac evaluation and blood pressure checkup",
                    Status = "Confirmed",
                    CreatedAt = DateTime.UtcNow.AddHours(-6)
                },
                new Appointment
                {
                    PatientName = "Rohan Das",
                    PatientEmail = "rohan.das@example.com",
                    PatientPhone = "9823456789",
                    DoctorId = drPatel.Id,
                    AppointmentDate = tomorrow,
                    AppointmentTime = "11:30 AM",
                    Reason = "Skin rash and allergy consultation following travel",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddHours(-3)
                },
                new Appointment
                {
                    PatientName = "Vikram Malhotra",
                    PatientEmail = "vikram.m@example.com",
                    PatientPhone = "9988776655",
                    DoctorId = drVerma.Id,
                    AppointmentDate = dayAfter,
                    AppointmentTime = "02:30 PM",
                    Reason = "Mild persistent fever and fatigue for 3 days",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                }
            };

            await context.Appointments.AddRangeAsync(demoAppointments);
            await context.SaveChangesAsync();
        }
    }
}
