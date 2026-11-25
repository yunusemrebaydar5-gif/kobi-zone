using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;
using Pazaryeri.Api.Dtos;
using Pazaryeri.Api.Entities;
using Pazaryeri.Api.Helpers;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/listings")]
[Authorize]
public class ListingsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ListingsController(AppDbContext db) => _db = db;

    // Pazar: yayındaki ilanlar (firma adı ve şehirle birlikte)
    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? type, int? categoryId, string? city, string? search,
        int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 200);

        var q = _db.Listings.Where(l => l.IsActive);

        if (type != null) q = q.Where(l => l.ListingType == type);
        if (categoryId != null) q = q.Where(l => l.CategoryId == categoryId);
        if (city != null) q = q.Where(l => l.City == city);
        if (search != null) q = q.Where(l => l.Title.Contains(search));

        var total = await q.CountAsync();

        var items = await (
            from l in q
            join c in _db.Companies on l.CompanyId equals c.CompanyId
            orderby l.CreatedAt descending
            select new
            {
                l.ListingId, l.Title, l.About, l.ListingType, l.Price, l.PriceUnit,
                l.Quantity, l.Condition, l.SwapWanted, l.IsActive, l.CategoryId,
                l.CompanyId, c.CompanyName,
                City = l.City ?? c.City
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    // İlanlarım: firmamın bütün ilanları (yayında olmayanlar dahil)
    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var companyId = User.GetCompanyId();

        var items = await (
            from l in _db.Listings
            join c in _db.Companies on l.CompanyId equals c.CompanyId
            where l.CompanyId == companyId
            orderby l.CreatedAt descending
            select new
            {
                l.ListingId, l.Title, l.About, l.ListingType, l.Price, l.PriceUnit,
                l.Quantity, l.Condition, l.SwapWanted, l.IsActive, l.CategoryId,
                l.CompanyId, c.CompanyName,
                City = l.City ?? c.City
            })
            .ToListAsync();

        return Ok(items);
    }

    // İlan ekleme: sadece onaylı firma
    [HttpPost]
    public async Task<IActionResult> Create(CreateListingDto dto)
    {
        var companyId = User.GetCompanyId();
        if (!await CompanyGuard.IsApprovedAsync(_db, companyId))
            return StatusCode(403, "Firmanız henüz onaylanmadı.");

        if (!await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
            return BadRequest("Kategori bulunamadı.");

        var isLabor = dto.ListingType.StartsWith("labor");
        if (isLabor && dto.Labor == null)
            return BadRequest("Personel ilanında personel bilgisi zorunlu.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var listing = new Listing
        {
            CompanyId = companyId,
            CategoryId = dto.CategoryId,
            ListingType = dto.ListingType,
            Title = dto.Title,
            About = dto.About,
            Price = dto.Price,
            PriceUnit = dto.PriceUnit,
            Quantity = dto.Quantity,
            Condition = dto.Condition,
            SwapWanted = dto.SwapWanted,
            AvailableFrom = dto.AvailableFrom,
            AvailableTo = dto.AvailableTo,
            City = dto.City,
            IsActive = true
        };
        _db.Listings.Add(listing);
        await _db.SaveChangesAsync();

        if (isLabor)
        {
            _db.LaborDetails.Add(new LaborDetail
            {
                ListingId = listing.ListingId,
                SkillTitle = dto.Labor!.SkillTitle,
                HeadCount = dto.Labor.HeadCount,
                ExperienceYears = dto.Labor.ExperienceYears,
                Certifications = dto.Labor.Certifications
            });
            await _db.SaveChangesAsync();
        }

        await tx.CommitAsync();
        return Ok(new { listing.ListingId });
    }

    // Yayına al / yayından kaldır
    [HttpPut("{id}/active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing == null) return NotFound();
        if (listing.CompanyId != User.GetCompanyId()) return Forbid();

        listing.IsActive = !listing.IsActive;
        await _db.SaveChangesAsync();
        return Ok(new { listing.IsActive });
    }

    // İlanı sil: teklif ya da mesaj bağlıysa silinmez, yayından kaldırılabilir
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing == null) return NotFound();
        if (listing.CompanyId != User.GetCompanyId()) return Forbid();

        if (await _db.Quotes.AnyAsync(q => q.ListingId == id) ||
            await _db.Messages.AnyAsync(m => m.ListingId == id))
            return BadRequest("Bu ilana teklif ya da mesaj var, silinemez. Yayından kaldırabilirsiniz.");

        var favorites = await _db.Favorites.Where(f => f.ListingId == id).ToListAsync();
        _db.Favorites.RemoveRange(favorites);
        _db.Listings.Remove(listing);
        await _db.SaveChangesAsync();
        return Ok();
    }
}