using CertifiedSkill.Domain.Entities;

namespace CertifiedSkill.Application.Services;

public class CertificateService
{
    public Certificate IssueCertificate(Guid personId, string courseName)
    {
        return new Certificate(personId, courseName);
    }

    public void RevokeCertificate(Certificate certificate)
    {
        if (certificate is null)
            throw new ArgumentNullException(nameof(certificate));

        certificate.Revoke();
    }

    public CertificateVerificationDto Verify(Certificate certificate)
    {
        if (certificate is null)
            throw new ArgumentNullException(nameof(certificate));

        return new CertificateVerificationDto
        {
            CertificateId = certificate.Id,
            CourseName = certificate.CourseName,
            IssuedAt = certificate.IssuedAt,
            IsRevoked = certificate.IsRevoked
        };
    }
}