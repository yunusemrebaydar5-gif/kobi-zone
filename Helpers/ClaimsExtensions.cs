   using System.Security.Claims;

   namespace Pazaryeri.Api.Helpers;

   public static class ClaimsExtensions
   {
       public static int GetUserId(this ClaimsPrincipal u) =>
           int.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);

       public static int GetCompanyId(this ClaimsPrincipal u) =>
           int.Parse(u.FindFirstValue("companyId")!);
   }