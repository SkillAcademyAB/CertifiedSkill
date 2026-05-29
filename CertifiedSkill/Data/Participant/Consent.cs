namespace CertifiedSkill.Data.Participant
{
    /// <summary>
    /// Tracks GDPR consent given by a Person.
    /// Consent can be granted, versioned, and revoked.
    /// </summary>
    public class Consent
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public Guid PersonId { get; init; }

        /// <summary>
        /// The type of consent, e.g. "DataProcessing" or "Marketing".
        /// </summary>
        public string ConsentType { get; init; } = string.Empty;

        /// <summary>
        /// Version of the consent text the person agreed to.
        /// </summary>
        public string ConsentVersion { get; init; } = string.Empty;

        /// <summary>
        /// When consent was granted.
        /// </summary>
        public DateTimeOffset GrantedAt { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// When consent was revoked. Null means still active.
        /// </summary>
        public DateTimeOffset? RevokedAt { get; set; }

        public bool IsActive => RevokedAt is null;

        public void Revoke(DateTimeOffset revokedAt)
        {
            RevokedAt = revokedAt;
        }

        public Person Person { get; init; } = null!;
    }
}
