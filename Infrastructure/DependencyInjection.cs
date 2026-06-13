using Application.Interfaces;
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
using Polly;
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
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<CustomerSyncJob>();
        services.AddScoped<ProcessPendingOrdersJob>();
        services.AddHostedService<CreateOrderEventConsumer>();

        return services;
    }
}
