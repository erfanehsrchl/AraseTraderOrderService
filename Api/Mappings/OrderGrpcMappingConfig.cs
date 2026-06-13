using Application.DTOs.Orders;
using Contracts.Enums;
using Contracts.Grpc.Models;
using Mapster;

namespace Api.Mappings;

public class OrderGrpcMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GetOrderByTrackingIdOutDto, GetOrderByTrackingIdGrpcResponse>()
            .Map(destination => destination.TrackingId, source => source.TrackingId)
            .Map(destination => destination.CustomerId, source => source.CustomerId)
            .Map(destination => destination.Side, source => (OrderSideContract)source.Side)
            .Map(destination => destination.Amount, source => source.Amount)
            .Map(destination => destination.Status, source => (OrderStatusContract)source.Status)
            .Map(destination => destination.FailureReason, source => source.FailureReason)
            .Map(destination => destination.CreatedAt, source => source.CreatedAt)
            .Map(destination => destination.UpdatedAt, source => source.UpdatedAt);
    }
}
