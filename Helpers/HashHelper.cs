using System.Security.Cryptography;
using System.Text;

namespace Pazaryeri.Api.Helpers;

public static class HashHelper
{
    public static string Sha256(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}