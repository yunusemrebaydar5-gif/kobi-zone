using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;
using Pazaryeri.Api.Dtos;
using Pazaryeri.Api.Entities;
using Pazaryeri.Api.Helpers;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _db;
    public MessagesController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Send(SendMessageDto dto)
    {
        var me = User.GetUserId();
        if (dto.ReceiverId == me) return BadRequest("Kendinize mesaj gönderemezsiniz.");
        if (!await _db.Users.AnyAsync(u => u.UserId == dto.ReceiverId))
            return NotFound("Alıcı bulunamadı.");

        _db.Messages.Add(new Message
        {
            SenderId = me,
            ReceiverId = dto.ReceiverId,
            ListingId = dto.ListingId,
            QuoteId = dto.QuoteId,
            Body = dto.Body
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Bir kişiyle olan konuşma
    [HttpGet("with/{otherUserId}")]
    public async Task<IActionResult> Conversation(int otherUserId)
    {
        var me = User.GetUserId();

        var messages = await _db.Messages
            .Where(m => (m.SenderId == me && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == me))
            .OrderBy(m => m.SentAt)
            .Select(m => new { m.MessageId, m.SenderId, m.Body, m.SentAt, m.IsRead })
            .ToListAsync();

        // Bana gelenleri okundu yap
        var unread = await _db.Messages
            .Where(m => m.SenderId == otherUserId && m.ReceiverId == me && !m.IsRead)
            .ToListAsync();
        foreach (var m in unread) m.IsRead = true;
        await _db.SaveChangesAsync();

        return Ok(messages);
    }

    // Okunmamış mesaj sayısı
    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var me = User.GetUserId();
        var count = await _db.Messages.CountAsync(m => m.ReceiverId == me && !m.IsRead);
        return Ok(new { count });
    }
}