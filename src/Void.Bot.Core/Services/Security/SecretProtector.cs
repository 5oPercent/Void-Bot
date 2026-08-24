using System.Security.Cryptography;
using System.Text;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.Core.Services.Security;

public interface ISecretProtector : ISingleton
{
    string Protect(string value, string purpose);

    string Unprotect(string value, string purpose);
}

public sealed class SecretProtector(SecretProtectionSetting setting) : ISecretProtector
{
    private const byte FormatVersion = 1;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public string Protect(string value, string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        var key = GetKey();
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintext = Encoding.UTF8.GetBytes(value);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];
        var associatedData = Encoding.UTF8.GetBytes(purpose);

        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

            var payload = new byte[1 + NonceSize + TagSize + ciphertext.Length];
            payload[0] = FormatVersion;
            nonce.CopyTo(payload, 1);
            tag.CopyTo(payload, 1 + NonceSize);
            ciphertext.CopyTo(payload, 1 + NonceSize + TagSize);
            return Convert.ToBase64String(payload);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
            CryptographicOperations.ZeroMemory(key);
        }
    }

    public string Unprotect(string value, string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        var payload = Convert.FromBase64String(value);
        if (payload.Length <= 1 + NonceSize + TagSize || payload[0] != FormatVersion)
        {
            throw new CryptographicException("Invalid protected secret.");
        }

        var nonce = payload.AsSpan(1, NonceSize);
        var tag = payload.AsSpan(1 + NonceSize, TagSize);
        var ciphertext = payload.AsSpan(1 + NonceSize + TagSize);
        var plaintext = new byte[ciphertext.Length];
        var key = GetKey();
        var associatedData = Encoding.UTF8.GetBytes(purpose);

        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);
            return Encoding.UTF8.GetString(plaintext);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
            CryptographicOperations.ZeroMemory(key);
        }
    }

    private byte[] GetKey()
    {
        byte[] key;
        try
        {
            key = Convert.FromBase64String(setting.EncryptionKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "SecretProtection:EncryptionKey must be a Base64 encoded 32-byte key.", exception);
        }

        if (key.Length != 32)
        {
            CryptographicOperations.ZeroMemory(key);
            throw new InvalidOperationException(
                "SecretProtection:EncryptionKey must be a Base64 encoded 32-byte key.");
        }

        return key;
    }
}
