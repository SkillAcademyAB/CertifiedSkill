namespace CertifiedSkill.Data.Participant
{
    public class Person
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Normalized (lowercase, trimmed) email used as the stable lookup key.
        /// </summary>
        public string Email { get; init; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// AES-256-GCM encrypted PNR. Never exposed in logs or UI.
        /// Null if PNR has not been provided.
        /// </summary>
        public string? EncryptedPnr { get; set; }

        /// <summary>
        /// The key version used to encrypt EncryptedPnr.
        /// Required when EncryptedPnr is set.
        /// </summary>
        public string? PnrKeyVersion { get; set; }

        /// <summary>
        /// HMAC-SHA256 hash of the normalized PNR.
        /// Used for deduplication and lookup without exposing plaintext.
        /// Null if PNR has not been provided.
        /// </summary>
        public string? PnrHash { get; set; }

        public ICollection<Certificate> Certificates { get; init; } = new List<Certificate>();

        public ICollection<Consent> Consents { get; init; } = new List<Consent>();

        public ICollection<ExternalIdentity> ExternalIdentities { get; init; } = new List<ExternalIdentity>();
    }
}
