using System.Security.Cryptography;

namespace Sandbox;

class Program
{
    static void Main(string[] args)
    {
        for (int i = 1; i <= 3; i++)
        {
            var key = GenerateRandomKey();
            Console.WriteLine($"\n {key}");
        }
    }

    public static string GenerateRandomKey(int length = 32)
    {
        var key = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return Convert.ToBase64String(key);
    }
}