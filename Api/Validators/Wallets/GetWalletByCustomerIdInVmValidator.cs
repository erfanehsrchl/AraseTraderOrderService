using Api.ViewModels.Wallets;
using FluentValidation;

namespace Api.Validators.Wallets;

public class GetWalletByCustomerIdInVmValidator : AbstractValidator<GetWalletByCustomerIdInVm>
{
    public GetWalletByCustomerIdInVmValidator()
    {
        RuleFor(request => request.CustomerId)
            .GreaterThan(0)
            .WithMessage("customerId must be greater than 0.");
    }
}
