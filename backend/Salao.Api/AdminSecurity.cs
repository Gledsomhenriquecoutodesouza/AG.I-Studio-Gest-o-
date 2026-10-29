using System.Security.Cryptography;
using System.Text;

namespace Salao.Api;

public static class AdminCodeHash
{
    private const int Iterations = 150_000;

    public static string Create(string code)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(code, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string code, string stored)
    {
        try
        {
            var parts = stored.Split('.', 2);
            if (parts.Length != 2) return false;
            var salt = Convert.FromBase64String(parts[0]);
            var expected = Convert.FromBase64String(parts[1]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(code, salt, Iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}

public static class AdminSessionTokens
{
    public static string Create(AdminCredential credential)
    {
        var payload = Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds().ToString());
        var key = Convert.FromBase64String(credential.SessionKey);
        var signature = HMACSHA256.HashData(key, payload);
        return $"{ToBase64Url(payload)}.{ToBase64Url(signature)}";
    }

    public static bool IsValid(AdminCredential credential, string token)
    {
        try
        {
            var parts = token.Split('.', 2);
            if (parts.Length != 2) return false;
            var payload = FromBase64Url(parts[0]);
            var suppliedSignature = FromBase64Url(parts[1]);
            var expectedSignature = HMACSHA256.HashData(Convert.FromBase64String(credential.SessionKey), payload);
            if (!CryptographicOperations.FixedTimeEquals(suppliedSignature, expectedSignature)) return false;
            return long.TryParse(Encoding.UTF8.GetString(payload), out var expiresAt)
                && DateTimeOffset.UtcNow.ToUnixTimeSeconds() < expiresAt;
        }
        catch (FormatException) { return false; }
    }

    public static string NewSigningKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static string ToBase64Url(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] FromBase64Url(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 += new string('=', (4 - base64.Length % 4) % 4);
        return Convert.FromBase64String(base64);
    }
}

public static class SalonUserSessions
{
    public static string Create(SalonUser user)
    {
        var payload = Encoding.UTF8.GetBytes($"{user.Id}:{DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds()}");
        var signature = HMACSHA256.HashData(Convert.FromBase64String(user.SessionKey), payload);
        return $"{ToBase64Url(payload)}.{ToBase64Url(signature)}";
    }

    public static bool TryGetUserId(string token, out long id)
    {
        id = 0;
        try { var p=token.Split('.',2);if(p.Length!=2)return false;var data=FromBase64Url(p[0]);var fields=Encoding.UTF8.GetString(data).Split(':',2);return fields.Length==2&&long.TryParse(fields[0],out id)&&id>0; }
        catch (FormatException) { return false; }
    }

    public static bool IsValid(SalonUser user, string token)
    {
        try { var p=token.Split('.',2);if(p.Length!=2)return false;var payload=FromBase64Url(p[0]);var fields=Encoding.UTF8.GetString(payload).Split(':',2);if(fields.Length!=2||!long.TryParse(fields[0],out var id)||id!=user.Id||!long.TryParse(fields[1],out var expiry)||DateTimeOffset.UtcNow.ToUnixTimeSeconds()>=expiry)return false;var signature=FromBase64Url(p[1]);var expected=HMACSHA256.HashData(Convert.FromBase64String(user.SessionKey),payload);return CryptographicOperations.FixedTimeEquals(signature,expected); }
        catch (FormatException) { return false; }
    }

    private static string ToBase64Url(byte[] value)=>Convert.ToBase64String(value).TrimEnd('=').Replace('+','-').Replace('/','_');
    private static byte[] FromBase64Url(string value){var b=value.Replace('-','+').Replace('_','/');b+=new string('=',(4-b.Length%4)%4);return Convert.FromBase64String(b);}
}
