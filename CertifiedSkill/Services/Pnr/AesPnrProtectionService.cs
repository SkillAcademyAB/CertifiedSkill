using System.Security.Cryptography;
using System.Text;
using CertifiedSkill.Data.Protection;
using Microsoft.Extensions.Options;

namespace CertifiedSkill.Services.Pnr
{
    /// <summary>
    /// AES-256-GCM implementation of IPnrProtectionService.
    /// Uses versioned keys from PnrProtectionOptions.
    /// </summary>
    public sealed class AesPnrProtectionService : IPnrProtectionService
    {
        private const int NonceSizeBytes = 12;
        private const int TagSizeBytes = 16;
        private readonly PnrProtectionOptions _options;

        public AesPnrProtectionService(IOptions<PnrProtectionOptions> options)
        {
            _options = options.Value;
        }

        public (string EncryptedValue, string KeyVersion) Encrypt(string normalizedPnr)
        {
            EnsureEnabled();

            var keyVersion = _options.ActiveEncryptionKeyVersion;
            var keyBytes = GetEncryptionKey(keyVersion);

            var plaintext = Encoding.UTF8.GetBytes(normalizedPnr);
            var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[TagSizeBytes];

            using var aesGcm = new AesGcm(keyBytes, TagSizeBytes);
            aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

            // Format: base64(nonce + ciphertext + tag)
            var combined = new byte[NonceSizeBytes + ciphertext.Length + TagSizeBytes];
            nonce.CopyTo(combined, 0);
            ciphertext.CopyTo(combined, NonceSizeBytes);
            tag.CopyTo(combined, NonceSizeBytes + ciphertext.Length);

            return (Convert.ToBase64String(combined), keyVersion);
        }

        public string Decrypt(string encryptedValue, string keyVersion)
        {
            EnsureEnabled();

            var keyBytes = GetEncryptionKey(keyVersion);
            var combined = Convert.FromBase64String(encryptedValue);

            if (combined.Length < NonceSizeBytes + TagSizeBytes)
                throw new CryptographicException("Invalid encrypted value: too short.");

            var nonce = combined[..NonceSizeBytes];
            var tag = combined[^TagSizeBytes..];
            var ciphertext = combined[NonceSizeBytes..^TagSizeBytes];
            var plaintext = new byte[ciphertext.Length];

            using var aesGcm = new AesGcm(keyBytes, TagSizeBytes);
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }

        public string ComputeHash(string normalizedPnr)
        {
            EnsureEnabled();

            var keyVersion = _options.ActiveHashKeyVersion;
            var keyBytes = GetHashKey(keyVersion);
            var data = Encoding.UTF8.GetBytes(normalizedPnr);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }

        private void EnsureEnabled()
        {
            if (!_options.Enabled)
                throw new InvalidOperationException("PNR protection is not enabled.");
        }

        private byte[] GetEncryptionKey(string version)
        {
            if (!_options.EncryptionKeys.TryGetValue(version, out var base64Key))
                throw new InvalidOperationException($"Encryption key version '{version}' is not configured.");
            return Convert.FromBase64String(base64Key);
        }

        private byte[] GetHashKey(string version)
        {
            if (!_options.HashKeys.TryGetValue(version, out var base64Key))
                throw new InvalidOperationException($"Hash key version '{version}' is not configured.");
            return Convert.FromBase64String(base64Key);
        }
    }
}
