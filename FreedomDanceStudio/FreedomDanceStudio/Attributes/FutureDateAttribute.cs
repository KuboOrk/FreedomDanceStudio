using System;
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

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is DateTime dateValue)
        {
            if (dateValue.Date < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ??
                                            "Дата должна быть не ранее сегодняшнего дня.");
            }
        }

        return ValidationResult.Success;
    }
}