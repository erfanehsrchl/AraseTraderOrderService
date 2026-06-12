using Api.ViewModels.Orders;
using FluentValidation;

namespace Api.Validators.Orders;

public class AddOrderInVmValidator : AbstractValidator<AddOrderInVm>
{
    public AddOrderInVmValidator()
    {
        RuleFor(order => order.TrackingId)
            .NotEmpty()
            .WithMessage("TrackingId is required.");

        RuleFor(order => order.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId must be greater than zero.");

        RuleFor(order => order.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(order => order.Side)
            .IsInEnum()
            .WithMessage("Order side is invalid.");
    }
}
