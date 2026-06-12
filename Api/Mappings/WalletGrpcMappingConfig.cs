using Application.DTOs.Wallets;
using Contracts.Grpc.Models;
using Mapster;

namespace Api.Mappings;

public class WalletGrpcMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GetWalletByCustomerIdOutDto, GetWalletByCustomerIdGrpcResponse>()
            .Map(destination => destination.WalletId, source => source.WalletId)
            .Map(destination => destination.CustomerId, source => source.CustomerId)
            .Map(destination => destination.Balance, source => source.Balance);

        config.NewConfig<WalletTransactionOutDto, WalletTransactionGrpcDto>()
            .Map(destination => destination.Id, source => source.Id)
            .Map(destination => destination.WalletId, source => source.WalletId)
            .Map(destination => destination.OrderId, source => source.OrderId)
            .Map(destination => destination.Amount, source => source.Amount)
            .Map(destination => destination.BalanceBefore, source => source.BalanceBefore)
            .Map(destination => destination.BalanceAfter, source => source.BalanceAfter)
            .Map(destination => destination.Reason, source => source.Reason)
            .Map(destination => destination.CreatedAt, source => source.CreatedAt);

        config.NewConfig<GetWalletTransactionsByWalletIdOutDto, GetWalletTransactionsByWalletIdGrpcResponse>()
            .Map(destination => destination.Transactions, source => source.Transactions);
    }
}
