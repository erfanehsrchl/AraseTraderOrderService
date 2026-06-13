using Api.Constants;
using Api.ViewModels.Jobs;
using Infrastructure.Jobs;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagJobRoutes.Base)]
/// <summary>
/// Provides diagnostic entry points for manually triggering background order processing jobs.
/// </summary>
public class JobsDiagController : ControllerBase
{
    private readonly ProcessPendingOrdersJob _processPendingOrdersJob;

    public JobsDiagController(ProcessPendingOrdersJob processPendingOrdersJob)
    {
        _processPendingOrdersJob = processPendingOrdersJob;
    }

    /// <summary>
    /// Executes pending order wallet processing on demand so operational users can verify the scheduled job behavior.
    /// </summary>
    [HttpPost(DiagJobRoutes.ProcessPendingOrders)]
    public async Task<ActionResult<ProcessPendingOrdersOutVm>> ProcessPendingOrders(CancellationToken cancellationToken)
    {
        var result = await _processPendingOrdersJob.RunAsync(cancellationToken);

        return Ok(result.Adapt<ProcessPendingOrdersOutVm>());
    }
}
