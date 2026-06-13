using System.Text.Json.Serialization;

namespace Infrastructure.ExternalServices.Models;

public class AraseCustomerDto
{
    [JsonPropertyName("nationalCode")]
    public string NationalCode { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("fatherName")]
    public string FatherName { get; set; } = string.Empty;

    [JsonPropertyName("birthCertificationNumber")]
    public string BirthCertificationNumber { get; set; } = string.Empty;

    [JsonPropertyName("registerationNumber")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [JsonPropertyName("birthDate")]
    public DateTime BirthDate { get; set; }

    [JsonPropertyName("branchName")]
    public string BranchName { get; set; } = string.Empty;

    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;
}
