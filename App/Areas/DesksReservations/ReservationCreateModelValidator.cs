using FluentValidation;

namespace App;

public class ReservationCreateModelValidator : AbstractValidator<ReservationCreateModel>
{
    public ReservationCreateModelValidator()
    {
        RuleFor(m => m.DeskId)
            .NotEmpty();
        RuleFor(m => m.StartDate)
            .NotEmpty()
            .Must((model, startDate) => startDate >= model.EndDate);
        RuleFor(m => m.EndDate)
            .NotEmpty();
    }
}
