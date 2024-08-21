using FluentValidation;
using RenTN.Application.DTOs.ChangeLogDTOs;

namespace RenTN.Application.Validators.ChangeLogsValidators;

public class DateRangeValidator : AbstractValidator<DateRangeDTO>
{
    public DateRangeValidator()
    {
        RuleFor(x => x.EntityName).NotEmpty().WithMessage("Please provide the entity name!");
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("StartDate cannot be in the future.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("EndDate cannot be earlier than StartDate.");
    }
}
