using Contracts.Grpc.Models;
using FluentValidation;

namespace Api.Validators.Grpc;

public class GetWalletByCustomerIdGrpcRequestValidator : AbstractValidator<GetWalletByCustomerIdGrpcRequest>
{
    public GetWalletByCustomerIdGrpcRequestValidator()
    {
        RuleFor(request => request.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId must be greater than 0.");
    }
}
