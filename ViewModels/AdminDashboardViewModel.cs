using MediCare.Models;

namespace MediCare.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public int CancelledAppointments { get; set; }

    public List<Appointment> RecentAppointments { get; set; } = new();
    public List<Doctor> Doctors { get; set; } = new();
    public string? StatusFilter { get; set; }
}
