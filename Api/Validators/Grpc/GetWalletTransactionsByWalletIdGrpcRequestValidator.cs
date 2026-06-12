using Contracts.Grpc.Models;
using FluentValidation;

namespace Api.Validators.Grpc;

public class GetWalletTransactionsByWalletIdGrpcRequestValidator : AbstractValidator<GetWalletTransactionsByWalletIdGrpcRequest>
{
    public GetWalletTransactionsByWalletIdGrpcRequestValidator()
    {
        RuleFor(request => request.WalletId)
            .GreaterThan(0)
            .WithMessage("WalletId must be greater than 0.");
    }
}
