using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;
using Pazaryeri.Api.Dtos;
using Pazaryeri.Api.Entities;
using Pazaryeri.Api.Helpers;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/forum")]
public class ForumController : ControllerBase
{
    private readonly AppDbContext _db;
    public ForumController(AppDbContext db) => _db = db;

    [HttpGet("categories")]
    public async Task<IActionResult> Categories() =>
        Ok(await _db.ForumCategories
            .Select(c => new { c.ForumCategoryId, c.Name, c.Description })
            .ToListAsync());

    // Konu listesi (kategoriye göre filtrelenebilir)
    [HttpGet("topics")]
    public async Task<IActionResult> Topics(int? categoryId)
    {
        var q = _db.ForumTopics.AsQueryable();
        if (categoryId != null) q = q.Where(t => t.ForumCategoryId == categoryId);

        var list = await (
            from t in q
            join u in _db.Users on t.AuthorUserId equals u.UserId
            orderby t.CreatedAt descending
            select new
            {
                t.TopicId, t.ForumCategoryId, t.Title, t.Body, t.IsClosed, t.CreatedAt,
                Author = u.FullName,
                ReplyCount = _db.ForumPosts.Count(p => p.TopicId == t.TopicId)
            }).ToListAsync();

        return Ok(list);
    }

    // Konu aç
    [HttpPost("topics")]
    [Authorize]
    public async Task<IActionResult> CreateTopic(CreateTopicDto dto)
    {
        if (!await _db.ForumCategories.AnyAsync(c => c.ForumCategoryId == dto.ForumCategoryId))
            return BadRequest("Kategori bulunamadı.");

        var topic = new ForumTopic
        {
            ForumCategoryId = dto.ForumCategoryId,
            AuthorUserId = User.GetUserId(),
            Title = dto.Title,
            Body = dto.Body
        };
        _db.ForumTopics.Add(topic);
        await _db.SaveChangesAsync();
        return Ok(new { topic.TopicId });
    }

    // Konunun cevapları
    [HttpGet("topics/{topicId}/posts")]
    public async Task<IActionResult> Posts(int topicId)
    {
        var list = await (
            from p in _db.ForumPosts
            join u in _db.Users on p.AuthorUserId equals u.UserId
            where p.TopicId == topicId
            orderby p.CreatedAt
            select new { p.PostId, p.Body, p.IsSolution, p.CreatedAt, Author = u.FullName }
        ).ToListAsync();

        return Ok(list);
    }

    // Cevap yaz
    [HttpPost("topics/{topicId}/posts")]
    [Authorize]
    public async Task<IActionResult> CreatePost(int topicId, CreatePostDto dto)
    {
        var topic = await _db.ForumTopics.FindAsync(topicId);
        if (topic == null) return NotFound();
        if (topic.IsClosed) return BadRequest("Bu konu kapatılmış.");

        var post = new ForumPost
        {
            TopicId = topicId,
            AuthorUserId = User.GetUserId(),
            Body = dto.Body
        };
        _db.ForumPosts.Add(post);
        await _db.SaveChangesAsync();
        return Ok(new { post.PostId });
    }
}