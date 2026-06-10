using CertifiedSkill.Data.Protection;
using Microsoft.Extensions.Options;

namespace CertifiedSkill.Services.Pnr
{
    /// <summary>
    /// Provides helper utilities for PNR key management.
    /// Handles startup validation and development key generation.
    /// In production, keys must be stored in Azure Key Vault.
    /// </summary>
    public sealed class PnrKeyManagementService
    {
        private readonly PnrProtectionOptions _options;
        private readonly ILogger<PnrKeyManagementService> _logger;

        public PnrKeyManagementService(
            IOptions<PnrProtectionOptions> options,
            ILogger<PnrKeyManagementService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Returns true if PNR protection is enabled and configured.
        /// </summary>
        public bool IsEnabled => _options.Enabled;

        /// <summary>
        /// Returns the active encryption key version.
        /// </summary>
        public string ActiveEncryptionKeyVersion => _options.ActiveEncryptionKeyVersion;

        /// <summary>
        /// Returns the active hash key version.
        /// </summary>
        public string ActiveHashKeyVersion => _options.ActiveHashKeyVersion;

        /// <summary>
        /// Logs the current key configuration status without exposing key material.
        /// Safe to call on startup for audit purposes.
        /// </summary>
        public void LogKeyStatus()
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation(
                    "PNR protection is disabled. No key material loaded.");
                return;
            }

            _logger.LogInformation(
                "PNR protection is enabled. " +
                "Encryption keys configured: {EncryptionKeyCount}. " +
                "Hash keys configured: {HashKeyCount}. " +
                "Active encryption version: {ActiveEncryptionVersion}. " +
                "Active hash version: {ActiveHashVersion}.",
                _options.EncryptionKeys.Count,
                _options.HashKeys.Count,
                _options.ActiveEncryptionKeyVersion,
                _options.ActiveHashKeyVersion);
        }

        /// <summary>
        /// Generates a new random base64-encoded key suitable for use as
        /// PNR encryption or hash key material. For development use only.
        /// Never use generated keys directly in production.
        /// </summary>
        public static string GenerateDevKey()
        {
            var keyBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(keyBytes);
        }
    }
}
