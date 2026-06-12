using Api.Constants;
using Api.ViewModels.Customers;
using Application.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagCustomerRoutes.Base)]
public class CustomersDiagController : ControllerBase
{
    private readonly ICustomerSyncService _customerSyncService;

    public CustomersDiagController(ICustomerSyncService customerSyncService)
    {
        _customerSyncService = customerSyncService;
    }

    [HttpPost(DiagCustomerRoutes.Sync)]
    public async Task<ActionResult<SyncCustomersOutVm>> SyncCustomers(CancellationToken cancellationToken)
    {
        var result = await _customerSyncService.SyncAsync(cancellationToken);

        return Ok(result.Adapt<SyncCustomersOutVm>());
    }
}
