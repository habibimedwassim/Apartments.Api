using FluentValidation;
using RenTN.Application.DTOs.ApartmentPhotoDTOs;

namespace RenTN.Application.Validators.ApartmentPhotoValidators;

public class CreateUpdateApartmentPhotoValidator : AbstractValidator<CreateUpdateApartmentPhotoDTO>
{
    public CreateUpdateApartmentPhotoValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("Photo URL cannot be empty.")
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Photo URL must be a valid URL.");
    }
}
