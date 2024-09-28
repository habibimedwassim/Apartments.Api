using Apartments.Application.Dtos.AdminDtos;
using Apartments.Domain.Common;
using FluentValidation;

namespace Apartments.Application.Validators;

public class ChangeLogValidator : AbstractValidator<ChangeLogDto>
{
    public ChangeLogValidator()
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

public class AssignRoleValidator : AbstractValidator<AssignRoleDto>
{
    public AssignRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(x => x.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                       x.Equals(UserRoles.Owner, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Role must be Admin or Owner");
    }
}