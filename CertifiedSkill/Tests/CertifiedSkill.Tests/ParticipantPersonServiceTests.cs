using CertifiedSkill.Data;
using CertifiedSkill.Data.Identity;
using CertifiedSkill.Data.Participant;
using CertifiedSkill.Data.Protection;
using CertifiedSkill.Services.Participant;
using CertifiedSkill.Services.Pnr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CertifiedSkill.Tests
{
    public class ParticipantPersonServiceTests
    {
        private static AesPnrProtectionService CreatePnrService()
        {
            var keyMaterial = Convert.ToBase64String(new byte[32]);
            var options = new PnrProtectionOptions
            {
                Enabled = true,
                ActiveEncryptionKeyVersion = "2026-04",
                ActiveHashKeyVersion = "2026-04",
                EncryptionKeysConfiguration = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["2026-04"] = keyMaterial
                },
                HashKeysConfiguration = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["2026-04"] = keyMaterial
                }
            };
            return new AesPnrProtectionService(Options.Create(options));
        }

        private static ApplicationDbContext CreateDb()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(dbOptions);
        }

        private static ParticipantPersonService CreateService(ApplicationDbContext db)
        {
            var pnrService = CreatePnrService();
            var validator = new SwedishPersonalIdentityNumberValidator();
            return new ParticipantPersonService(db, pnrService, validator);
        }

        [Fact]
        public async Task CreateWithPnrAsync_ShouldCreatePerson_WithEncryptedPnrAndHash()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            var person = await service.CreateWithPnrAsync(
                "test@example.com",
                "Test Person",
                "199001010017");

            Assert.NotNull(person);
            Assert.NotNull(person.EncryptedPnr);
            Assert.NotNull(person.PnrHash);
            Assert.NotNull(person.PnrKeyVersion);
            Assert.DoesNotContain("199001010017", person.EncryptedPnr);
            Assert.DoesNotContain("199001010017", person.PnrHash);
        }

        [Fact]
        public async Task CreateWithPnrAsync_ShouldThrow_WhenDuplicatePnr()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            await service.CreateWithPnrAsync("first@example.com", "First", "199001010017");

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateWithPnrAsync("second@example.com", "Second", "199001010017"));
        }

        [Fact]
        public async Task CreateWithPnrAsync_ShouldThrow_WhenInvalidPnr()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateWithPnrAsync("test@example.com", "Test", "not-a-pnr"));
        }

        [Fact]
        public async Task FindByPnrAsync_ShouldReturnPerson_WhenPnrExists()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            await service.CreateWithPnrAsync("test@example.com", "Test Person", "199001010017");

            var found = await service.FindByPnrAsync("199001010017");

            Assert.NotNull(found);
            Assert.Equal("test@example.com", found.Email);
        }

        [Fact]
        public async Task FindByPnrAsync_ShouldReturnNull_WhenPnrDoesNotExist()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            var found = await service.FindByPnrAsync("199001010017");

            Assert.Null(found);
        }

        [Fact]
        public async Task ExistsByPnrAsync_ShouldReturnTrue_WhenPnrExists()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            await service.CreateWithPnrAsync("test@example.com", "Test", "199001010017");

            var exists = await service.ExistsByPnrAsync("199001010017");

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByPnrAsync_ShouldReturnFalse_WhenPnrDoesNotExist()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            var exists = await service.ExistsByPnrAsync("199001010017");

            Assert.False(exists);
        }

        [Fact]
        public async Task FindByPnrAsync_ShouldAcceptDifferentFormats_ForSamePnr()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            // Create with 12-digit format
            await service.CreateWithPnrAsync("test@example.com", "Test", "199001010017");

            // Find with 10-digit format with separator
            var found = await service.FindByPnrAsync("900101-0017");

            Assert.NotNull(found);
            Assert.Equal("test@example.com", found.Email);
        }
    }
}
