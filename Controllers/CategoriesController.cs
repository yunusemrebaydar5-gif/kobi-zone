using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;

namespace Pazaryeri.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    // Ana kategoriler ve alt türler (ParentCategoryId dolu olanlar alt tür)
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Categories
            .OrderBy(c => c.CategoryId)
            .Select(c => new { c.CategoryId, c.CategoryName, c.ParentCategoryId })
            .ToListAsync());
}