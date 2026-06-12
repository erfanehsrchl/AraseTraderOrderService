using Api.ViewModels.Customers;
using Application.DTOs.Customers;
using Mapster;

namespace Api.Mappings;

public class CustomerSyncMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustomerSyncOutDto, SyncCustomersOutVm>()
            .Map(destination => destination.InsertedCount, source => source.InsertedCount)
            .Map(destination => destination.UpdatedCount, source => source.UpdatedCount)
            .Map(destination => destination.WalletCreatedCount, source => source.WalletCreatedCount)
            .Map(destination => destination.Message, source => source.Message);
    }
}
