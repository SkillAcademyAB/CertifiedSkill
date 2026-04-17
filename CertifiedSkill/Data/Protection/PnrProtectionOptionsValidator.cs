using Microsoft.Extensions.Options;

namespace CertifiedSkill.Data.Protection
{
    public sealed class PnrProtectionOptionsValidator : IValidateOptions<PnrProtectionOptions>
    {
        private const int MinimumKeyBytes = 32;

        public ValidateOptionsResult Validate(string? name, PnrProtectionOptions options)
        {
            if (!string.IsNullOrEmpty(name) && name != Options.DefaultName)
            {
                return ValidateOptionsResult.Skip;
            }

            if (!options.Enabled)
            {
                return ValidateOptionsResult.Success;
            }

            var failures = new List<string>();

            ValidateActiveVersion(
                options.ActiveEncryptionKeyVersion,
                options.EncryptionKeys,
                "encryption",
                failures);

            ValidateActiveVersion(
                options.ActiveHashKeyVersion,
                options.HashKeys,
                "hash",
                failures);

            ValidateKeyMaterial(options.EncryptionKeys, "encryption", failures);
            ValidateKeyMaterial(options.HashKeys, "hash", failures);

            return failures.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(failures);
        }

        private static void ValidateActiveVersion(
            string activeVersion,
            IReadOnlyDictionary<string, string> keys,
            string keyType,
            ICollection<string> failures)
        {
            if (keys.Count == 0)
            {
                failures.Add($"PnrProtection requires at least one configured {keyType} key when enabled.");
                return;
            }

            if (string.IsNullOrWhiteSpace(activeVersion))
            {
                failures.Add($"PnrProtection requires an active {keyType} key version when enabled.");
                return;
            }

            if (!keys.ContainsKey(activeVersion))
            {
                failures.Add($"PnrProtection active {keyType} key version '{activeVersion}' is not configured.");
            }
        }

        private static void ValidateKeyMaterial(
            IReadOnlyDictionary<string, string> keys,
            string keyType,
            ICollection<string> failures)
        {
            foreach (var (version, value) in keys)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    failures.Add($"PnrProtection {keyType} key version '{version}' is empty.");
                    continue;
                }

                try
                {
                    var keyBytes = Convert.FromBase64String(value);
                    if (keyBytes.Length < MinimumKeyBytes)
                    {
                        failures.Add($"PnrProtection {keyType} key version '{version}' must be at least {MinimumKeyBytes} bytes.");
                    }
                }
                catch (FormatException)
                {
                    failures.Add($"PnrProtection {keyType} key version '{version}' is not valid base64.");
                }
            }
        }
    }
}
