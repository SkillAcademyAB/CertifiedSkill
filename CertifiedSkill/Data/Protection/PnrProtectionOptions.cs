namespace CertifiedSkill.Data.Protection
{
    public sealed class PnrProtectionOptions
    {
        public const string SectionName = "PnrProtection";

        public bool Enabled { get; set; }

        public string ActiveEncryptionKeyVersion { get; set; } = string.Empty;

        public string ActiveHashKeyVersion { get; set; } = string.Empty;

        public Dictionary<string, string> EncryptionKeys { get; set; } = new(StringComparer.Ordinal);

        public Dictionary<string, string> HashKeys { get; set; } = new(StringComparer.Ordinal);
    }
}
