using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;
using Pazaryeri.Api.Dtos;
using Pazaryeri.Api.Entities;
using Pazaryeri.Api.Helpers;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public CompaniesController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // Kendi firmamın bilgisi
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = User.GetCompanyId();
        var company = await _db.Companies
            .Where(c => c.CompanyId == id)
            .Select(c => new
            {
                c.CompanyId, c.CompanyName, c.TaxNumber, c.City, c.OrganizedZone,
                c.Sector, c.VerificationStatus, c.RejectReason
            })
            .FirstOrDefaultAsync();
        return company == null ? NotFound() : Ok(company);
    }

    // Doğrulama belgesi yükle
    [HttpPost("documents")]
    public async Task<IActionResult> UploadDocument([FromForm] string docType, IFormFile file)
    {
        var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext)) return BadRequest("Sadece PDF veya resim yüklenebilir.");

        var folder = Path.Combine(_env.ContentRootPath, "PrivateFiles", "docs");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid()}{ext}";
        await using (var stream = System.IO.File.Create(Path.Combine(folder, fileName)))
            await file.CopyToAsync(stream);

        _db.CompanyDocuments.Add(new CompanyDocument
        {
            CompanyId = User.GetCompanyId(),
            DocType = docType,
            FileUrl = fileName
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Admin: onay bekleyen firmalar
    [HttpGet("pending")]
    [Authorize(Roles = "platform_admin")]
    public async Task<IActionResult> Pending() =>
        Ok(await _db.Companies
            .Where(c => c.VerificationStatus == "pending")
            .Select(c => new { c.CompanyId, c.CompanyName, c.TaxNumber, c.City, c.CreatedAt })
            .ToListAsync());

    // Admin: onayla veya reddet
    [HttpPost("{id}/verify")]
    [Authorize(Roles = "platform_admin")]
    public async Task<IActionResult> Verify(int id, VerifyDto dto)
    {
        var company = await _db.Companies.FindAsync(id);
        if (company == null) return NotFound();

        company.VerificationStatus = dto.Approve ? "approved" : "rejected";
        company.RejectReason = dto.Approve ? null : dto.RejectReason;
        company.VerifiedAt = DateTime.UtcNow;
        company.VerifiedByUserId = User.GetUserId();

        await _db.SaveChangesAsync();
        return Ok();
    }
}