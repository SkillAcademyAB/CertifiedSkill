using CertifiedSkill.Data;
using CertifiedSkill.Data.Participant;
using CertifiedSkill.Services.Export;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CertifiedSkill.Tests
{
    public class CourseExportServiceTests
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

        private static async Task<Person> CreatePersonAsync(ApplicationDbContext db, string email, string name)
        {
            var person = new Person { Email = email, DisplayName = name };
            db.Persons.Add(person);
            await db.SaveChangesAsync();
            return person;
        }

        private static async Task<Certificate> CreateCertificateAsync(
            ApplicationDbContext db, Person person, string courseName, string version)
        {
            var cert = new Certificate
            {
                PersonId = person.Id,
                CourseName = courseName,
                CourseVersion = version,
                IssuedAt = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero),
                Status = CertificateStatus.Active
            };
            db.Certificates.Add(cert);
            await db.SaveChangesAsync();
            return cert;
        }

        [Fact]
        public async Task ExportCertificatesCsvAsync_ShouldContainHeader()
        {
            using var db = CreateDb();
            var service = new CourseExportService(db);

            var csv = await service.ExportCertificatesCsvAsync("Test Course");

            Assert.Contains("DisplayName,Email,CourseName,CourseVersion,IssuedAt,Status", csv);
        }

        [Fact]
        public async Task ExportCertificatesCsvAsync_ShouldNotContainPnr()
        {
            using var db = CreateDb();
            var person = await CreatePersonAsync(db, "test@example.com", "Test Person");
            person.EncryptedPnr = "encrypted-pnr-value";
            person.PnrHash = "hash-value";
            await db.SaveChangesAsync();
            await CreateCertificateAsync(db, person, "Test Course", "1.0");
            var service = new CourseExportService(db);

            var csv = await service.ExportCertificatesCsvAsync("Test Course");

            Assert.DoesNotContain("encrypted-pnr-value", csv);
            Assert.DoesNotContain("hash-value", csv);
        }

        [Fact]
        public async Task ExportCertificatesCsvAsync_ShouldContainPersonData()
        {
            using var db = CreateDb();
            var person = await CreatePersonAsync(db, "test@example.com", "Test Person");
            await CreateCertificateAsync(db, person, "Test Course", "1.0");
            var service = new CourseExportService(db);

            var csv = await service.ExportCertificatesCsvAsync("Test Course");

            Assert.Contains("Test Person", csv);
            Assert.Contains("test@example.com", csv);
            Assert.Contains("Test Course", csv);
        }

        [Fact]
        public async Task ExportAllCertificatesCsvAsync_ShouldReturnAllCertificates()
        {
            using var db = CreateDb();
            var person1 = await CreatePersonAsync(db, "a@example.com", "Person A");
            var person2 = await CreatePersonAsync(db, "b@example.com", "Person B");
            await CreateCertificateAsync(db, person1, "Course A", "1.0");
            await CreateCertificateAsync(db, person2, "Course B", "2.0");
            var service = new CourseExportService(db);

            var csv = await service.ExportAllCertificatesCsvAsync();

            Assert.Contains("Person A", csv);
            Assert.Contains("Person B", csv);
        }
    }
}
