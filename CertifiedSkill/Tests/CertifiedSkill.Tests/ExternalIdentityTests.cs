using CertifiedSkill.Data.Participant;

namespace CertifiedSkill.Tests
{
    public class ExternalIdentityTests
    {
        [Fact]
        public void ExternalIdentity_ShouldHaveProvider_WhenCreated()
        {
            var identity = new ExternalIdentity
            {
                PersonId = Guid.NewGuid(),
                Provider = "BankID",
                ProviderSubjectId = "198507102391"
            };

            Assert.Equal("BankID", identity.Provider);
        }

        [Fact]
        public void ExternalIdentity_ShouldHaveProviderSubjectId_WhenCreated()
        {
            var identity = new ExternalIdentity
            {
                PersonId = Guid.NewGuid(),
                Provider = "BankID",
                ProviderSubjectId = "198507102391"
            };

            Assert.Equal("198507102391", identity.ProviderSubjectId);
        }

        [Fact]
        public void ExternalIdentity_ShouldSetLinkedAt_WhenCreated()
        {
            var before = DateTimeOffset.UtcNow;

            var identity = new ExternalIdentity
            {
                PersonId = Guid.NewGuid(),
                Provider = "BankID",
                ProviderSubjectId = "198507102391"
            };

            Assert.True(identity.LinkedAt >= before);
        }

        [Fact]
        public void Person_ShouldHaveEmptyExternalIdentities_WhenCreated()
        {
            var person = new Person
            {
                Email = "test@example.com",
                DisplayName = "Test Person"
            };

            Assert.Empty(person.ExternalIdentities);
        }
    }
}
