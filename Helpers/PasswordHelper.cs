using System;
using System.Security.Cryptography;

namespace FinanceCrudApp.Helpers;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        byte[] salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            10000,
            HashAlgorithmName.SHA256,
            32
        );

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');

        if (parts.Length != 2)
            return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            10000,
            HashAlgorithmName.SHA256,
            32
        );

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
