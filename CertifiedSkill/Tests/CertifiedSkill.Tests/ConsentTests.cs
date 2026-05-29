using CertifiedSkill.Data.Participant;

namespace CertifiedSkill.Tests
{
    public class ConsentTests
    {
        [Fact]
        public void Consent_ShouldBeActive_WhenNotRevoked()
        {
            var consent = new Consent
            {
                PersonId = Guid.NewGuid(),
                ConsentType = "DataProcessing",
                ConsentVersion = "1.0"
            };

            Assert.True(consent.IsActive);
            Assert.Null(consent.RevokedAt);
        }

        [Fact]
        public void Consent_ShouldBeInactive_WhenRevoked()
        {
            var consent = new Consent
            {
                PersonId = Guid.NewGuid(),
                ConsentType = "DataProcessing",
                ConsentVersion = "1.0"
            };

            consent.Revoke(DateTimeOffset.UtcNow);

            Assert.False(consent.IsActive);
            Assert.NotNull(consent.RevokedAt);
        }

        [Fact]
        public void Consent_ShouldRecordRevokedAt_WhenRevoked()
        {
            var consent = new Consent
            {
                PersonId = Guid.NewGuid(),
                ConsentType = "DataProcessing",
                ConsentVersion = "1.0"
            };

            var revokedAt = DateTimeOffset.UtcNow;
            consent.Revoke(revokedAt);

            Assert.Equal(revokedAt, consent.RevokedAt);
        }

        [Fact]
        public void Person_ShouldHaveEmptyConsents_WhenCreated()
        {
            var person = new Person
            {
                Email = "test@example.com",
                DisplayName = "Test Person"
            };

            Assert.Empty(person.Consents);
        }
    }
}
