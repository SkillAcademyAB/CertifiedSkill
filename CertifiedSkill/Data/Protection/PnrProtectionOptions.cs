using Microsoft.Extensions.Configuration;

namespace CertifiedSkill.Data.Protection
{
    public sealed class PnrProtectionOptions
    {
        private Dictionary<string, string> encryptionKeys = new(StringComparer.Ordinal);
        private Dictionary<string, string> hashKeys = new(StringComparer.Ordinal);

        public const string SectionName = "PnrProtection";

        public bool Enabled { get; set; }

        public string ActiveEncryptionKeyVersion { get; set; } = string.Empty;

        public string ActiveHashKeyVersion { get; set; } = string.Empty;

        public IReadOnlyDictionary<string, string> EncryptionKeys => encryptionKeys;

        public IReadOnlyDictionary<string, string> HashKeys => hashKeys;

        [ConfigurationKeyName(nameof(EncryptionKeys))]
        public Dictionary<string, string> EncryptionKeysConfiguration
        {
            get => encryptionKeys;
            set => encryptionKeys = CloneDictionary(value);
        }

        [ConfigurationKeyName(nameof(HashKeys))]
        public Dictionary<string, string> HashKeysConfiguration
        {
            get => hashKeys;
            set => hashKeys = CloneDictionary(value);
        }

        private static Dictionary<string, string> CloneDictionary(IDictionary<string, string>? source) =>
            source is null
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                : new Dictionary<string, string>(source, StringComparer.Ordinal);
    }
}
