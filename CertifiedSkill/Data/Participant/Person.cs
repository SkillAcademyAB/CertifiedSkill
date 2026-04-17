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

        public ICollection<Certificate> Certificates { get; init; } = new List<Certificate>();
    }
}
