using System.ComponentModel.DataAnnotations;
using MediCare.Models;

namespace MediCare.ViewModels;

public class MyAppointmentsViewModel
{
    [Display(Name = "Enter your Email Address")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? Email { get; set; }

    public List<Appointment> Appointments { get; set; } = new();

    public bool HasSearched { get; set; }
}
