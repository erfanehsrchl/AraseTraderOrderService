namespace Api.Constants;

public static class DiagWalletRoutes
{
    public const string Base = "diag/wallets";
    public const string GetByCustomerId = "by-customer/{customerId:long}";
    public const string GetTransactionsByWalletId = "{walletId:long}/transactions";
}
