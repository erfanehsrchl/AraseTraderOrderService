using Api.Constants;
using Api.ViewModels.Orders;
using Application.DTOs.Orders;
using Application.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagOrderRoutes.Base)]
/// <summary>
/// Exposes diagnostic order endpoints that exercise the same application services used by messaging and gRPC boundaries.
/// </summary>
public class OrdersDiagController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersDiagController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Registers an order as pending wallet processing using the internal order application use case.
    /// </summary>
    [HttpPost(DiagOrderRoutes.Add)]
    public async Task<ActionResult<AddOrderOutVm>> AddOrder(
        AddOrderInVm request,
        CancellationToken cancellationToken)
    {
        var input = request.Adapt<AddOrderInDto>();
        var output = await _orderService.AddOrderAsync(input, cancellationToken);

        return Ok(output.Adapt<AddOrderOutVm>());
    }

    /// <summary>
    /// Retrieves an order by its gateway-generated tracking identifier for diagnostics and support workflows.
    /// </summary>
    [HttpGet(DiagOrderRoutes.GetByTrackingId)]
    public async Task<ActionResult<GetOrderByTrackingIdOutVm>> GetOrder(
        Guid trackingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var output = await _orderService.GetOrderByTrackingIdAsync(
                trackingId,
                cancellationToken);

            return Ok(output.Adapt<GetOrderByTrackingIdOutVm>());
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(exception.Message);
        }
    }
}
