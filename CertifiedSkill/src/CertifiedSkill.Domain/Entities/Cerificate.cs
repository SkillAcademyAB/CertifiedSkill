namespace CertifiedSkill.Domain.Entities;

public class Certificate
{
    public Guid Id { get; private set; }

    public Guid PersonId { get; private set; }

    public string CourseName { get; private set; } = null!;

    public DateTime IssuedAt { get; private set; }

    public bool IsRevoked { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    private Certificate() { } // EF Core

    public Certificate(Guid personId, string courseName)
    {
        Id = Guid.NewGuid();
        PersonId = personId;
        CourseName = courseName;
        IssuedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}