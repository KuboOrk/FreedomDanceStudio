using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using System.Collections.Generic;
using System.Linq;

namespace FreedomDanceStudio.Controllers
{
        public class SalaryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Salary
        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            // Устанавливаем значения по умолчанию
            startDate ??= DateTime.Today.AddMonths(-1);
            endDate ??= DateTime.Today;

            // 1. Получаем активных сотрудников
            var activeEmployees = _context.Employees
                .Where(e => e.IsActive)
                .ToList();

            // 2. Для каждого сотрудника рассчитываем данные
            var calculations = new List<SalaryCalculation>();

            foreach (var employee in activeEmployees)
            {
                // Считаем количество занятий в заданном периоде
                var lessonCount = _context.Lessons
                    .Count(l => l.EmployeeId == employee.Id &&
                                l.Date >= startDate && l.Date <= endDate);

                // Рассчитываем сумму к выплате
                var totalAmount = employee.SalaryRate * lessonCount;

                calculations.Add(new SalaryCalculation
                {
                    Employee = employee,
                    LessonCount = lessonCount,
                    TotalAmount = totalAmount
                });
            }

            // Передаём данные в представление
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(calculations);
        }
    }
}