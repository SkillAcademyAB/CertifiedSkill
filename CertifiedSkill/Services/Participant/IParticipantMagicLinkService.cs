namespace CertifiedSkill.Services.Participant
{
    public enum MagicLinkRequestResult
    {
        Sent,
        RateLimited
    }

    public enum MagicLinkConsumeResult
    {
        Success,
        Invalid,
        Expired,
        AlreadyUsed
    }

    public interface IParticipantMagicLinkService
    {
        /// <summary>
        /// Generates a magic link token, persists it and sends the link via email.
        /// Returns <see cref="MagicLinkRequestResult.RateLimited"/> when too many
        /// tokens have been requested for the same email within the rate-limit window.
        /// </summary>
        Task<MagicLinkRequestResult> RequestMagicLinkAsync(
            string email,
            string magicLinkBaseUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates and atomically consumes the raw token from the magic link URL.
        /// On success the token is marked as used and the normalised email is returned.
        /// </summary>
        Task<(MagicLinkConsumeResult Result, string? Email)> ConsumeTokenAsync(
            string rawToken,
            CancellationToken cancellationToken = default);
    }
}
