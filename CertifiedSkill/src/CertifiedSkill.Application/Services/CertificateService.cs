using CertifiedSkill.Domain.Entities;

namespace CertifiedSkill.Application.Services;

public class CertificateService
{
    public Certificate IssueCertificate(Guid personId, string courseName)
    {
        return new Certificate
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            CourseName = courseName,
            IssuedAt = DateTime.UtcNow
        };
    }
}
// add comment temporarily
// DEBUG: making sure PR diff exists