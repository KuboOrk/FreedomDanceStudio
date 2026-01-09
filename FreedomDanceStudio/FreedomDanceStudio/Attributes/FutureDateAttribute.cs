using System.ComponentModel.DataAnnotations;

namespace FreedomDanceStudio.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FutureDateAttribute : ValidationAttribute
{
    public FutureDateAttribute()
        : base("Дата должна быть не ранее сегодняшнего дня.")
    {
    }

    public FutureDateAttribute(string errorMessage)
        : base(errorMessage)
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || value is not DateTime dateValue)
            return ValidationResult.Success;

        return dateValue.Date < DateTime.Today
            ? new ValidationResult(ErrorMessage ?? "Дата должна быть не ранее сегодняшнего дня.")
            : ValidationResult.Success;
    }
}