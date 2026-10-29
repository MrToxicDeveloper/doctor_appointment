using System.ComponentModel.DataAnnotations;

namespace MediCare.Models.Attributes;

public class FutureOrTodayDateAttribute : ValidationAttribute
{
    public FutureOrTodayDateAttribute()
        : base("Appointment date cannot be in the past.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateValue)
        {
            if (dateValue.Date < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "Appointment date cannot be in the past.");
            }
        }
        else if (value is DateOnly dateOnlyValue)
        {
            if (dateOnlyValue < DateOnly.FromDateTime(DateTime.Today))
            {
                return new ValidationResult(ErrorMessage ?? "Appointment date cannot be in the past.");
            }
        }

        return ValidationResult.Success;
    }
}
