using Hangfire;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Jobs;

public static class HangfireJobsRegistration
{
    public const string CustomerSyncDailyJobId = "customer-sync-daily";
    public const string ProcessPendingOrdersJobId = "process-pending-orders";

    public static void RegisterRecurringJobs(
        this IRecurringJobManager recurringJobManager,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>()
            ?? new HangfireOptions();

        recurringJobManager.AddOrUpdate<CustomerSyncJob>(
            CustomerSyncDailyJobId,
            job => job.RunAsync(CancellationToken.None),
            options.CustomerSyncCron,
            new RecurringJobOptions
            {
                TimeZone = ResolveTimeZone(options.TimeZoneId)
            });

        recurringJobManager.AddOrUpdate<ProcessPendingOrdersJob>(
            ProcessPendingOrdersJobId,
            job => job.RunAsync(CancellationToken.None),
            options.ProcessPendingOrdersCron,
            new RecurringJobOptions
            {
                TimeZone = ResolveTimeZone(options.TimeZoneId)
            });
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Local;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Local;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Local;
        }
    }
}
