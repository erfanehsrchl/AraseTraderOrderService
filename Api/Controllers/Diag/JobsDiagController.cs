using Api.Constants;
using Api.ViewModels.Jobs;
using Infrastructure.Jobs;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagJobRoutes.Base)]
public class JobsDiagController : ControllerBase
{
    private readonly ProcessPendingOrdersJob _processPendingOrdersJob;

    public JobsDiagController(ProcessPendingOrdersJob processPendingOrdersJob)
    {
        _processPendingOrdersJob = processPendingOrdersJob;
    }

    [HttpPost(DiagJobRoutes.ProcessPendingOrders)]
    public async Task<ActionResult<ProcessPendingOrdersOutVm>> ProcessPendingOrders(CancellationToken cancellationToken)
    {
        var result = await _processPendingOrdersJob.RunAsync(cancellationToken);

        return Ok(result.Adapt<ProcessPendingOrdersOutVm>());
    }
}
