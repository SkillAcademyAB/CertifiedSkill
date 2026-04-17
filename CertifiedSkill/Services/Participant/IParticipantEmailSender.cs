namespace CertifiedSkill.Services.Participant
{
    public interface IParticipantEmailSender
    {
        /// <summary>
        /// Sends a magic link to the specified email address.
        /// </summary>
        Task SendMagicLinkAsync(string email, string magicLinkUrl, CancellationToken cancellationToken = default);
    }
}
