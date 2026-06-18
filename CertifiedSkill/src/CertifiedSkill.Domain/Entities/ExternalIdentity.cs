namespace CertifiedSkill.Domain.Entities;

public class ExternalIdentity
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public string Provider { get; set; } = null!; // BankID, etc.

    public string ProviderUserId { get; set; } = null!;

    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}