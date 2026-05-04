using CertifiedSkill.Data.Protection;
using CertifiedSkill.Services.Pnr;
using Microsoft.Extensions.Options;

namespace CertifiedSkill.Tests
{
    public class AesPnrProtectionServiceTests
    {
        private static AesPnrProtectionService CreateService(bool enabled = true)
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

            return new AesPnrProtectionService(Options.Create(options));
        }

        [Fact]
        public void Encrypt_ShouldReturnEncryptedValue_AndActiveKeyVersion()
        {
            var service = CreateService();

            var (encryptedValue, keyVersion) = service.Encrypt("199001011234");

            Assert.False(string.IsNullOrEmpty(encryptedValue));
            Assert.Equal("2026-04", keyVersion);
        }

        [Fact]
        public void Encrypt_ShouldNotContainPlaintextPnr()
        {
            var service = CreateService();
            var pnr = "199001011234";

            var (encryptedValue, _) = service.Encrypt(pnr);

            Assert.DoesNotContain(pnr, encryptedValue);
        }

        [Fact]
        public void Encrypt_ShouldProduceDifferentCiphertexts_ForSamePnr()
        {
            var service = CreateService();
            var pnr = "199001011234";

            var (first, _) = service.Encrypt(pnr);
            var (second, _) = service.Encrypt(pnr);

            // AES-GCM uses random nonce so same plaintext gives different ciphertext
            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Decrypt_ShouldReturnOriginalPnr_AfterEncrypt()
        {
            var service = CreateService();
            var pnr = "199001011234";

            var (encryptedValue, keyVersion) = service.Encrypt(pnr);
            var decrypted = service.Decrypt(encryptedValue, keyVersion);

            Assert.Equal(pnr, decrypted);
        }

        [Fact]
        public void Decrypt_ShouldThrow_WhenEncryptedValueIsTampered()
        {
            var service = CreateService();
            var (encryptedValue, keyVersion) = service.Encrypt("199001011234");

            // Tamper with the ciphertext
            var bytes = Convert.FromBase64String(encryptedValue);
            bytes[15] ^= 0xFF;
            var tampered = Convert.ToBase64String(bytes);

            Assert.ThrowsAny<Exception>(() => service.Decrypt(tampered, keyVersion));
        }

        [Fact]
        public void ComputeHash_ShouldReturnSameHash_ForSamePnr()
        {
            var service = CreateService();
            var pnr = "199001011234";

            var first = service.ComputeHash(pnr);
            var second = service.ComputeHash(pnr);

            Assert.Equal(first, second);
        }

        [Fact]
        public void ComputeHash_ShouldReturnDifferentHash_ForDifferentPnr()
        {
            var service = CreateService();

            var hash1 = service.ComputeHash("199001011234");
            var hash2 = service.ComputeHash("198505052345");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void ComputeHash_ShouldNotContainPlaintextPnr()
        {
            var service = CreateService();
            var pnr = "199001011234";

            var hash = service.ComputeHash(pnr);

            Assert.DoesNotContain(pnr, hash);
        }

        [Fact]
        public void Encrypt_ShouldThrow_WhenProtectionIsDisabled()
        {
            var service = CreateService(enabled: false);

            Assert.Throws<InvalidOperationException>(() => service.Encrypt("199001011234"));
        }

        [Fact]
        public void ComputeHash_ShouldThrow_WhenProtectionIsDisabled()
        {
            var service = CreateService(enabled: false);

            Assert.Throws<InvalidOperationException>(() => service.ComputeHash("199001011234"));
        }
    }
}
