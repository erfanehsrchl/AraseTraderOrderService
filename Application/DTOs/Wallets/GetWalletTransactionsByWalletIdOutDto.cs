namespace Application.DTOs.Wallets;

public class GetWalletTransactionsByWalletIdOutDto
{
    public List<WalletTransactionOutDto> Transactions { get; set; } = [];
}
