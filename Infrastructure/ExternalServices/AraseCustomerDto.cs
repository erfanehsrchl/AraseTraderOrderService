namespace Infrastructure.ExternalServices;

public class AraseCustomerDto
{
    public string NationalCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string BirthCertificationNumber { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
}
