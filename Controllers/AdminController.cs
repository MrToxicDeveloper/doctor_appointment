using MediCare.Data;
using MediCare.Models;
using MediCare.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /Admin
    public async Task<IActionResult> Index(string? status)
    {
        var appointmentsQuery = _context.Appointments
            .Include(a => a.Doctor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.Status == status);
        }

        var recentAppointments = await appointmentsQuery
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        var doctors = await _context.Doctors
            .OrderBy(d => d.Name)
            .ToListAsync();

        var totalAppointments = await _context.Appointments.CountAsync();
        var pendingAppointments = await _context.Appointments.CountAsync(a => a.Status == "Pending");
        var confirmedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Confirmed");
        var cancelledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Cancelled");

        var viewModel = new AdminDashboardViewModel
        {
            TotalDoctors = doctors.Count,
            TotalAppointments = totalAppointments,
            PendingAppointments = pendingAppointments,
            ConfirmedAppointments = confirmedAppointments,
            CancelledAppointments = cancelledAppointments,
            RecentAppointments = recentAppointments,
            Doctors = doctors,
            StatusFilter = status
        };

        return View(viewModel);
    }

    // GET: /Admin/Appointments
    public async Task<IActionResult> Appointments(string? status, string? search)
    {
        var query = _context.Appointments
            .Include(a => a.Doctor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            query = query.Where(a => a.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a => a.PatientName.ToLower().Contains(term) ||
                                     a.PatientEmail.ToLower().Contains(term) ||
                                     a.PatientPhone.Contains(term) ||
                                     (a.Doctor != null && a.Doctor.Name.ToLower().Contains(term)));
        }

        var appointments = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentStatus = status ?? "All";
        ViewBag.SearchTerm = search;

        return View(appointments);
    }

    // POST: /Admin/UpdateAppointmentStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, string newStatus, string? returnAction)
    {
        var allowedStatuses = new[] { "Pending", "Confirmed", "Cancelled" };
        if (!allowedStatuses.Contains(newStatus))
        {
            TempData["ErrorMessage"] = "Invalid appointment status specified.";
            return RedirectToAction(returnAction ?? nameof(Index));
        }

        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            TempData["ErrorMessage"] = "Appointment not found.";
            return RedirectToAction(returnAction ?? nameof(Index));
        }

        appointment.Status = newStatus;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Appointment #MED-{appointment.Id:D4} status updated to '{newStatus}'.";

        if (returnAction == "Appointments")
        {
            return RedirectToAction(nameof(Appointments));
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Doctors
    public async Task<IActionResult> Doctors()
    {
        var doctors = await _context.Doctors
            .OrderBy(d => d.Name)
            .ToListAsync();

        return View(doctors);
    }

    // GET: /Admin/CreateDoctor
    public IActionResult CreateDoctor()
    {
        return View(new Doctor { AvailableDays = "Monday - Friday", IsActive = true });
    }

    // POST: /Admin/CreateDoctor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDoctor(Doctor doctor)
    {
        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(doctor.ProfilePictureUrl))
            {
                doctor.ProfilePictureUrl = "/images/doctors/default-doctor.svg";
            }

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Doctor '{doctor.Name}' added successfully.";
            return RedirectToAction(nameof(Doctors));
        }

        return View(doctor);
    }

    // GET: /Admin/EditDoctor/5
    public async Task<IActionResult> EditDoctor(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }

    // POST: /Admin/EditDoctor/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDoctor(int id, Doctor doctor)
    {
        if (id != doctor.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(doctor.ProfilePictureUrl))
                {
                    doctor.ProfilePictureUrl = "/images/doctors/default-doctor.svg";
                }

                _context.Update(doctor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Doctor '{doctor.Name}' updated successfully.";
                return RedirectToAction(nameof(Doctors));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Doctors.AnyAsync(e => e.Id == doctor.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        return View(doctor);
    }

    // POST: /Admin/ToggleDoctorActive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDoctorActive(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null)
        {
            TempData["ErrorMessage"] = "Doctor not found.";
            return RedirectToAction(nameof(Doctors));
        }

        doctor.IsActive = !doctor.IsActive;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Doctor '{doctor.Name}' is now {(doctor.IsActive ? "Active" : "Inactive")}.";
        return RedirectToAction(nameof(Doctors));
    }

    // POST: /Admin/DeleteDoctor/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null)
        {
            TempData["ErrorMessage"] = "Doctor not found.";
            return RedirectToAction(nameof(Doctors));
        }

        // Safety Rule 1: Check for upcoming active appointments
        var hasUpcomingAppointments = await _context.Appointments
            .AnyAsync(a => a.DoctorId == id && a.Status != "Cancelled" && a.AppointmentDate.Date >= DateTime.Today);

        if (hasUpcomingAppointments)
        {
            TempData["ErrorMessage"] = $"Cannot delete '{doctor.Name}' because they have upcoming active appointments. Please cancel or reassign appointments first, or deactivate the doctor.";
            return RedirectToAction(nameof(Doctors));
        }

        // Safety Rule 2: If the doctor has any past or cancelled appointments in history,
        // do NOT hard-delete to avoid violating foreign keys and breaking patient appointment history.
        // Instead, safely soft-deactivate.
        var hasAppointmentHistory = await _context.Appointments
            .AnyAsync(a => a.DoctorId == id);

        if (hasAppointmentHistory)
        {
            doctor.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["InfoMessage"] = $"Doctor '{doctor.Name}' has existing appointment history. To preserve historical records safely, the doctor has been deactivated rather than permanently removed.";
            return RedirectToAction(nameof(Doctors));
        }

        // If no appointment history at all, safe to delete record completely
        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Doctor '{doctor.Name}' was deleted successfully.";
        return RedirectToAction(nameof(Doctors));
    }
}
