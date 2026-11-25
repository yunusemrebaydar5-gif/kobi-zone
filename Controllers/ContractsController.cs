using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;
using Pazaryeri.Api.Entities;
using Pazaryeri.Api.Helpers;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ContractsController(AppDbContext db) => _db = db;

    // Firmamın sözleşmeleri
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var myCompanyId = User.GetCompanyId();

        var list = await (
            from c in _db.Contracts
            join q in _db.Quotes on c.QuoteId equals q.QuoteId
            where q.BuyerCompanyId == myCompanyId || q.SellerCompanyId == myCompanyId
            orderby c.CreatedAt descending
            select new
            {
                c.ContractId, c.ContractNo, c.Status, c.TotalAmount,
                c.StartDate, c.EndDate, c.TermsText,
                q.BuyerCompanyId, q.SellerCompanyId
            }).ToListAsync();

        return Ok(list);
    }

    // Teklifi kabul et: sözleşme oluşur
    [HttpPost("accept-quote/{quoteId}")]
    public async Task<IActionResult> AcceptQuote(int quoteId)
    {
        var quote = await _db.Quotes.FindAsync(quoteId);
        if (quote == null) return NotFound();
        if (quote.Status != "pending")
            return BadRequest("Bu teklif artık cevaplanamaz.");

        var myCompanyId = User.GetCompanyId();
        if (myCompanyId != quote.BuyerCompanyId && myCompanyId != quote.SellerCompanyId)
            return Forbid();

        var creatorCompanyId = await _db.Users
            .Where(u => u.UserId == quote.CreatedByUserId)
            .Select(u => u.CompanyId)
            .FirstAsync();
        if (creatorCompanyId == myCompanyId)
            return BadRequest("Kendi teklifinizi kabul edemezsiniz.");

        var listing = await _db.Listings.FindAsync(quote.ListingId);

        var terms =
            $"İlan: {listing!.Title}\n" +
            $"Alıcı firma no: {quote.BuyerCompanyId}, Satıcı firma no: {quote.SellerCompanyId}\n" +
            $"Miktar: {quote.Quantity}, Birim fiyat: {quote.ProposedPrice} {listing.PriceUnit}\n" +
            $"Tarih: {quote.StartDate} - {quote.EndDate}\n" +
            $"Takas: {quote.SwapOffer}";

        quote.Status = "accepted";

        var contract = new Contract
        {
            QuoteId = quote.QuoteId,
            ContractNo = $"SZL-{DateTime.UtcNow:yyyyMMdd}-{quote.QuoteId}",
            TermsText = terms,
            TotalAmount = quote.ProposedPrice * quote.Quantity,
            StartDate = quote.StartDate,
            EndDate = quote.EndDate,
            ContentHash = HashHelper.Sha256(terms),
            Status = "awaiting_signature"
        };
        _db.Contracts.Add(contract);

        await _db.SaveChangesAsync();
        return Ok(new { contract.ContractId, contract.ContractNo });
    }

    // Sözleşmeyi imzala
    [HttpPost("{id}/sign")]
    [Authorize(Roles = "company_admin")]
    public async Task<IActionResult> Sign(int id)
    {
        var contract = await _db.Contracts.FindAsync(id);
        if (contract == null) return NotFound();
        if (contract.Status != "awaiting_signature")
            return BadRequest("Bu sözleşme imzaya açık değil.");

        var quote = await _db.Quotes.FindAsync(contract.QuoteId);
        var myCompanyId = User.GetCompanyId();
        if (myCompanyId != quote!.BuyerCompanyId && myCompanyId != quote.SellerCompanyId)
            return Forbid();

        if (HashHelper.Sha256(contract.TermsText) != contract.ContentHash)
            return Conflict("Sözleşme metni değiştirilmiş, imzalanamaz.");

        if (await _db.ContractSignatures.AnyAsync(s =>
                s.ContractId == id && s.CompanyId == myCompanyId))
            return BadRequest("Firmanız zaten imzaladı.");

        // Diğer taraf daha önce imzaladıysa sözleşme yürürlüğe girer
        var otherSigned = await _db.ContractSignatures.AnyAsync(s => s.ContractId == id);

        var userId = User.GetUserId();
        var now = DateTime.UtcNow;

        _db.ContractSignatures.Add(new ContractSignature
        {
            ContractId = id,
            CompanyId = myCompanyId,
            SignedByUserId = userId,
            SignedAt = now,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            SignatureHash = HashHelper.Sha256($"{contract.ContentHash}|{myCompanyId}|{userId}|{now:O}")
        });

        if (otherSigned) contract.Status = "active";

        await _db.SaveChangesAsync();
        return Ok(new { contract.Status });
    }

    // İş bitince tamamlandı işaretle
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var contract = await _db.Contracts.FindAsync(id);
        if (contract == null) return NotFound();
        if (contract.Status != "active") return BadRequest("Sözleşme aktif değil.");

        var quote = await _db.Quotes.FindAsync(contract.QuoteId);
        var myCompanyId = User.GetCompanyId();
        if (myCompanyId != quote!.BuyerCompanyId && myCompanyId != quote.SellerCompanyId)
            return Forbid();

        contract.Status = "completed";
        await _db.SaveChangesAsync();
        return Ok();
    }
}