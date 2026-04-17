namespace CertifiedSkill.Data.Participant
{
    public class Certificate
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public Guid PersonId { get; init; }

        public string CourseName { get; set; } = string.Empty;

        public string CourseVersion { get; set; } = string.Empty;

        public DateTimeOffset IssuedAt { get; init; }

        public CertificateStatus Status { get; set; } = CertificateStatus.Active;

        public DateTimeOffset? RevokedAt { get; set; }

        public Person Person { get; init; } = null!;
    }
}
