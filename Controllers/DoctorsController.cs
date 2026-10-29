using MediCare.Data;
using MediCare.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Controllers;

public class DoctorsController : Controller
{
    private readonly ApplicationDbContext _context;

    public DoctorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Doctors
    public async Task<IActionResult> Index(string? searchTerm, string? specialization)
    {
        var query = _context.Doctors.Where(d => d.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(d => d.Name.ToLower().Contains(term) || 
                                     d.Specialization.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(specialization) && specialization != "All")
        {
            query = query.Where(d => d.Specialization == specialization);
        }

        var doctors = await query.OrderBy(d => d.Name).ToListAsync();

        var specializations = await _context.Doctors
            .Where(d => d.IsActive)
            .Select(d => d.Specialization)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        var viewModel = new DoctorFilterViewModel
        {
            Doctors = doctors,
            SearchTerm = searchTerm,
            Specialization = specialization,
            AllSpecializations = specializations
        };

        return View(viewModel);
    }

    // GET: /Doctors/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }
}
