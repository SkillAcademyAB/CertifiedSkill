using System.Security.Cryptography;
using System.Text;
using CertifiedSkill.Data;
using CertifiedSkill.Data.Participant;
using Microsoft.EntityFrameworkCore;

namespace CertifiedSkill.Services.Participant
{
    public sealed class ParticipantMagicLinkService(
        ApplicationDbContext db,
        IParticipantEmailSender emailSender,
        ILogger<ParticipantMagicLinkService> logger,
        TimeProvider timeProvider)
        : IParticipantMagicLinkService
    {
        private const int TokenValidityMinutes = 15;
        private const int RateLimitWindowMinutes = 15;
        private const int RateLimitMaxRequests = 3;
        private const int RawTokenBytes = 32;

        public async Task<MagicLinkRequestResult> RequestMagicLinkAsync(
            string email,
            string magicLinkBaseUrl,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmail = NormalizeEmail(email);
            var now = timeProvider.GetUtcNow();

            if (await IsRateLimitedAsync(normalizedEmail, now, cancellationToken))
            {
                logger.LogWarning("Magic link request rate-limited");
                return MagicLinkRequestResult.RateLimited;
            }

            var rawToken = GenerateRawToken();
            var tokenHash = HashToken(rawToken);

            var token = new ParticipantMagicLinkToken
            {
                Email = normalizedEmail,
                TokenHash = tokenHash,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(TokenValidityMinutes)
            };

            db.ParticipantMagicLinkTokens.Add(token);
            await db.SaveChangesAsync(cancellationToken);

            var magicLink = BuildMagicLinkUrl(magicLinkBaseUrl, rawToken);

            await emailSender.SendMagicLinkAsync(normalizedEmail, magicLink, cancellationToken);

            logger.LogInformation("Magic link sent");
            return MagicLinkRequestResult.Sent;
        }

        public async Task<(MagicLinkConsumeResult Result, string? Email)> ConsumeTokenAsync(
            string rawToken,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return (MagicLinkConsumeResult.Invalid, null);
            }

            string tokenHash;
            try
            {
                tokenHash = HashToken(rawToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to hash token during consume");
                return (MagicLinkConsumeResult.Invalid, null);
            }

            var now = timeProvider.GetUtcNow();

            var record = await db.ParticipantMagicLinkTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

            if (record is null)
            {
                return (MagicLinkConsumeResult.Invalid, null);
            }

            if (record.IsUsed)
            {
                return (MagicLinkConsumeResult.AlreadyUsed, null);
            }

            if (record.IsExpired(now))
            {
                return (MagicLinkConsumeResult.Expired, null);
            }

            record.MarkUsed(now);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Magic link consumed successfully");
            return (MagicLinkConsumeResult.Success, record.Email);
        }

        private async Task<bool> IsRateLimitedAsync(string normalizedEmail, DateTimeOffset now, CancellationToken cancellationToken)
        {
            var windowStart = now.AddMinutes(-RateLimitWindowMinutes);
            var recentCount = await db.ParticipantMagicLinkTokens
                .CountAsync(
                    t => t.Email == normalizedEmail && t.CreatedAt >= windowStart,
                    cancellationToken);

            return recentCount >= RateLimitMaxRequests;
        }

        private static string GenerateRawToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(RawTokenBytes);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToBase64String(bytes);
        }

        private static string NormalizeEmail(string email) =>
            email.Trim().ToLowerInvariant();

        private static string BuildMagicLinkUrl(string baseUrl, string rawToken) =>
            $"{baseUrl.TrimEnd('/')}?token={Uri.EscapeDataString(rawToken)}";
    }
}
