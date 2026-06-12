using Application.Interfaces;
using Infrastructure.Caching;
using Infrastructure.ExternalServices;
using Infrastructure.Jobs;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<AraseExternalApiOptions>(
            configuration.GetSection(AraseExternalApiOptions.SectionName));

        var redisConnectionString = configuration.GetSection(RedisOptions.SectionName)
            .Get<RedisOptions>()?
            .ConnectionString;

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string was not found.");
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        services.AddHttpClient<IAraseAuthTokenClient, AraseAuthTokenClient>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<AraseExternalApiOptions>>()
                    .Value;

                httpClient.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddHttpClient<IAraseCustomerClient, AraseCustomerClient>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<AraseExternalApiOptions>>()
                    .Value;

                httpClient.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddScoped<ICustomerSyncService, CustomerSyncService>();
        services.AddScoped<CustomerSyncJob>();

        return services;
    }
}
