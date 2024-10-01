using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;
using FluentValidation;

namespace Apartments.Application.Validators;

public class ApartmentRequestQueryFilterValidator : AbstractValidator<ApartmentRequestQueryFilter>
{
    private string[] allowedApartmentRequestTypes = Enum.GetNames(typeof(ApartmentRequestType));

    private string[] allowedSortByColumnNames =
    [
        nameof(ApartmentRequestDto.CreatedDate).ToLower()
    ];

    public ApartmentRequestQueryFilterValidator()
    {
        RuleFor(x => x.pageNumber)
            .NotEmpty()
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.type)
            .NotEmpty()
            .Must(x => allowedApartmentRequestTypes.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Type is mandatory and must be in [{string.Join(",", allowedApartmentRequestTypes)}]");

        RuleFor(x => x.sortBy)
            .Must(x => allowedSortByColumnNames.Contains(x))
            .When(x => x.sortBy != null)
            .WithMessage($"Sort by is optional, or must be in [{string.Join(",", allowedSortByColumnNames)}]");
    }
}

public class ApartmentRequestValidators
{
}