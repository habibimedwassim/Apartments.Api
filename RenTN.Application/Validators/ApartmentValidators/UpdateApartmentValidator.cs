using FluentValidation;
using RenTN.Application.DTOs.ApartmentDTOs;

namespace RenTN.Application.Validators.ApartmentValidators;

public class UpdateApartmentValidator : AbstractValidator<UpdateApartmentDTO>
{
    public UpdateApartmentValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Size)
            .GreaterThanOrEqualTo(0).WithMessage("Size must be 0 or greater.")
            .When(x => x.Size.HasValue);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .When(x => !string.IsNullOrEmpty(x.Street));

        RuleFor(x => x.PostalCode)
            .Matches(@"^\d{4}$").WithMessage("PostalCode must be a 4-digit number.")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.ApartmentPhotoUrls)
            .ForEach(photo =>
            {
                photo.NotEmpty().WithMessage("Photo URL cannot be empty.");
                photo.Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute)).WithMessage("Photo URL must be a valid URL.");
            })
            .When(x => x.ApartmentPhotoUrls != null && x.ApartmentPhotoUrls.Count > 0);
    }
}
