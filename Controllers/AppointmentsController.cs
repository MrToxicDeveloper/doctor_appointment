using MediCare.Data;
using MediCare.Models;
using MediCare.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Controllers;

public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(ApplicationDbContext context, ILogger<AppointmentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /Appointments/Book?doctorId=1
    public async Task<IActionResult> Book(int? doctorId)
    {
        var model = new BookAppointmentViewModel
        {
            AppointmentDate = DateTime.Today.AddDays(1)
        };

        if (doctorId.HasValue)
        {
            model.DoctorId = doctorId.Value;
            model.SelectedDoctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId.Value && d.IsActive);
        }

        await PopulateDoctorListAsync(model);
        return View(model);
    }

    // POST: /Appointments/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(BookAppointmentViewModel model)
    {
        // 1. Validate Past Date
        if (model.AppointmentDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.AppointmentDate), "Appointment date cannot be in the past. Please select today or a future date.");
        }

        // 2. Validate Doctor Existence
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == model.DoctorId && d.IsActive);
        if (doctor == null)
        {
            ModelState.AddModelError(nameof(model.DoctorId), "The selected doctor was not found or is currently not available.");
        }

        // 3. Prevent duplicate active booking for the same doctor, date, and time slot
        if (ModelState.IsValid)
        {
            var isSlotTaken = await _context.Appointments
                .AnyAsync(a => a.DoctorId == model.DoctorId 
                            && a.AppointmentDate.Date == model.AppointmentDate.Date 
                            && a.AppointmentTime == model.AppointmentTime 
                            && a.Status != "Cancelled");

            if (isSlotTaken)
            {
                ModelState.AddModelError(nameof(model.AppointmentTime), 
                    $"Dr. {doctor?.Name ?? "Selected Doctor"} is already booked for {model.AppointmentDate:dd MMM yyyy} at {model.AppointmentTime}. Please select a different time slot or date.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateDoctorListAsync(model);
            if (model.DoctorId > 0)
            {
                model.SelectedDoctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == model.DoctorId);
            }
            return View(model);
        }

        // Create and persist appointment
        var appointment = new Appointment
        {
            PatientName = model.PatientName.Trim(),
            PatientEmail = model.PatientEmail.Trim().ToLower(),
            PatientPhone = model.PatientPhone.Trim(),
            DoctorId = model.DoctorId,
            AppointmentDate = model.AppointmentDate.Date,
            AppointmentTime = model.AppointmentTime,
            Reason = model.Reason.Trim(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment booked successfully!";
            return RedirectToAction(nameof(BookingSuccess), new { id = appointment.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save appointment for {Email}", model.PatientEmail);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while booking your appointment. Please try again.");
            await PopulateDoctorListAsync(model);
            return View(model);
        }
    }

    // GET: /Appointments/BookingSuccess/5
    public async Task<IActionResult> BookingSuccess(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            return RedirectToAction(nameof(MyAppointments));
        }

        return View(appointment);
    }

    // GET: /Appointments/MyAppointments?email=user@example.com
    public async Task<IActionResult> MyAppointments(string? email)
    {
        var model = new MyAppointmentsViewModel();

        // If user is authenticated patient, prefill email if not provided
        if (string.IsNullOrWhiteSpace(email) && User.Identity?.IsAuthenticated == true)
        {
            email = User.Identity.Name;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var cleanEmail = email.Trim().ToLower();
            model.Email = cleanEmail;
            model.HasSearched = true;

            model.Appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientEmail.ToLower() == cleanEmail)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        return View(model);
    }

    // POST: /Appointments/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? returnEmail)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            TempData["ErrorMessage"] = "Appointment not found.";
            return RedirectToAction(nameof(MyAppointments), new { email = returnEmail });
        }

        if (appointment.Status == "Cancelled")
        {
            TempData["InfoMessage"] = "This appointment is already cancelled.";
            return RedirectToAction(nameof(MyAppointments), new { email = returnEmail });
        }

        // Prevent cancelling appointments that have already passed
        if (appointment.AppointmentDate.Date < DateTime.Today)
        {
            TempData["ErrorMessage"] = "Cannot cancel an appointment that has already passed.";
            return RedirectToAction(nameof(MyAppointments), new { email = returnEmail });
        }

        appointment.Status = "Cancelled";
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Appointment #MED-{appointment.Id:D4} with {appointment.Doctor?.Name} on {appointment.AppointmentDate:dd MMM yyyy} has been cancelled.";
        return RedirectToAction(nameof(MyAppointments), new { email = returnEmail ?? appointment.PatientEmail });
    }

    // Helper to populate doctor dropdown
    private async Task PopulateDoctorListAsync(BookAppointmentViewModel model)
    {
        var doctors = await _context.Doctors
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();

        model.DoctorList = doctors.Select(d => new SelectListItem
        {
            Value = d.Id.ToString(),
            Text = $"{d.Name} — {d.Specialization} (₹{d.ConsultationFee:N0})"
        });
    }
}
