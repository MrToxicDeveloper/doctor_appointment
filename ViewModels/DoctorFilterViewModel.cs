using MediCare.Models;

namespace MediCare.ViewModels;

public class DoctorFilterViewModel
{
    public IEnumerable<Doctor> Doctors { get; set; } = new List<Doctor>();
    public string? SearchTerm { get; set; }
    public string? Specialization { get; set; }
    public List<string> AllSpecializations { get; set; } = new();
}
