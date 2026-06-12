using Api.Constants;
using Api.ViewModels.Orders;
using Application.DTOs.Orders;
using Domain.Enums;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagOrderRoutes.Base)]
public class OrdersDiagController : ControllerBase
{
    [HttpPost(DiagOrderRoutes.Add)]
    public ActionResult<AddOrderOutVm> AddOrder(AddOrderInVm request)
    {
        var input = request.Adapt<AddOrderInDto>();
        _ = input;

        var output = new AddOrderOutDto
        {
            TrackingId = Guid.Empty,
            Status = OrderStatus.Pending,
            Message = "Placeholder implementation."
        };

        return Ok(output.Adapt<AddOrderOutVm>());
    }

    [HttpGet(DiagOrderRoutes.GetByTrackingId)]
    public ActionResult<AddOrderOutVm> GetOrder(Guid trackingId)
    {
        var output = new AddOrderOutDto
        {
            TrackingId = trackingId,
            Status = OrderStatus.Pending,
            Message = "Placeholder implementation."
        };

        return Ok(output.Adapt<AddOrderOutVm>());
    }
}
