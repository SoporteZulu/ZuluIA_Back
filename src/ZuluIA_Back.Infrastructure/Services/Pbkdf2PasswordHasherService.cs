using System.Security.Cryptography;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Infrastructure.Services;

public class Pbkdf2PasswordHasherService : IPasswordHasherService
{
    private const string Algorithm = "pbkdf2-sha1";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100000;

#pragma warning disable SYSLIB0041
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations);
        var hash = deriveBytes.GetBytes(KeySize);

        return $"{Algorithm}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        var parts = passwordHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || !string.Equals(parts[0], Algorithm, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!int.TryParse(parts[1], out var iterations))
            return false;

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expectedHash = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);
        var computedHash = deriveBytes.GetBytes(expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
    }
#pragma warning restore SYSLIB0041
}
