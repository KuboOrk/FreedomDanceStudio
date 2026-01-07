    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace FreedomDanceStudio.Models;

    public class SalaryCalculation
    {
        /// <summary>
        /// Уникальный идентификатор расчёта
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Ссылка на сотрудника (преподавателя)
        /// </summary>
        [Required(ErrorMessage = "Необходимо выбрать сотрудника")]
        [Display(Name = "Сотрудник")]
        int EmployeeId { get; set; }

        /// <summary>
        /// Навигация к сотруднику
        /// </summary>
        public virtual Employee? Employee { get; set; }

        /// <summary>
        /// Начало расчётного периода
        /// </summary>
        [Display(Name = "Период с")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Укажите начало периода")]
        DateTime PeriodStart { get; set; }

        /// <summary>
        /// Окончание расчётного периода
        /// </summary>
        [Display(Name = "Период по")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Укажите окончание периода")]
        DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Сумма к выплате
        /// </summary>
        [Display(Name = "Сумма")]
        [Range(0, 999999.99, ErrorMessage = "Сумма должна быть положительной")]
        decimal Amount { get; set; }

        /// <summary>
        /// Статус расчёта (Черновик, Оплачено и т. д.)
        /// </summary>
        [Display(Name = "Статус")]
        [StringLength(20)]
        string Status { get; set; } = "Draft"; // по умолчанию — черновик
    }