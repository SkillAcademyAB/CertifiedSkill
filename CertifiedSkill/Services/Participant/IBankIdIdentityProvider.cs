namespace CertifiedSkill.Services.Participant
{
    /// <summary>
    /// Defines the contract for a BankID identity provider.
    /// Implementations can be swapped between sandbox and production
    /// without changing the consuming code.
    /// </summary>
    public interface IBankIdIdentityProvider
    {
        /// <summary>
        /// Initiates a BankID authentication session.
        /// Returns a session token to be used for polling.
        /// </summary>
        Task<string> InitiateAuthAsync(string personalNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Polls the status of an ongoing BankID authentication session.
        /// </summary>
        Task<BankIdAuthResult> PollAuthAsync(string sessionToken, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents the result of a BankID authentication poll.
    /// </summary>
    public sealed class BankIdAuthResult
    {
        public BankIdAuthStatus Status { get; init; }

        /// <summary>
        /// The verified subject identifier from BankID.
        /// Only set when Status is Complete.
        /// </summary>
        public string? SubjectId { get; init; }
    }

    public enum BankIdAuthStatus
    {
        Pending,
        Complete,
        Failed
    }
}
