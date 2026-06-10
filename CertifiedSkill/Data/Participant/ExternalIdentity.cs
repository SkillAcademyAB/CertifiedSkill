namespace CertifiedSkill.Data.Participant
{
    /// <summary>
    /// Represents an external identity linked to a Person,
    /// such as a BankID or other verified identity provider.
    /// </summary>
    public class ExternalIdentity
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public Guid PersonId { get; init; }

        /// <summary>
        /// The identity provider, e.g. "BankID".
        /// </summary>
        public string Provider { get; init; } = string.Empty;

        /// <summary>
        /// The unique subject identifier from the provider.
        /// </summary>
        public string ProviderSubjectId { get; init; } = string.Empty;

        /// <summary>
        /// When this external identity was linked.
        /// </summary>
        public DateTimeOffset LinkedAt { get; init; } = DateTimeOffset.UtcNow;

        public Person Person { get; init; } = null!;
    }
}
