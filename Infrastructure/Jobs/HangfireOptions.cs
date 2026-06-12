namespace Infrastructure.Jobs;

public class HangfireOptions
{
    public const string SectionName = "Hangfire";

    public string DashboardPath { get; set; } = "/hangfire";
    public string CustomerSyncCron { get; set; } = "0 4 * * *";
    public string TimeZoneId { get; set; } = "Iran Standard Time";
}
