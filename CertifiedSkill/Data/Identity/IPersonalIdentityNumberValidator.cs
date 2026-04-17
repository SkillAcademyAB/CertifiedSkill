namespace CertifiedSkill.Data.Identity
{
    public interface IPersonalIdentityNumberValidator
    {
        PersonalIdentityNumberValidationResult Validate(string? input);

        bool TryNormalize(string? input, out string normalizedValue);
    }
}
