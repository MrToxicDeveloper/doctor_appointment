using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCare.Models;

public class Doctor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Doctor name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Doctor Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialization is required")]
    [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
    public string Specialization { get; set; } = string.Empty;

    [Required(ErrorMessage = "Experience is required")]
    [Range(0, 70, ErrorMessage = "Experience must be between 0 and 70 years")]
    [Display(Name = "Experience (Years)")]
    public int Experience { get; set; }

    [Required(ErrorMessage = "Consultation fee is required")]
    [Range(0, 100000, ErrorMessage = "Fee must be a valid positive amount")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Consultation Fee (₹)")]
    public decimal ConsultationFee { get; set; }

    [Required(ErrorMessage = "Available days are required")]
    [StringLength(100, ErrorMessage = "Available days cannot exceed 100 characters")]
    [Display(Name = "Available Days")]
    public string AvailableDays { get; set; } = "Monday - Friday";

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Photo URL / Filename")]
    [StringLength(255)]
    public string? ProfilePictureUrl { get; set; }

    [Display(Name = "Doctor Biography / Details")]
    [StringLength(500)]
    public string? Bio { get; set; }

    // Navigation property
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
