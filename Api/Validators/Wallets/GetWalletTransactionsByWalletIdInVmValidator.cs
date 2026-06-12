using Api.ViewModels.Wallets;
using FluentValidation;

namespace Api.Validators.Wallets;

public class GetWalletTransactionsByWalletIdInVmValidator : AbstractValidator<GetWalletTransactionsByWalletIdInVm>
{
    public GetWalletTransactionsByWalletIdInVmValidator()
    {
        RuleFor(request => request.WalletId)
            .GreaterThan(0)
            .WithMessage("walletId must be greater than 0.");
    }
}
