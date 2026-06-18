using CertifiedSkill.Domain.Entities;

namespace CertifiedSkill.Application.Services;

public class CertificateService
{
    public Certificate IssueCertificate(Guid personId, string courseName)
    {
        // ONLY orchestration here — domain handles creation rules
        return new Certificate(personId, courseName);
    }
}