using CertifiedSkill.Services.Participant;

namespace CertifiedSkill.Tests
{
    public class BankIdSandboxTests
    {
        [Fact]
        public async Task InitiateAuthAsync_ShouldReturnSessionToken()
        {
            var provider = new SandboxBankIdIdentityProvider();

            var token = await provider.InitiateAuthAsync("199001010017");

            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task PollAuthAsync_ShouldReturnComplete_WhenSessionExists()
        {
            var provider = new SandboxBankIdIdentityProvider();

            var token = await provider.InitiateAuthAsync("199001010017");
            var result = await provider.PollAuthAsync(token);

            Assert.Equal(BankIdAuthStatus.Complete, result.Status);
            Assert.Equal("199001010017", result.SubjectId);
        }

        [Fact]
        public async Task PollAuthAsync_ShouldReturnFailed_WhenSessionDoesNotExist()
        {
            var provider = new SandboxBankIdIdentityProvider();

            var result = await provider.PollAuthAsync("invalid-token");

            Assert.Equal(BankIdAuthStatus.Failed, result.Status);
            Assert.Null(result.SubjectId);
        }

        [Fact]
        public async Task InitiateAuthAsync_ShouldReturnUniqueTokens()
        {
            var provider = new SandboxBankIdIdentityProvider();

            var token1 = await provider.InitiateAuthAsync("199001010017");
            var token2 = await provider.InitiateAuthAsync("199001010017");

            Assert.NotEqual(token1, token2);
        }
    }
}
