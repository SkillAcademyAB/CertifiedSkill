using CertifiedSkill.Data.Protection;
using Microsoft.Extensions.Options;

namespace CertifiedSkill.Tests
{
    public class PnrProtectionOptionsValidatorTests
    {
        private readonly PnrProtectionOptionsValidator validator = new();

        [Fact]
        public void Validate_ShouldAllowDisabledProtection_WithoutConfiguredKeys()
        {
            var options = new PnrProtectionOptions
            {
                Enabled = false
            };

            var result = validator.Validate(Options.DefaultName, options);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public void Validate_ShouldRejectEnabledProtection_WhenActiveEncryptionKeyVersionIsMissing()
        {
            var options = CreateValidOptions();
            options.ActiveEncryptionKeyVersion = string.Empty;

            var result = validator.Validate(Options.DefaultName, options);

            Assert.False(result.Succeeded);
            var failures = Assert.IsAssignableFrom<IEnumerable<string>>(result.Failures);
            Assert.Contains(failures, message => message.Contains("active encryption key version", StringComparison.Ordinal));
        }

        [Fact]
        public void Validate_ShouldRejectEnabledProtection_WhenActiveHashKeyIsNotConfigured()
        {
            var options = CreateValidOptions();
            options.ActiveHashKeyVersion = "2026-05";

            var result = validator.Validate(Options.DefaultName, options);

            Assert.False(result.Succeeded);
            var failures = Assert.IsAssignableFrom<IEnumerable<string>>(result.Failures);
            Assert.Contains(failures, message => message.Contains("2026-05", StringComparison.Ordinal));
        }

        [Fact]
        public void Validate_ShouldRejectEnabledProtection_WhenKeyMaterialIsNotBase64()
        {
            var options = CreateValidOptions();
            options.EncryptionKeysConfiguration = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["2026-04"] = "not-base64"
            };

            var result = validator.Validate(Options.DefaultName, options);

            Assert.False(result.Succeeded);
            var failures = Assert.IsAssignableFrom<IEnumerable<string>>(result.Failures);
            Assert.Contains(failures, message => message.Contains("not valid base64", StringComparison.Ordinal));
        }

        [Fact]
        public void Validate_ShouldRejectEnabledProtection_WhenKeyMaterialIsTooShort()
        {
            var options = CreateValidOptions();
            options.HashKeysConfiguration = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["2026-04"] = Convert.ToBase64String("too-short-secret"u8.ToArray())
            };

            var result = validator.Validate(Options.DefaultName, options);

            Assert.False(result.Succeeded);
            var failures = Assert.IsAssignableFrom<IEnumerable<string>>(result.Failures);
            Assert.Contains(failures, message => message.Contains("at least 32 bytes", StringComparison.Ordinal));
        }

        [Fact]
        public void Validate_ShouldAllowEnabledProtection_WhenActiveVersionsAndKeyMaterialAreValid()
        {
            var options = CreateValidOptions();

            var result = validator.Validate(Options.DefaultName, options);

            Assert.True(result.Succeeded);
        }

        private static PnrProtectionOptions CreateValidOptions()
        {
            var keyMaterial = Convert.ToBase64String(new byte[32]);

            return new PnrProtectionOptions
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
        }
    }
}
