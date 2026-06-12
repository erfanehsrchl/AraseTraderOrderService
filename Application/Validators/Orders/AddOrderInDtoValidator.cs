using Application.DTOs.Orders;
using FluentValidation;

namespace Application.Validators.Orders;

public class AddOrderInDtoValidator : AbstractValidator<AddOrderInDto>
{
    public AddOrderInDtoValidator()
    {
        RuleFor(order => order.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId must be greater than 0.");

        RuleFor(order => order.Side)
            .IsInEnum()
            .WithMessage("Order side is invalid.");

        RuleFor(order => order.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0.");
    }
}
