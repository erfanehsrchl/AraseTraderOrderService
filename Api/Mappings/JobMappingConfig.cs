using Api.ViewModels.Jobs;
using Application.DTOs.Jobs;
using Mapster;

namespace Api.Mappings;

public class JobMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProcessPendingOrdersOutDto, ProcessPendingOrdersOutVm>()
            .Map(destination => destination.ProcessedCount, source => source.ProcessedCount)
            .Map(destination => destination.FailedCount, source => source.FailedCount)
            .Map(destination => destination.SkippedCount, source => source.SkippedCount)
            .Map(destination => destination.Message, source => source.Message);
    }
}
