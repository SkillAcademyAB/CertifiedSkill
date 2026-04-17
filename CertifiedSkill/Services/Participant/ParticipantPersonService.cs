using CertifiedSkill.Data;
using CertifiedSkill.Data.Participant;
using Microsoft.EntityFrameworkCore;

namespace CertifiedSkill.Services.Participant
{
    public sealed class ParticipantPersonService(ApplicationDbContext db)
    {
        /// <summary>
        /// Returns the <see cref="Person"/> for the given normalised email,
        /// or null if no person is linked to that email yet.
        /// </summary>
        public Task<Person?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
            db.Persons
              .Include(p => p.Certificates)
              .FirstOrDefaultAsync(p => p.Email == normalizedEmail, cancellationToken);
    }
}
