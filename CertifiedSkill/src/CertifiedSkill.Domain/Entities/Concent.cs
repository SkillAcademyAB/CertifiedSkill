namespace CertifiedSkill.Domain.Entities;

public class Consent
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public string Type { get; set; } = null!; // t.ex. "GDPR", "TrainingData"

    public int Version { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RevokedAt == null;
}