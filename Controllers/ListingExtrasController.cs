using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;
using Pazaryeri.Api.Entities;
using Pazaryeri.Api.Helpers;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/listings")]
[Authorize]
public class ListingExtrasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ListingExtrasController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // İlana resim yükle: sadece ilan sahibi
    [HttpPost("{id}/images")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file, [FromForm] bool isMain = false)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing == null) return NotFound();
        if (listing.CompanyId != User.GetCompanyId()) return Forbid();

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
            return BadRequest("Sadece JPG veya PNG.");

        var folder = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid()}{ext}";
        await using (var stream = System.IO.File.Create(Path.Combine(folder, fileName)))
            await file.CopyToAsync(stream);

        // Yeni ana resim geldiyse eskisini ana olmaktan çıkar
        if (isMain)
        {
            var oldMains = await _db.ListingImages
                .Where(i => i.ListingId == id && i.IsMain).ToListAsync();
            foreach (var img in oldMains) img.IsMain = false;
        }

        _db.ListingImages.Add(new ListingImage
        {
            ListingId = id,
            ImageUrl = $"/uploads/{fileName}",
            IsMain = isMain
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    // İlanın resimleri
    [HttpGet("{id}/images")]
    [AllowAnonymous]
    public async Task<IActionResult> Images(int id) =>
        Ok(await _db.ListingImages
            .Where(i => i.ListingId == id)
            .Select(i => new { i.ImageId, i.ImageUrl, i.IsMain })
            .ToListAsync());

    // Favoriye ekle
    [HttpPost("{id}/favorite")]
    public async Task<IActionResult> AddFavorite(int id)
    {
        var userId = User.GetUserId();
        if (!await _db.Listings.AnyAsync(l => l.ListingId == id)) return NotFound();

        if (!await _db.Favorites.AnyAsync(f => f.UserId == userId && f.ListingId == id))
        {
            _db.Favorites.Add(new Favorite { UserId = userId, ListingId = id });
            await _db.SaveChangesAsync();
        }
        return Ok();
    }

    // Favoriden çıkar
    [HttpDelete("{id}/favorite")]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        var userId = User.GetUserId();
        var fav = await _db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == id);
        if (fav != null)
        {
            _db.Favorites.Remove(fav);
            await _db.SaveChangesAsync();
        }
        return Ok();
    }

    // Favorilerim
    [HttpGet("favorites")]
    public async Task<IActionResult> MyFavorites()
    {
        var userId = User.GetUserId();
        var list = await (
            from f in _db.Favorites
            join l in _db.Listings on f.ListingId equals l.ListingId
            where f.UserId == userId
            orderby f.AddedAt descending
            select new { l.ListingId, l.Title, l.ListingType, l.Price, l.PriceUnit, l.City }
        ).ToListAsync();
        return Ok(list);
    }
}