using Api.Constants;
using Api.ViewModels.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagJobRoutes.Base)]
public class JobsDiagController : ControllerBase
{
    [HttpPost(DiagJobRoutes.ProcessPendingOrders)]
    public ActionResult<ProcessPendingOrdersOutVm> ProcessPendingOrders()
    {
        return Ok(new ProcessPendingOrdersOutVm
        {
            Message = "Placeholder implementation."
        });
    }
}
