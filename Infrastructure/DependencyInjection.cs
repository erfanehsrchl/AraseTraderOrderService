using Application.Interfaces;
using System.Net;
using Infrastructure.Caching;
using Infrastructure.ExternalServices;
using Infrastructure.ExternalServices.Models;
using Infrastructure.Jobs;
using Infrastructure.Mappings;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(ServiceMappingConfig).Assembly);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<AraseExternalApiOptions>(
            configuration.GetSection(AraseExternalApiOptions.SectionName));
        services.Configure<HangfireOptions>(
            configuration.GetSection(HangfireOptions.SectionName));
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

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
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton(serviceProvider =>
            RedLockFactory.Create(new[]
            {
                new RedLockMultiplexer(serviceProvider.GetRequiredService<IConnectionMultiplexer>())
            }));
        services.AddScoped<IDistributedLockService, RedLockDistributedLockService>();

        services.AddAraseExternalApiHttpClient<IAraseAuthTokenClient, AraseAuthTokenClient>();
        services.AddAraseExternalApiHttpClient<IAraseCustomerClient, AraseCustomerClient>();

        services.AddScoped<ICustomerSyncService, CustomerSyncService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<CustomerSyncJob>();
        services.AddScoped<ProcessPendingOrdersJob>();
        services.AddHostedService<CreateOrderEventConsumer>();

        return services;
    }

    private static IHttpClientBuilder AddAraseExternalApiHttpClient<TClient, TImplementation>(
        this IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services.AddHttpClient<TClient, TImplementation>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<AraseExternalApiOptions>>()
                    .Value;

                httpClient.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddPolicyHandler((serviceProvider, _) => CreateRetryPolicy(serviceProvider));
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider
            .GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
            .CreateLogger("AraseExternalApiResilience");

        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response => IsTransientStatusCode(response.StatusCode))
            .WaitAndRetryAsync(
                retryCount: 3,
                (retryAttempt, outcome, _) => GetRetryDelay(retryAttempt, outcome),
                (outcome, delay, retryAttempt, _) =>
                {
                    var statusCode = outcome.Result?.StatusCode;
                    logger.LogWarning(
                        outcome.Exception,
                        "Retrying Arase external API request. Attempt: {RetryAttempt}, Delay: {Delay}, StatusCode: {StatusCode}",
                        retryAttempt,
                        delay,
                        statusCode);

                    return Task.CompletedTask;
                });
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests ||
               (int)statusCode >= 500;
    }

    private static TimeSpan GetRetryDelay(
        int retryAttempt,
        DelegateResult<HttpResponseMessage> outcome)
    {
        if (outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = outcome.Result.Headers.RetryAfter;
            if (retryAfter?.Delta is not null)
            {
                return retryAfter.Delta.Value;
            }

            if (retryAfter?.Date is not null)
            {
                var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    return delay;
                }
            }
        }

        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
    }
}
