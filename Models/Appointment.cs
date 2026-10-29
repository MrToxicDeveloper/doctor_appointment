using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCare.Models;

public class Appointment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Patient full name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Patient Name")]
    public string PatientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100)]
    [Display(Name = "Email Address")]
    public string PatientEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^(\+?[0-9]{1,4}[\s-]?)?\(?[0-9]{3}\)?[\s-]?[0-9]{3}[\s-]?[0-9]{4}$|^[0-9]{10}$", 
        ErrorMessage = "Please enter a valid 10-digit phone number")]
    [StringLength(20)]
    [Display(Name = "Phone Number")]
    public string PatientPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a doctor")]
    [Display(Name = "Doctor")]
    public int DoctorId { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public virtual Doctor? Doctor { get; set; }

    [Required(ErrorMessage = "Appointment date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Appointment Date")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Appointment time slot is required")]
    [StringLength(50)]
    [Display(Name = "Time Slot")]
    public string AppointmentTime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Reason for visit is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Reason must be between 5 and 500 characters")]
    [Display(Name = "Reason for Visit")]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

    [Display(Name = "Booking Date & Time")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
