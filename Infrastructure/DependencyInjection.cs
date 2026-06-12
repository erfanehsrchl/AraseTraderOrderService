using Application.Interfaces;
using Infrastructure.ExternalServices;
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
        services.AddMemoryCache();

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

        return services;
    }
}
