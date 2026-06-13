using System.Net.Http.Headers;
using System.Text.Json;
using Infrastructure.ExternalServices.Models;

namespace Infrastructure.ExternalServices;

/// <summary>
/// Reads customers from the external Arase Trader API as the infrastructure adapter for Customer Synchronization.
/// </summary>
public class AraseCustomerClient : IAraseCustomerClient
{
    private readonly HttpClient _httpClient;
    private readonly IAraseAuthTokenClient _authTokenClient;

    public AraseCustomerClient(
        HttpClient httpClient,
        IAraseAuthTokenClient authTokenClient)
    {
        _httpClient = httpClient;
        _authTokenClient = authTokenClient;
    }

    /// <summary>
    /// Retrieves external customers using the cached authentication token managed by the token client.
    /// </summary>
    public async Task<IReadOnlyCollection<AraseCustomerDto>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        var token = await _authTokenClient.GetTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/customers");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize<List<AraseCustomerDto>>(document.RootElement.GetRawText()) ?? [];
        }

        if (document.RootElement.TryGetProperty("customers", out var customersElement) &&
            customersElement.ValueKind == JsonValueKind.Array)
        {
            var customersResponse = JsonSerializer.Deserialize<CustomersResponse>(document.RootElement.GetRawText());
            return customersResponse?.Customers ?? [];
        }

        return [];
    }
}
