using CertifiedSkill.Data;
using CertifiedSkill.Data.Participant;
using Microsoft.EntityFrameworkCore;

namespace CertifiedSkill.Services.Participant
{
    /// <summary>
    /// Handles linking and lookup of external identities (e.g. BankID) for a Person.
    /// </summary>
    public class ExternalIdentityService(ApplicationDbContext db)
    {
        /// <summary>
        /// Links an external identity to a person.
        /// Throws if the person does not exist or the identity is already linked.
        /// </summary>
        public async Task<ExternalIdentity> LinkAsync(
            Guid personId,
            string provider,
            string providerSubjectId,
            CancellationToken cancellationToken = default)
        {
            var personExists = await db.Persons
                .AnyAsync(p => p.Id == personId, cancellationToken);

            if (!personExists)
                throw new InvalidOperationException("Person not found.");

            var alreadyLinked = await db.ExternalIdentities
                .AnyAsync(e => e.Provider == provider &&
                               e.ProviderSubjectId == providerSubjectId,
                          cancellationToken);

            if (alreadyLinked)
                throw new InvalidOperationException(
                    "This external identity is already linked to a person.");

            var identity = new ExternalIdentity
            {
                PersonId = personId,
                Provider = provider,
                ProviderSubjectId = providerSubjectId
            };

            db.ExternalIdentities.Add(identity);
            await db.SaveChangesAsync(cancellationToken);

            return identity;
        }

        /// <summary>
        /// Finds a person by external identity provider and subject ID.
        /// Returns null if not found.
        /// </summary>
        public async Task<Person?> FindPersonByExternalIdentityAsync(
            string provider,
            string providerSubjectId,
            CancellationToken cancellationToken = default)
        {
            var identity = await db.ExternalIdentities
                .Include(e => e.Person)
                .FirstOrDefaultAsync(e => e.Provider == provider &&
                                          e.ProviderSubjectId == providerSubjectId,
                                    cancellationToken);

            return identity?.Person;
        }

        /// <summary>
        /// Returns all external identities linked to a person.
        /// </summary>
        public async Task<IReadOnlyList<ExternalIdentity>> GetByPersonAsync(
            Guid personId,
            CancellationToken cancellationToken = default)
        {
            return await db.ExternalIdentities
                .Where(e => e.PersonId == personId)
                .ToListAsync(cancellationToken);
        }
    }
}
