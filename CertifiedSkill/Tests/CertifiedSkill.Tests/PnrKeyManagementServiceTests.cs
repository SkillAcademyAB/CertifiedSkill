using CertifiedSkill.Data.Protection;
using CertifiedSkill.Services.Pnr;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CertifiedSkill.Tests
{
    public class PnrKeyManagementServiceTests
    {
        private static PnrKeyManagementService CreateService(bool enabled = true)
        {
            var keyMaterial = Convert.ToBase64String(new byte[32]);
            var options = new PnrProtectionOptions
            {
                Enabled = enabled,
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
            return new PnrKeyManagementService(
                Options.Create(options),
                NullLogger<PnrKeyManagementService>.Instance);
        }

        [Fact]
        public void IsEnabled_ShouldReturnTrue_WhenProtectionEnabled()
        {
            var service = CreateService(enabled: true);
            Assert.True(service.IsEnabled);
        }

        [Fact]
        public void IsEnabled_ShouldReturnFalse_WhenProtectionDisabled()
        {
            var service = CreateService(enabled: false);
            Assert.False(service.IsEnabled);
        }

        [Fact]
        public void ActiveEncryptionKeyVersion_ShouldReturnConfiguredVersion()
        {
            var service = CreateService();
            Assert.Equal("2026-04", service.ActiveEncryptionKeyVersion);
        }

        [Fact]
        public void ActiveHashKeyVersion_ShouldReturnConfiguredVersion()
        {
            var service = CreateService();
            Assert.Equal("2026-04", service.ActiveHashKeyVersion);
        }

        [Fact]
        public void GenerateDevKey_ShouldReturn32ByteBase64Key()
        {
            var key = PnrKeyManagementService.GenerateDevKey();
            var keyBytes = Convert.FromBase64String(key);
            Assert.Equal(32, keyBytes.Length);
        }

        [Fact]
        public void GenerateDevKey_ShouldReturnUniqueKeys()
        {
            var key1 = PnrKeyManagementService.GenerateDevKey();
            var key2 = PnrKeyManagementService.GenerateDevKey();
            Assert.NotEqual(key1, key2);
        }
    }
}
