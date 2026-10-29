using System.Diagnostics;
using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // Load active doctors for the featured section
        var featuredDoctors = await _context.Doctors
            .Where(d => d.IsActive)
            .OrderBy(d => d.Id)
            .Take(8)
            .ToListAsync();

        ViewBag.TotalDoctors = await _context.Doctors.CountAsync(d => d.IsActive);
        ViewBag.TotalAppointments = await _context.Appointments.CountAsync();

        return View(featuredDoctors);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
