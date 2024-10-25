using Apartments.Application.Dtos.AdminDtos;
using Apartments.Domain.Common;
using FluentValidation;

namespace Apartments.Application.Validators;

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