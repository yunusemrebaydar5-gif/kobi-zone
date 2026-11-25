using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;

namespace Pazaryeri.Api.Helpers;

public class QuoteExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    public QuoteExpiryService(IServiceScopeFactory scopes) => _scopes = scopes;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using (var scope = _scopes.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var expired = await db.Quotes
                    .Where(q => q.Status == "pending" && q.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync(ct);
                foreach (var q in expired) q.Status = "expired";
                if (expired.Count > 0) await db.SaveChangesAsync(ct);
            }

            try { await Task.Delay(TimeSpan.FromHours(1), ct); }
            catch (TaskCanceledException) { }
        }
    }
}