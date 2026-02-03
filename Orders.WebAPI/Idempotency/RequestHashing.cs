using System.Text;

namespace Orders.WebAPI.Idempotency;

public static class RequestHashing
{
    public static string Sha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}