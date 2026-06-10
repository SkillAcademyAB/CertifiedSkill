namespace CertifiedSkill.Services.Participant
{
    /// <summary>
    /// Sandbox implementation of IBankIdIdentityProvider for development and testing.
    /// Always returns a successful authentication without contacting real BankID.
    /// Never use in production.
    /// </summary>
    public sealed class SandboxBankIdIdentityProvider : IBankIdIdentityProvider
    {
        private readonly Dictionary<string, string> _sessions = new();

        public Task<string> InitiateAuthAsync(
            string personalNumber,
            CancellationToken cancellationToken = default)
        {
            var sessionToken = Guid.NewGuid().ToString();
            _sessions[sessionToken] = personalNumber;
            return Task.FromResult(sessionToken);
        }

        public Task<BankIdAuthResult> PollAuthAsync(
            string sessionToken,
            CancellationToken cancellationToken = default)
        {
            if (_sessions.TryGetValue(sessionToken, out var personalNumber))
            {
                return Task.FromResult(new BankIdAuthResult
                {
                    Status = BankIdAuthStatus.Complete,
                    SubjectId = personalNumber
                });
            }

            return Task.FromResult(new BankIdAuthResult
            {
                Status = BankIdAuthStatus.Failed
            });
        }
    }
}
