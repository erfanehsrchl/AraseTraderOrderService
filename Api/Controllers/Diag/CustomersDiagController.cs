using Api.Constants;
using Api.ViewModels.Customers;
using Application.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagCustomerRoutes.Base)]
/// <summary>
/// Exposes diagnostic operations for customer synchronization from the external Arase Trader service.
/// </summary>
public class CustomersDiagController : ControllerBase
{
    private readonly ICustomerSyncService _customerSyncService;

    public CustomersDiagController(ICustomerSyncService customerSyncService)
    {
        _customerSyncService = customerSyncService;
    }

    /// <summary>
    /// Runs the customer synchronization use case and reports how many customers and wallets were created or updated.
    /// </summary>
    [HttpPost(DiagCustomerRoutes.Sync)]
    public async Task<ActionResult<SyncCustomersOutVm>> SyncCustomers(CancellationToken cancellationToken)
    {
        var result = await _customerSyncService.SyncAsync(cancellationToken);

        return Ok(result.Adapt<SyncCustomersOutVm>());
    }
}
