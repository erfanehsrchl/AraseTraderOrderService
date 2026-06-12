using Api.Constants;
using Api.ViewModels.Orders;
using Application.DTOs.Orders;
using Application.Interfaces;
using Domain.Enums;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagOrderRoutes.Base)]
public class OrdersDiagController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersDiagController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost(DiagOrderRoutes.Add)]
    public async Task<ActionResult<AddOrderOutVm>> AddOrder(
        AddOrderInVm request,
        CancellationToken cancellationToken)
    {
        var input = request.Adapt<AddOrderInDto>();
        var output = await _orderService.AddOrderAsync(input, cancellationToken);

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
