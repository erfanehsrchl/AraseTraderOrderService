using Api.Constants;
using Api.ViewModels.Customers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagCustomerRoutes.Base)]
public class CustomersDiagController : ControllerBase
{
    [HttpPost(DiagCustomerRoutes.Sync)]
    public ActionResult<SyncCustomersOutVm> SyncCustomers()
    {
        return Ok(new SyncCustomersOutVm
        {
            Message = "Placeholder implementation."
        });
    }
}
