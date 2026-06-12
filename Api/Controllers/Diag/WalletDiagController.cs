using Api.Constants;
using Api.ViewModels.Wallets;
using Application.DTOs.Wallets;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagWalletRoutes.Base)]
public class WalletDiagController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletDiagController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet(DiagWalletRoutes.GetByCustomerId)]
    public async Task<ActionResult<GetWalletByCustomerIdOutDto>> GetByCustomerId(
        [FromRoute] GetWalletByCustomerIdInVm request,
        CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await _walletService.GetWalletByCustomerIdAsync(request.CustomerId, cancellationToken);
            return Ok(wallet);
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpGet(DiagWalletRoutes.GetTransactionsByWalletId)]
    public async Task<ActionResult<GetWalletTransactionsByWalletIdOutDto>> GetTransactionsByWalletId(
        [FromRoute] GetWalletTransactionsByWalletIdInVm request,
        CancellationToken cancellationToken)
    {
        try
        {
            var transactions = await _walletService.GetWalletTransactionsByWalletIdAsync(request.WalletId, cancellationToken);
            return Ok(transactions);
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(exception.Message);
        }
    }
}
