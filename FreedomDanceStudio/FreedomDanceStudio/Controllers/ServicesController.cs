using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;

namespace FreedomDanceStudio.Controllers;

public class ServicesController: Controller
{
    private readonly ApplicationDbContext _context;

         public ServicesController(ApplicationDbContext context)
         {
             _context = context;
         }
         
         // GET: /Services
         public IActionResult Index()
         {
             return View(_context.Services.ToList());
         }
         
         // GET: /Services/Create
         public IActionResult Create()
         {
             return View();
         }
         
         // POST: /Services/Create
         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Create(Service service)
         {
             if (ModelState.IsValid)
             {
                 _context.Add(service);
                 await _context.SaveChangesAsync();
                 return RedirectToAction(nameof(Index));
             }
             return View(service);
         }
}