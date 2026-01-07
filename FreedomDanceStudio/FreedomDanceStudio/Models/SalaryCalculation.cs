using System;

namespace FreedomDanceStudio.Models
{
    public class SalaryCalculation
    {
        public Employee Employee { get; set; }
        public int LessonCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}