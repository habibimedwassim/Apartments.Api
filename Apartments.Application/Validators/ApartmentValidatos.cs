using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;
using FluentValidation;

namespace Apartments.Application.Validators;

public class ApartmentQueryFilterValidator : AbstractValidator<ApartmentQueryFilter>
{
    private string[] allowedSortByColumnNames =
    [
        nameof(ApartmentDto.CreatedDate).ToLower(),
        nameof(ApartmentDto.RentAmount).ToLower(),
        nameof(ApartmentDto.Size).ToLower()
    ];

    public ApartmentQueryFilterValidator()
    {
        RuleFor(x => x.pageNumber)
            .NotEmpty()
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.sortBy)
            .Must(x => allowedSortByColumnNames.Contains(x))
            .When(x => x.sortBy != null)
            .WithMessage($"Sort by is optional, or must be in [{string.Join(",", allowedSortByColumnNames)}]");
    }
}

public class CreateApartmentDtoValidator : AbstractValidator<CreateApartmentDto>
{
    public CreateApartmentDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(200).WithMessage("Street cannot exceed 200 characters.");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .Matches(@"^\d{4}$").WithMessage("Postal code must be exactly 4 digits.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Size)
            .GreaterThanOrEqualTo(0).WithMessage("Size must be positive.");

        RuleFor(x => x.RentAmount)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.ApartmentPhotos)
            .Must(photos => photos.Count <= AppConstants.PhotosLimit)
            .WithMessage($"You cannot upload more than {AppConstants.PhotosLimit} photos.")
            .When(x => x.ApartmentPhotos.Count > 0);
    }
}

public class UpdateApartmentDtoValidator : AbstractValidator<UpdateApartmentDto>
{
    public UpdateApartmentDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(200).WithMessage("Street cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Street));

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .Matches(@"^\d{4}$").WithMessage("Postal code must be exactly 4 digits.")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Size)
            .GreaterThanOrEqualTo(0).WithMessage("Size must be positive.")
            .When(x => x.Size != null);

        RuleFor(x => x.RentAmount)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .When(x => x.RentAmount != null);
    }
}