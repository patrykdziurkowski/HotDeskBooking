using FluentValidation;

namespace App;

public class AddLocationModelValidator : AbstractValidator<AddLocationModel>
{
    public AddLocationModelValidator()
    {
        RuleFor(l => l.Floor)
            .NotEmpty().WithMessage($"{nameof(AddLocationModel.Floor)} is required.");
        RuleFor(l => l.BuildingNumber)
            .NotEmpty().WithMessage($"{nameof(AddLocationModel.BuildingNumber)} is required.")
            .GreaterThan(0).WithMessage($"{nameof(AddLocationModel.BuildingNumber)} must be greater than zero.");
    }
}
