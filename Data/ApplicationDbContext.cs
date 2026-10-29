using MediCare.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Doctor entity
        builder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Specialization).IsRequired().HasMaxLength(100);
            entity.Property(d => d.ConsultationFee).HasPrecision(18, 2);
            entity.Property(d => d.AvailableDays).IsRequired().HasMaxLength(100);
        });

        // Configure Appointment entity
        builder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.PatientName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.PatientEmail).IsRequired().HasMaxLength(100);
            entity.Property(a => a.PatientPhone).IsRequired().HasMaxLength(20);
            entity.Property(a => a.AppointmentTime).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Reason).IsRequired().HasMaxLength(500);
            entity.Property(a => a.Status).IsRequired().HasMaxLength(50);

            // Restrict deletion of Doctor if appointments exist to protect appointment history
            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Index to optimize booking availability lookup & email searching
            entity.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.AppointmentTime });
            entity.HasIndex(a => a.PatientEmail);
        });
    }
}
