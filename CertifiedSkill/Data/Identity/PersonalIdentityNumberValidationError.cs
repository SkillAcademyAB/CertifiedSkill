namespace CertifiedSkill.Data.Identity
{
    public enum PersonalIdentityNumberValidationError
    {
        None = 0,
        Empty = 1,
        InvalidFormat = 2,
        InvalidDate = 3,
        InvalidChecksum = 4
    }
}
