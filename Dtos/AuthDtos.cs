namespace Pazaryeri.Api.Dtos;

public record RegisterDto(
    string CompanyName, string TaxNumber, string TaxOffice, string City,
    string FullName, string Email, string Password, string Phone);

public record LoginDto(string Email, string Password);