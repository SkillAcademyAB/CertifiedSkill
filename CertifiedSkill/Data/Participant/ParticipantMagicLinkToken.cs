namespace CertifiedSkill.Data.Participant
{
    /// <summary>
    /// Stores a pending magic link token request.
    /// The raw token is never persisted; only its SHA-256 hash is stored.
    /// Tokens are single-use and time-limited.
    /// </summary>
    public class ParticipantMagicLinkToken
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>Normalized (lowercase, trimmed) email the token was issued for.</summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>SHA-256 hash (Base64url) of the raw token sent via email.</summary>
        public string TokenHash { get; init; } = string.Empty;

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

        public DateTimeOffset ExpiresAt { get; init; }

        /// <summary>Set when the token is consumed. Null means unused.</summary>
        public DateTimeOffset? UsedAt { get; private set; }

        public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

        public bool IsUsed => UsedAt.HasValue;

        public bool IsValid(DateTimeOffset now) => !IsExpired(now) && !IsUsed;

        public void MarkUsed(DateTimeOffset usedAt)
        {
            UsedAt = usedAt;
        }
    }
}
