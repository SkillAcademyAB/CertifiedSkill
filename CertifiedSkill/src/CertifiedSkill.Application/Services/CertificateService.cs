using CertifiedSkill.Domain.Entities;
using CertifiedSkill.Application.DTOs;

namespace CertifiedSkill.Application.Services;

public class CertificateService
{
    public Certificate IssueCertificate(Guid personId, string courseName)
    {
        return new Certificate(personId, courseName);
    }

    public void RevokeCertificate(Certificate certificate)
    {
        certificate.Revoke();
    }

    public CertificateVerificationDto Verify(Certificate certificate)
    {
        return new CertificateVerificationDto
        {
            CertificateId = certificate.Id,
            CourseName = certificate.CourseName,
            IssuedAt = certificate.IssuedAt,
            IsRevoked = certificate.IsRevoked
        };
    }
}