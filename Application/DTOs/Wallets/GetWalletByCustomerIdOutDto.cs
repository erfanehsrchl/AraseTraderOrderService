namespace Application.DTOs.Wallets;

public class GetWalletByCustomerIdOutDto
{
    public long WalletId { get; set; }
    public long CustomerId { get; set; }
    public decimal Balance { get; set; }
}
