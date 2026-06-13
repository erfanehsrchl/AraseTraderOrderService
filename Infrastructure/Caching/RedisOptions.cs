namespace Infrastructure.Caching;

/// <summary>
/// Provides Redis connection settings used by distributed cache and distributed locking infrastructure.
/// </summary>
public class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = string.Empty;
}
