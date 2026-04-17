using CertifiedSkill.Data.Identity;

namespace CertifiedSkill.Tests;

public class SwedishPersonalIdentityNumberValidatorTests
{
    private readonly SwedishPersonalIdentityNumberValidator validator = new();

    [Theory]
    [InlineData("900101-0017", "9001010017")]
    [InlineData("900101+0017", "9001010017")]
    [InlineData("9001010017", "9001010017")]
    [InlineData("19900101-0017", "9001010017")]
    [InlineData("199001010017", "9001010017")]
    [InlineData("  19900101-0017  ", "9001010017")]
    [InlineData("000229-1235", "0002291235")]
    public void Validate_ShouldAccept_ValidFormatsAndChecksums(string input, string expectedNormalized)
    {
        var result = validator.Validate(input);

        Assert.True(result.IsValid);
        Assert.Equal(expectedNormalized, result.NormalizedValue);
        Assert.Equal(PersonalIdentityNumberValidationError.None, result.Error);
    }

    [Theory]
    [InlineData(null, PersonalIdentityNumberValidationError.Empty)]
    [InlineData("", PersonalIdentityNumberValidationError.Empty)]
    [InlineData("   ", PersonalIdentityNumberValidationError.Empty)]
    [InlineData("900101-0018", PersonalIdentityNumberValidationError.InvalidChecksum)]
    [InlineData("900132-0017", PersonalIdentityNumberValidationError.InvalidDate)]
    [InlineData("900100-0017", PersonalIdentityNumberValidationError.InvalidDate)]
    [InlineData("19900101-0018", PersonalIdentityNumberValidationError.InvalidChecksum)]
    [InlineData("19901301-0017", PersonalIdentityNumberValidationError.InvalidDate)]
    [InlineData("19900101-001", PersonalIdentityNumberValidationError.InvalidFormat)]
    [InlineData("19900101--0017", PersonalIdentityNumberValidationError.InvalidFormat)]
    [InlineData("19900101/0017", PersonalIdentityNumberValidationError.InvalidFormat)]
    [InlineData("900101A017", PersonalIdentityNumberValidationError.InvalidFormat)]
    [InlineData("90010100178", PersonalIdentityNumberValidationError.InvalidFormat)]
    public void Validate_ShouldReject_InvalidInput(string? input, PersonalIdentityNumberValidationError expectedError)
    {
        var result = validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Null(result.NormalizedValue);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public void TryNormalize_ShouldReturnFalse_AndEmptyString_ForInvalidInput()
    {
        var isValid = validator.TryNormalize("900101-0018", out var normalized);

        Assert.False(isValid);
        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void TryNormalize_ShouldReturnNormalizedTenDigitValue_ForValidInput()
    {
        var isValid = validator.TryNormalize("19900101-0017", out var normalized);

        Assert.True(isValid);
        Assert.Equal("9001010017", normalized);
    }
}
