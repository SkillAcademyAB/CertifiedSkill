using CertifiedSkill.Data;
using CertifiedSkill.Data.Participant;
using CertifiedSkill.Services.Participant;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CertifiedSkill.Tests
{
    public class ExternalIdentityServiceTests
    {
        private static ApplicationDbContext CreateDb()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;
            var db = new ApplicationDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        private static async Task<Person> CreatePersonAsync(ApplicationDbContext db)
        {
            var person = new Person
            {
                Email = "test@example.com",
                DisplayName = "Test Person"
            };
            db.Persons.Add(person);
            await db.SaveChangesAsync();
            return person;
        }

        [Fact]
        public async Task LinkAsync_ShouldLinkExternalIdentity_WhenPersonExists()
        {
            using var db = CreateDb();
            var person = await CreatePersonAsync(db);
            var service = new ExternalIdentityService(db);

            var identity = await service.LinkAsync(person.Id, "BankID", "199001010017");

            Assert.Equal("BankID", identity.Provider);
            Assert.Equal("199001010017", identity.ProviderSubjectId);
            Assert.Equal(person.Id, identity.PersonId);
        }

        [Fact]
        public async Task LinkAsync_ShouldThrow_WhenPersonDoesNotExist()
        {
            using var db = CreateDb();
            var service = new ExternalIdentityService(db);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.LinkAsync(Guid.NewGuid(), "BankID", "199001010017"));
        }

        [Fact]
        public async Task LinkAsync_ShouldThrow_WhenIdentityAlreadyLinked()
        {
            using var db = CreateDb();
            var person = await CreatePersonAsync(db);
            var service = new ExternalIdentityService(db);

            await service.LinkAsync(person.Id, "BankID", "199001010017");

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.LinkAsync(person.Id, "BankID", "199001010017"));
        }

        [Fact]
        public async Task FindPersonByExternalIdentityAsync_ShouldReturnPerson_WhenLinked()
        {
            using var db = CreateDb();
            var person = await CreatePersonAsync(db);
            var service = new ExternalIdentityService(db);

            await service.LinkAsync(person.Id, "BankID", "199001010017");
            var found = await service.FindPersonByExternalIdentityAsync("BankID", "199001010017");

            Assert.NotNull(found);
            Assert.Equal(person.Id, found.Id);
        }

        [Fact]
        public async Task FindPersonByExternalIdentityAsync_ShouldReturnNull_WhenNotLinked()
        {
            using var db = CreateDb();
            var service = new ExternalIdentityService(db);

            var found = await service.FindPersonByExternalIdentityAsync("BankID", "199001010017");

            Assert.Null(found);
        }

        [Fact]
        public async Task GetByPersonAsync_ShouldReturnLinkedIdentities()
        {
            using var db = CreateDb();
            var person = await CreatePersonAsync(db);
            var service = new ExternalIdentityService(db);

            await service.LinkAsync(person.Id, "BankID", "199001010017");
            var identities = await service.GetByPersonAsync(person.Id);

            Assert.Single(identities);
            Assert.Equal("BankID", identities[0].Provider);
        }
    }
}
