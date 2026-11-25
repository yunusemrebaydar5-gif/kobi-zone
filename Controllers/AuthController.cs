using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pazaryeri.Api.Data;
using Pazaryeri.Api.Dtos;
using Pazaryeri.Api.Entities;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public AuthController(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("Bu e-posta zaten kayıtlı.");
        if (await _db.Companies.AnyAsync(c => c.TaxNumber == dto.TaxNumber))
            return Conflict("Bu vergi numarası zaten kayıtlı.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var company = new Company
        {
            CompanyName = dto.CompanyName,
            TaxNumber = dto.TaxNumber,
            TaxOffice = dto.TaxOffice,
            City = dto.City
        };
        _db.Companies.Add(company);
        await _db.SaveChangesAsync();

        var user = new User
        {
            CompanyId = company.CompanyId,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
             Role = "company_admin",
            IsActive = true
        };
    
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await tx.CommitAsync();
        return Ok(new { token = CreateToken(user) });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !user.IsActive ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("E-posta veya şifre hatalı.");

        return Ok(new { token = CreateToken(user) });
    }

    private string CreateToken(User u)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, u.UserId.ToString()),
            new Claim("companyId", (u.CompanyId ?? 0).ToString()),
            new Claim(ClaimTypes.Role, u.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}