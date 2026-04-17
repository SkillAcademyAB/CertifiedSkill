namespace CertifiedSkill.Services.Participant
{
    /// <summary>
    /// No-op email sender for development and testing.
    /// Replace with a real sender (e.g. SendGrid, SMTP) in production.
    /// </summary>
    public sealed class NoOpParticipantEmailSender(ILogger<NoOpParticipantEmailSender> logger)
        : IParticipantEmailSender
    {
        public Task SendMagicLinkAsync(string email, string magicLinkUrl, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("NOOP: Sending magic link (link omitted from log)");

            return Task.CompletedTask;
        }
    }
}
