using FluentValidation;
using RenTN.Application.DTOs.ApartmentDTOs;

namespace RenTN.Application.Validators.ApartmentValidators;

public class CreateApartmentValidator : AbstractValidator<CreateApartmentDTO>
{
    public CreateApartmentValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .Length(3, 100).WithMessage("Description cannot exceed 100 characters.");

        RuleFor(x => x.Size)
            .GreaterThanOrEqualTo(0).WithMessage("Size must be 0 or greater.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("PostalCode is required.")
            .Matches(@"^\d{4}$").WithMessage("PostalCode must be a 4-digit number.");
    }
}
