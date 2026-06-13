using Api.Constants;
using Api.ViewModels.Wallets;
using Application.DTOs.Wallets;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Diag;

[ApiController]
[Route(DiagWalletRoutes.Base)]
/// <summary>
/// Exposes read-only diagnostic wallet endpoints backed by the application wallet query service.
/// </summary>
public class WalletDiagController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletDiagController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Returns the wallet assigned to a customer for diagnostics and cross-service verification.
    /// </summary>
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

    /// <summary>
    /// Returns wallet transaction history for diagnostics, ordered by the wallet query use case.
    /// </summary>
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
