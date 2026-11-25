using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;
using Pazaryeri.Api.Dtos;
using Pazaryeri.Api.Entities;
using Pazaryeri.Api.Helpers;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReviewsController(AppDbContext db) => _db = db;

    // Tamamlanan sözleşmenin karşı tarafına puan ver
    [HttpPost("contract/{contractId}")]
    [Authorize]
    public async Task<IActionResult> Create(int contractId, ReviewDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 5) return BadRequest("Puan 1-5 arası olmalı.");

        var contract = await _db.Contracts.FindAsync(contractId);
        if (contract == null) return NotFound();
        if (contract.Status != "completed")
            return BadRequest("Sadece tamamlanan sözleşmeler değerlendirilebilir.");

        var quote = await _db.Quotes.FindAsync(contract.QuoteId);
        var myCompanyId = User.GetCompanyId();

        int reviewedId;
        if (myCompanyId == quote!.BuyerCompanyId) reviewedId = quote.SellerCompanyId;
        else if (myCompanyId == quote.SellerCompanyId) reviewedId = quote.BuyerCompanyId;
        else return Forbid();

        if (await _db.CompanyReviews.AnyAsync(r =>
                r.ContractId == contractId && r.ReviewerCompanyId == myCompanyId))
            return BadRequest("Bu sözleşmeyi zaten değerlendirdiniz.");

        _db.CompanyReviews.Add(new CompanyReview
        {
            ContractId = contractId,
            ReviewerCompanyId = myCompanyId,
            ReviewedCompanyId = reviewedId,
            Rating = dto.Rating,
            Comment = dto.Comment
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Bir firmanın aldığı puanlar
    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> ForCompany(int companyId)
    {
        var reviews = await _db.CompanyReviews
            .Where(r => r.ReviewedCompanyId == companyId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new { r.ReviewId, r.ReviewerCompanyId, r.Rating, r.Comment, r.CreatedAt })
            .ToListAsync();

        var average = reviews.Count == 0 ? 0 : reviews.Average(r => (double)r.Rating);
        return Ok(new { average, count = reviews.Count, reviews });
    }
}