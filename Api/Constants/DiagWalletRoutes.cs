namespace Api.Constants;

public static class DiagWalletRoutes
{
    public const string Base = "diag/wallets";
    public const string GetByCustomerId = "by-customer/{CustomerId:long}";
    public const string GetTransactionsByWalletId = "{WalletId:long}/transactions";
}
