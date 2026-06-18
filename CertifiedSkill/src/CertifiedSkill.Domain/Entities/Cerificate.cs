namespace CertifiedSkill.Domain.Entities;

public class Certificate
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string CourseName { get; set; } = null!;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    // immutability flag (viktig i din EPIC)
    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }
}