using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.EntityFrameworkCore;

namespace FreedomDanceStudio.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Employees — список всех сотрудников
        public IActionResult Index()
        {
            var employees = _context.Employees.Where(e => e.IsActive).ToList();
            return View(employees);
        }

        // GET: /Employees/Create — форма добавления нового сотрудника
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Employees/Create — обработка отправки формы
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: /Employees/Edit/5 — форма редактирования
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: /Employees/Edit/5 — обработка сохранения изменений
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(employee.Id))
                return NotFound();
            else
                throw;
        }
        return RedirectToAction(nameof(Index));
    }
    return View(employee);
}

        // GET: /Employees/Delete/5 — подтверждение удаления
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: /Employees/Delete/5 — фактическое удаление
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id) => _context.Employees.Any(e => e.Id == id);
    } 
}
