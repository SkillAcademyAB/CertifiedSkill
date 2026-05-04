using CertifiedSkill.Data;
using CertifiedSkill.Data.Identity;
using CertifiedSkill.Data.Participant;
using CertifiedSkill.Services.Pnr;
using Microsoft.EntityFrameworkCore;

namespace CertifiedSkill.Services.Participant
{
    public sealed class ParticipantPersonService(
        ApplicationDbContext db,
        IPnrProtectionService pnrProtection,
        IPersonalIdentityNumberValidator pnrValidator)
    {
        /// <summary>
        /// Returns the Person for the given normalised email,
        /// or null if no person is linked to that email yet.
        /// </summary>
        public Task<Person?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
            db.Persons
              .Include(p => p.Certificates)
              .FirstOrDefaultAsync(p => p.Email == normalizedEmail, cancellationToken);

        /// <summary>
        /// Finds a Person by PNR hash for deduplication and lookup.
        /// Never accepts plaintext PNR - validates and hashes before lookup.
        /// Returns null if no person with that PNR exists.
        /// </summary>
        public async Task<Person?> FindByPnrAsync(string rawPnr, CancellationToken cancellationToken = default)
        {
            if (!pnrValidator.TryNormalize(rawPnr, out var normalizedPnr))
                throw new ArgumentException("Invalid PNR format.", nameof(rawPnr));

            var pnrHash = pnrProtection.ComputeHash(normalizedPnr);

            return await db.Persons
                .Include(p => p.Certificates)
                .FirstOrDefaultAsync(p => p.PnrHash == pnrHash, cancellationToken);
        }

        /// <summary>
        /// Returns true if a Person with the given PNR already exists.
        /// Used for deduplication before creating a new Person.
        /// </summary>
        public async Task<bool> ExistsByPnrAsync(string rawPnr, CancellationToken cancellationToken = default)
        {
            if (!pnrValidator.TryNormalize(rawPnr, out var normalizedPnr))
                throw new ArgumentException("Invalid PNR format.", nameof(rawPnr));

            var pnrHash = pnrProtection.ComputeHash(normalizedPnr);

            return await db.Persons
                .AnyAsync(p => p.PnrHash == pnrHash, cancellationToken);
        }

        /// <summary>
        /// Creates a new Person with encrypted PNR and PNR hash.
        /// Validates PNR format and checks for duplicates before saving.
        /// </summary>
        public async Task<Person> CreateWithPnrAsync(
            string email,
            string displayName,
            string rawPnr,
            CancellationToken cancellationToken = default)
        {
            if (!pnrValidator.TryNormalize(rawPnr, out var normalizedPnr))
                throw new ArgumentException("Invalid PNR format.", nameof(rawPnr));

            var pnrHash = pnrProtection.ComputeHash(normalizedPnr);

            var exists = await db.Persons
                .AnyAsync(p => p.PnrHash == pnrHash, cancellationToken);

            if (exists)
                throw new InvalidOperationException("A person with this PNR already exists.");

            var (encryptedPnr, keyVersion) = pnrProtection.Encrypt(normalizedPnr);

            var person = new Person
            {
                Email = email.Trim().ToLowerInvariant(),
                DisplayName = displayName,
                EncryptedPnr = encryptedPnr,
                PnrKeyVersion = keyVersion,
                PnrHash = pnrHash
            };

            db.Persons.Add(person);
            await db.SaveChangesAsync(cancellationToken);

            return person;
        }
    }
}
