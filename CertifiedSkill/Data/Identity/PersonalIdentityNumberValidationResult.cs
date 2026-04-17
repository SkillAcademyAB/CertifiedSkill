namespace CertifiedSkill.Data.Identity
{
    public readonly record struct PersonalIdentityNumberValidationResult(
        bool IsValid,
        string? NormalizedValue,
        PersonalIdentityNumberValidationError Error)
    {
        public static PersonalIdentityNumberValidationResult Success(string normalizedValue) =>
            new(true, normalizedValue, PersonalIdentityNumberValidationError.None);

        public static PersonalIdentityNumberValidationResult Failure(PersonalIdentityNumberValidationError error) =>
            new(false, null, error);
    }
}
