using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Void.Bot.Core.Settings.Account;

namespace Void.Bot.Core.Services.Identity;

public interface ITotpService : ISingleton
{
    string GenerateSecret();

    bool VerifyCode(string secret, string? code);

    string CreateOtpAuthUri(string issuer, string accountName, string secret);
}

public sealed class TotpService(TimeProvider timeProvider, TwoFactorSetting twoFactorSetting) : ITotpService
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    private const int TimeStepSeconds = 30;

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        try
        {
            return Base32Encode(bytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
        }
    }

    public bool VerifyCode(string secret, string? code)
    {
        if (code is not { Length: 6 } || code.Any(character => character is < '0' or > '9'))
        {
            return false;
        }

        var currentTimeStep = timeProvider.GetUtcNow().ToUnixTimeSeconds() / TimeStepSeconds;
        for (var offset = -twoFactorSetting.AllowedTimeStepDrift;
             offset <= twoFactorSetting.AllowedTimeStepDrift;
             offset++)
        {
            if (FixedTimeEquals(GenerateCode(secret, currentTimeStep + offset), code))
            {
                return true;
            }
        }

        return false;
    }

    public string CreateOtpAuthUri(string issuer, string accountName, string secret)
    {
        var label = Uri.EscapeDataString($"{issuer}:{accountName}");
        return $"otpauth://totp/{label}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period={TimeStepSeconds}";
    }

    private static string GenerateCode(string secret, long timeStep)
    {
        var key = Base32Decode(secret);
        Span<byte> counter = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(counter, timeStep);

        try
        {
            using var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(counter.ToArray());
            try
            {
                var offset = hash[^1] & 0x0f;
                var value = BinaryPrimitives.ReadInt32BigEndian(hash.AsSpan(offset, sizeof(int))) & 0x7fffffff;
                return (value % 1_000_000).ToString("D6");
            }
            finally
            {
                CryptographicOperations.ZeroMemory(hash);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expected),
            Encoding.ASCII.GetBytes(actual));
    }

    private static string Base32Encode(ReadOnlySpan<byte> bytes)
    {
        var result = new StringBuilder((bytes.Length * 8 + 4) / 5);
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var value in bytes)
        {
            buffer = (buffer << 8) | value;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                result.Append(Base32Alphabet[(buffer >> (bitsLeft - 5)) & 31]);
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
        {
            result.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 31]);
        }

        return result.ToString();
    }

    private static byte[] Base32Decode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var output = new byte[value.Length * 5 / 8];
        var buffer = 0;
        var bitsLeft = 0;
        var outputIndex = 0;

        foreach (var character in value)
        {
            var digit = Base32Alphabet.IndexOf(char.ToUpperInvariant(character));
            if (digit < 0)
            {
                throw new FormatException("The TOTP secret is not valid Base32.");
            }

            buffer = (buffer << 5) | digit;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                output[outputIndex++] = (byte)(buffer >> (bitsLeft - 8));
                bitsLeft -= 8;
            }
        }

        return output;
    }
}
