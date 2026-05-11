namespace CertifiedSkill.Services.Pnr
{
    /// <summary>
    /// Handles encryption, decryption and hashing of Swedish personal identity numbers (PNR).
    /// All operations require PNR to be normalized before calling.
    /// </summary>
    public interface IPnrProtectionService
    {
        /// <summary>
        /// Encrypts a normalized PNR. Returns the encrypted value and the key version used.
        /// </summary>
        (string EncryptedValue, string KeyVersion) Encrypt(string normalizedPnr);

        /// <summary>
        /// Decrypts an encrypted PNR using the specified key version.
        /// </summary>
        string Decrypt(string encryptedValue, string keyVersion);

        /// <summary>
        /// Computes a stable HMAC-SHA256 hash of a normalized PNR for deduplication and lookup.
        /// </summary>
        string ComputeHash(string normalizedPnr);
    }
}
