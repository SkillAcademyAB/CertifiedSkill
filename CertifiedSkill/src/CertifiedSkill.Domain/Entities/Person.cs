namespace CertifiedSkill.Domain.Entities;

public class Person
{
    public Guid Id { get; set; }

    // Krypterat personnummer (aldrig klartext)
    public string EncryptedNationalId { get; set; } = null!;

    // Hash för dedupe/sök
    public string NationalIdHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Consent> Consents { get; set; } = new();
    public List<ExternalIdentity> ExternalIdentities { get; set; } = new();
}