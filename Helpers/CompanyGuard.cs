using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Data;

namespace Pazaryeri.Api.Helpers;

public static class CompanyGuard
{
    public static Task<bool> IsApprovedAsync(AppDbContext db, int companyId) =>
        db.Companies.AnyAsync(c =>
            c.CompanyId == companyId && c.VerificationStatus == "approved");
}