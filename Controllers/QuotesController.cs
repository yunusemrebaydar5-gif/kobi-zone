using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;
using Pazaryeri.Api.Dtos;
using Pazaryeri.Api.Entities;
using Pazaryeri.Api.Helpers;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/quotes")]
[Authorize]
public class QuotesController : ControllerBase
{
    private readonly AppDbContext _db;
    public QuotesController(AppDbContext db) => _db = db;

    // Firmamın verdiği ve aldığı teklifler; varsa sözleşmesiyle birlikte
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var myCompanyId = User.GetCompanyId();

        var quotes = await (
            from q in _db.Quotes
            join l in _db.Listings on q.ListingId equals l.ListingId
            join b in _db.Companies on q.BuyerCompanyId equals b.CompanyId
            join s in _db.Companies on q.SellerCompanyId equals s.CompanyId
            join u in _db.Users on q.CreatedByUserId equals u.UserId
            where q.BuyerCompanyId == myCompanyId || q.SellerCompanyId == myCompanyId
            orderby q.CreatedAt descending
            select new
            {
                q.QuoteId, q.ListingId, ListingTitle = l.Title, l.PriceUnit,
                q.BuyerCompanyId, BuyerName = b.CompanyName,
                q.SellerCompanyId, SellerName = s.CompanyName,
                q.ParentQuoteId, q.ProposedPrice, q.Quantity, q.SwapOffer, q.Note,
                q.Status, q.CreatedAt,
                CreatedByMe = u.CompanyId == myCompanyId
            }).ToListAsync();

        var quoteIds = quotes.Select(x => x.QuoteId).ToList();

        var contracts = await _db.Contracts
            .Where(c => quoteIds.Contains(c.QuoteId))
            .Select(c => new
            {
                c.QuoteId, c.ContractId, c.ContractNo, c.Status, c.TermsText, c.TotalAmount,
                SignedByMe = _db.ContractSignatures.Any(x =>
                    x.ContractId == c.ContractId && x.CompanyId == myCompanyId),
                ReviewedByMe = _db.CompanyReviews.Any(r =>
                    r.ContractId == c.ContractId && r.ReviewerCompanyId == myCompanyId)
            })
            .ToListAsync();

        var result = quotes.Select(x => new
        {
            Quote = x,
            Contract = contracts.FirstOrDefault(c => c.QuoteId == x.QuoteId)
        });

        return Ok(result);
    }

    // İlk teklif
    [HttpPost]
    public async Task<IActionResult> Create(CreateQuoteDto dto)
    {
        var myCompanyId = User.GetCompanyId();
        if (!await CompanyGuard.IsApprovedAsync(_db, myCompanyId))
            return StatusCode(403, "Firmanız henüz onaylanmadı.");

        var listing = await _db.Listings.FindAsync(dto.ListingId);
        if (listing == null || !listing.IsActive) return NotFound("İlan bulunamadı.");
        if (listing.CompanyId == myCompanyId)
            return BadRequest("Kendi ilanınıza teklif veremezsiniz.");

        var quote = new Quote
        {
            ListingId = listing.ListingId,
            BuyerCompanyId = myCompanyId,
            SellerCompanyId = listing.CompanyId,
            CreatedByUserId = User.GetUserId(),
            ProposedPrice = dto.ProposedPrice,
            Quantity = dto.Quantity,
            SwapOffer = dto.SwapOffer,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Note = dto.Note,
            Status = "pending",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync();
        return Ok(new { quote.QuoteId });
    }

    // Karşı teklif
    [HttpPost("{id}/counter")]
    public async Task<IActionResult> Counter(int id, CounterDto dto)
    {
        var (quote, error) = await GetAnswerableQuote(id);
        if (error != null) return error;

        quote!.Status = "countered";

        var counter = new Quote
        {
            ListingId = quote.ListingId,
            BuyerCompanyId = quote.BuyerCompanyId,
            SellerCompanyId = quote.SellerCompanyId,
            CreatedByUserId = User.GetUserId(),
            ParentQuoteId = quote.QuoteId,
            ProposedPrice = dto.ProposedPrice,
            Quantity = dto.Quantity,
            SwapOffer = dto.SwapOffer,
            StartDate = quote.StartDate,
            EndDate = quote.EndDate,
            Note = dto.Note,
            Status = "pending",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _db.Quotes.Add(counter);
        await _db.SaveChangesAsync();
        return Ok(new { counter.QuoteId });
    }

    // Reddet
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var (quote, error) = await GetAnswerableQuote(id);
        if (error != null) return error;

        quote!.Status = "rejected";
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Ortak kontrol: teklif var mı, bekliyor mu, cevaplayan karşı taraf mı?
    private async Task<(Quote?, IActionResult?)> GetAnswerableQuote(int id)
    {
        var quote = await _db.Quotes.FindAsync(id);
        if (quote == null) return (null, NotFound());
        if (quote.Status != "pending")
            return (null, BadRequest("Bu teklif artık cevaplanamaz."));

        var myCompanyId = User.GetCompanyId();
        if (myCompanyId != quote.BuyerCompanyId && myCompanyId != quote.SellerCompanyId)
            return (null, Forbid());

        var creatorCompanyId = await _db.Users
            .Where(u => u.UserId == quote.CreatedByUserId)
            .Select(u => u.CompanyId)
            .FirstAsync();
        if (creatorCompanyId == myCompanyId)
            return (null, BadRequest("Kendi teklifinizi cevaplayamazsınız."));

        return (quote, null);
    }
}