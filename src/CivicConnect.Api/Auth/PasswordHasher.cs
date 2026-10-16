using System.Security.Cryptography;

namespace CivicConnect.Api.Auth;

/// <summary>
/// PBKDF2-SHA256 password hashing (NFR-002), built into .NET so no extra package is needed.
/// Stored format: "PBKDF2-SHA256$iterations$salt$hash" (salt and hash are Base64).
/// </summary>
public static class PasswordHasher
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int Iterations = 100_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    // Used when the user does not exist, so a failed login takes about the same time either way.
    private static readonly string DummyHash = Hash("not-a-real-password");

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);
        return $"{Algorithm}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>Returns true only when <paramref name="storedHash"/> exists and matches the password.</summary>
    public static bool Verify(string? password, string? storedHash)
    {
        var userExists = storedHash is not null;
        var parts = (storedHash ?? DummyHash).Split('$');
        if (parts.Length != 4 || parts[0] != Algorithm || !int.TryParse(parts[1], out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password ?? "", salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        // Constant-time comparison avoids leaking how many bytes matched.
        return CryptographicOperations.FixedTimeEquals(actual, expected) && userExists;
    }
}
