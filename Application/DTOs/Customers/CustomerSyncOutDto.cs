namespace Application.DTOs.Customers;

public class CustomerSyncOutDto
{
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int WalletCreatedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
