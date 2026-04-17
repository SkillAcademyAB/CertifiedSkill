using System.Globalization;

namespace CertifiedSkill.Data.Identity
{
    public sealed class SwedishPersonalIdentityNumberValidator : IPersonalIdentityNumberValidator
    {
        private const int NormalizedLength = 10;

        public PersonalIdentityNumberValidationResult Validate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return PersonalIdentityNumberValidationResult.Failure(PersonalIdentityNumberValidationError.Empty);
            }

            var trimmed = input.Trim();
            if (!TryExtractDigits(trimmed, out var digitSequence))
            {
                return PersonalIdentityNumberValidationResult.Failure(PersonalIdentityNumberValidationError.InvalidFormat);
            }

            if (!TryNormalizeToInternalFormat(digitSequence, out var normalized))
            {
                return PersonalIdentityNumberValidationResult.Failure(PersonalIdentityNumberValidationError.InvalidFormat);
            }

            if (!IsValidDatePart(digitSequence))
            {
                return PersonalIdentityNumberValidationResult.Failure(PersonalIdentityNumberValidationError.InvalidDate);
            }

            if (!HasValidChecksum(normalized))
            {
                return PersonalIdentityNumberValidationResult.Failure(PersonalIdentityNumberValidationError.InvalidChecksum);
            }

            return PersonalIdentityNumberValidationResult.Success(normalized);
        }

        public bool TryNormalize(string? input, out string normalizedValue)
        {
            var result = Validate(input);
            normalizedValue = result.NormalizedValue ?? string.Empty;
            return result.IsValid;
        }

        private static bool TryExtractDigits(string input, out string digits)
        {
            digits = string.Empty;

            if (input.Length == 10 && IsDigitsOnly(input))
            {
                digits = input;
                return true;
            }

            if (input.Length == 11
                && IsDigitsOnly(input[..6])
                && IsSeparator(input[6])
                && IsDigitsOnly(input[7..]))
            {
                digits = input[..6] + input[7..];
                return true;
            }

            if (input.Length == 12 && IsDigitsOnly(input))
            {
                digits = input;
                return true;
            }

            if (input.Length == 13
                && IsDigitsOnly(input[..8])
                && IsSeparator(input[8])
                && IsDigitsOnly(input[9..]))
            {
                digits = input[..8] + input[9..];
                return true;
            }

            return false;
        }

        private static bool TryNormalizeToInternalFormat(string digitSequence, out string normalized)
        {
            normalized = string.Empty;

            if (digitSequence.Length == NormalizedLength)
            {
                normalized = digitSequence;
                return true;
            }

            if (digitSequence.Length == 12)
            {
                normalized = digitSequence[2..];
                return true;
            }

            return false;
        }

        private static bool IsValidDatePart(string digitSequence)
        {
            if (digitSequence.Length == 12)
            {
                return DateOnly.TryParseExact(digitSequence[..8], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
            }

            if (digitSequence.Length == NormalizedLength)
            {
                var shortDate = digitSequence[..6];
                var year = int.Parse(shortDate[..2], CultureInfo.InvariantCulture);
                var month = int.Parse(shortDate.Substring(2, 2), CultureInfo.InvariantCulture);
                var day = int.Parse(shortDate.Substring(4, 2), CultureInfo.InvariantCulture);

                return DateOnly.TryParseExact($"19{year:00}{month:00}{day:00}", "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    || DateOnly.TryParseExact($"20{year:00}{month:00}{day:00}", "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
            }

            return false;
        }

        private static bool HasValidChecksum(string normalizedTenDigits)
        {
            var sum = 0;
            for (var i = 0; i < 9; i++)
            {
                var digit = normalizedTenDigits[i] - '0';
                var factor = i % 2 == 0 ? 2 : 1;
                var product = digit * factor;
                sum += product > 9 ? product - 9 : product;
            }

            var controlDigit = (10 - (sum % 10)) % 10;
            return controlDigit == normalizedTenDigits[9] - '0';
        }

        private static bool IsDigitsOnly(string value)
        {
            foreach (var ch in value)
            {
                if (!char.IsAsciiDigit(ch))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsSeparator(char separator) => separator is '-' or '+';
    }
}
