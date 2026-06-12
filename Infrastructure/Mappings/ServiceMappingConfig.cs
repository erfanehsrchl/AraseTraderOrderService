using Application.DTOs.Orders;
using Application.DTOs.Wallets;
using Contracts.Events;
using Domain.Entities;
using Domain.Enums;
using Mapster;

namespace Infrastructure.Mappings;

public class ServiceMappingConfig : IRegister
{
    private const string OrderRegisteredMessage = "Order has been registered and is pending wallet processing.";

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AddOrderInDto, Order>()
            .Map(destination => destination.TrackingId, source => source.TrackingId)
            .Map(destination => destination.CustomerId, source => source.CustomerId)
            .Map(destination => destination.Side, source => source.Side)
            .Map(destination => destination.Amount, source => source.Amount);

        config.NewConfig<Order, AddOrderOutDto>()
            .Map(destination => destination.TrackingId, source => source.TrackingId)
            .Map(destination => destination.Status, source => source.Status)
            .Map(destination => destination.Message, _ => OrderRegisteredMessage);

        config.NewConfig<CreateOrderEvent, AddOrderInDto>()
            .Map(destination => destination.TrackingId, source => source.TrackingId)
            .Map(destination => destination.CustomerId, source => source.CustomerId)
            .Map(destination => destination.Side, source => (OrderSide)source.Side)
            .Map(destination => destination.Amount, source => source.Amount);

        config.NewConfig<Wallet, GetWalletByCustomerIdOutDto>()
            .Map(destination => destination.WalletId, source => source.Id)
            .Map(destination => destination.CustomerId, source => source.CustomerId)
            .Map(destination => destination.Balance, source => source.Balance);

        config.NewConfig<WalletTransaction, WalletTransactionOutDto>()
            .Map(destination => destination.Id, source => source.Id)
            .Map(destination => destination.WalletId, source => source.WalletId)
            .Map(destination => destination.OrderId, source => source.OrderId)
            .Map(destination => destination.Amount, source => source.Amount)
            .Map(destination => destination.BalanceBefore, source => source.BalanceBefore)
            .Map(destination => destination.BalanceAfter, source => source.BalanceAfter)
            .Map(destination => destination.Reason, source => (int)source.Reason)
            .Map(destination => destination.CreatedAt, source => source.CreatedAt);
    }
}
