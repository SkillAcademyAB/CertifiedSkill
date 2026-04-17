using CertifiedSkill.Data;
using CertifiedSkill.Data.Participant;
using CertifiedSkill.Services.Participant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertifiedSkill.Tests
{
    public class ParticipantMagicLinkServiceTests : IDisposable
    {
        private readonly ApplicationDbContext db;
        private readonly FakeParticipantEmailSender emailSender;
        private readonly FakeTimeProvider timeProvider;
        private readonly ParticipantMagicLinkService service;

        public ParticipantMagicLinkServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            db = new ApplicationDbContext(options);
            emailSender = new FakeParticipantEmailSender();
            timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
            service = new ParticipantMagicLinkService(
                db,
                emailSender,
                NullLogger<ParticipantMagicLinkService>.Instance,
                timeProvider);
        }

        public void Dispose() => db.Dispose();

        [Fact]
        public async Task RequestMagicLink_ShouldPersistToken_AndSendEmail()
        {
            var result = await service.RequestMagicLinkAsync("User@Example.COM", "https://example.com/verify");

            Assert.Equal(MagicLinkRequestResult.Sent, result);
            Assert.Single(emailSender.SentLinks);
            Assert.Contains("https://example.com/verify", emailSender.SentLinks[0].Url);
            Assert.Equal("user@example.com", emailSender.SentLinks[0].Email);

            var tokens = await db.ParticipantMagicLinkTokens.ToListAsync();
            Assert.Single(tokens);
            Assert.Equal("user@example.com", tokens[0].Email);
            Assert.False(tokens[0].IsUsed);
            Assert.False(tokens[0].IsExpired(timeProvider.GetUtcNow()));
        }

        [Fact]
        public async Task RequestMagicLink_ShouldNormalizeEmail()
        {
            await service.RequestMagicLinkAsync("  TEST@EXAMPLE.COM  ", "https://example.com/verify");

            var token = await db.ParticipantMagicLinkTokens.SingleAsync();
            Assert.Equal("test@example.com", token.Email);
            Assert.Equal("test@example.com", emailSender.SentLinks[0].Email);
        }

        [Fact]
        public async Task RequestMagicLink_ShouldEnforceRateLimit_AfterThreeRequests()
        {
            const string email = "test@example.com";
            const string baseUrl = "https://example.com/verify";

            var result1 = await service.RequestMagicLinkAsync(email, baseUrl);
            var result2 = await service.RequestMagicLinkAsync(email, baseUrl);
            var result3 = await service.RequestMagicLinkAsync(email, baseUrl);
            var result4 = await service.RequestMagicLinkAsync(email, baseUrl);

            Assert.Equal(MagicLinkRequestResult.Sent, result1);
            Assert.Equal(MagicLinkRequestResult.Sent, result2);
            Assert.Equal(MagicLinkRequestResult.Sent, result3);
            Assert.Equal(MagicLinkRequestResult.RateLimited, result4);
            Assert.Equal(3, emailSender.SentLinks.Count);
        }

        [Fact]
        public async Task RequestMagicLink_ShouldNotRateLimit_AfterWindowExpires()
        {
            const string email = "test@example.com";
            const string baseUrl = "https://example.com/verify";

            await service.RequestMagicLinkAsync(email, baseUrl);
            await service.RequestMagicLinkAsync(email, baseUrl);
            await service.RequestMagicLinkAsync(email, baseUrl);

            // Advance time past the rate limit window
            timeProvider.Advance(TimeSpan.FromMinutes(16));

            var result = await service.RequestMagicLinkAsync(email, baseUrl);

            Assert.Equal(MagicLinkRequestResult.Sent, result);
        }

        [Fact]
        public async Task ConsumeToken_ShouldSucceed_WithValidToken()
        {
            await service.RequestMagicLinkAsync("test@example.com", "https://example.com/verify");
            var rawToken = ExtractTokenFromUrl(emailSender.SentLinks[0].Url);

            var (result, email) = await service.ConsumeTokenAsync(rawToken);

            Assert.Equal(MagicLinkConsumeResult.Success, result);
            Assert.Equal("test@example.com", email);
        }

        [Fact]
        public async Task ConsumeToken_ShouldMarkTokenAsUsed_AfterConsumption()
        {
            await service.RequestMagicLinkAsync("test@example.com", "https://example.com/verify");
            var rawToken = ExtractTokenFromUrl(emailSender.SentLinks[0].Url);

            await service.ConsumeTokenAsync(rawToken);

            var token = await db.ParticipantMagicLinkTokens.SingleAsync();
            Assert.True(token.IsUsed);
        }

        [Fact]
        public async Task ConsumeToken_ShouldReturnAlreadyUsed_WhenTokenConsumedTwice()
        {
            await service.RequestMagicLinkAsync("test@example.com", "https://example.com/verify");
            var rawToken = ExtractTokenFromUrl(emailSender.SentLinks[0].Url);

            await service.ConsumeTokenAsync(rawToken);
            var (result, email) = await service.ConsumeTokenAsync(rawToken);

            Assert.Equal(MagicLinkConsumeResult.AlreadyUsed, result);
            Assert.Null(email);
        }

        [Fact]
        public async Task ConsumeToken_ShouldReturnExpired_WhenTokenIsExpired()
        {
            await service.RequestMagicLinkAsync("test@example.com", "https://example.com/verify");
            var rawToken = ExtractTokenFromUrl(emailSender.SentLinks[0].Url);

            // Advance past expiry
            timeProvider.Advance(TimeSpan.FromMinutes(16));

            var (result, email) = await service.ConsumeTokenAsync(rawToken);

            Assert.Equal(MagicLinkConsumeResult.Expired, result);
            Assert.Null(email);
        }

        [Fact]
        public async Task ConsumeToken_ShouldReturnInvalid_ForUnknownToken()
        {
            var (result, email) = await service.ConsumeTokenAsync("nonexistent-token-value");

            Assert.Equal(MagicLinkConsumeResult.Invalid, result);
            Assert.Null(email);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ConsumeToken_ShouldReturnInvalid_ForNullOrWhitespace(string? rawToken)
        {
            var (result, email) = await service.ConsumeTokenAsync(rawToken!);

            Assert.Equal(MagicLinkConsumeResult.Invalid, result);
            Assert.Null(email);
        }

        private static string ExtractTokenFromUrl(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return Uri.UnescapeDataString(query["token"] ?? string.Empty);
        }

        private sealed class FakeParticipantEmailSender : IParticipantEmailSender
        {
            public List<(string Email, string Url)> SentLinks { get; } = new();

            public Task SendMagicLinkAsync(string email, string magicLinkUrl, CancellationToken cancellationToken = default)
            {
                SentLinks.Add((email, magicLinkUrl));
                return Task.CompletedTask;
            }
        }

        private sealed class FakeTimeProvider(DateTimeOffset initialTime) : TimeProvider
        {
            private DateTimeOffset _current = initialTime;

            public override DateTimeOffset GetUtcNow() => _current;

            public void Advance(TimeSpan delta) => _current = _current.Add(delta);
        }
    }
}
