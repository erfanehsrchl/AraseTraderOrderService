namespace Infrastructure.Jobs;

/// <summary>
/// Provides scheduler settings for Hangfire jobs that run customer synchronization and order processing.
/// </summary>
public class HangfireOptions
{
    public const string SectionName = "Hangfire";

    public string DashboardPath { get; set; } = "/hangfire";
    public string CustomerSyncCron { get; set; } = "0 4 * * *";
    public string ProcessPendingOrdersCron { get; set; } = "*/1 * * * *";
    public string TimeZoneId { get; set; } = "Iran Standard Time";
    public int WalletLockExpirySeconds { get; set; } = 30;
}
