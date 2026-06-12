using System.Text.Json.Serialization;

namespace Infrastructure.ExternalServices.Models;

public class CustomersResponse
{
    [JsonPropertyName("customers")]
    public List<AraseCustomerDto> Customers { get; set; } = [];
}
