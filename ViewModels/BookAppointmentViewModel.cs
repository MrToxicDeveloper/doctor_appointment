using System.ComponentModel.DataAnnotations;
using MediCare.Models;
using MediCare.Models.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCare.ViewModels;

public class BookAppointmentViewModel
{
    [Required(ErrorMessage = "Please enter your full name")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Full Name")]
    public string PatientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100)]
    [Display(Name = "Email Address")]
    public string PatientEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your phone number")]
    [RegularExpression(@"^(\+?[0-9]{1,4}[\s-]?)?\(?[0-9]{3}\)?[\s-]?[0-9]{3}[\s-]?[0-9]{4}$|^[0-9]{10}$", 
        ErrorMessage = "Please enter a valid phone number (e.g. 9876543210)")]
    [StringLength(20)]
    [Display(Name = "Phone Number")]
    public string PatientPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a doctor")]
    [Display(Name = "Select Doctor")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Please select an appointment date")]
    [DataType(DataType.Date)]
    [Display(Name = "Appointment Date")]
    [FutureOrTodayDate(ErrorMessage = "Appointment date cannot be in the past.")]
    public DateTime AppointmentDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please select a time slot")]
    [Display(Name = "Appointment Time Slot")]
    public string AppointmentTime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please describe the reason for your visit")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Reason must be between 5 and 500 characters")]
    [Display(Name = "Reason for Visit")]
    public string Reason { get; set; } = string.Empty;

    // Helper data for rendering dropdowns and doctor info
    public IEnumerable<SelectListItem> DoctorList { get; set; } = new List<SelectListItem>();
    public Doctor? SelectedDoctor { get; set; }

    public static readonly List<string> StandardTimeSlots = new()
    {
        "09:00 AM",
        "09:30 AM",
        "10:00 AM",
        "10:30 AM",
        "11:00 AM",
        "11:30 AM",
        "02:00 PM",
        "02:30 PM",
        "03:00 PM",
        "03:30 PM",
        "04:00 PM",
        "04:30 PM",
        "05:00 PM"
    };
}
