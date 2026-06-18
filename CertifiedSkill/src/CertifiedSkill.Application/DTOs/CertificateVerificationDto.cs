namespace CertifiedSkill.Application.DTOs;

public class CertificateVerificationDto
{
    public Guid CertificateId { get; set; }
    public string CourseName { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
    public bool IsRevoked { get; set; }
}