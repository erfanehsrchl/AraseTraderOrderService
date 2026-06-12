using Api.ViewModels.Orders;
using Application.DTOs.Orders;
using Mapster;

namespace Api.Mappings;

public class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AddOrderInVm, AddOrderInDto>()
            .Map(destination => destination.TrackingId, source => source.TrackingId)
            .Map(destination => destination.CustomerId, source => source.CustomerId)
            .Map(destination => destination.Side, source => source.Side)
            .Map(destination => destination.Amount, source => source.Amount);

        config.NewConfig<AddOrderOutDto, AddOrderOutVm>()
            .Map(destination => destination.TrackingId, source => source.TrackingId)
            .Map(destination => destination.Status, source => source.Status)
            .Map(destination => destination.Message, source => source.Message);
    }
}
