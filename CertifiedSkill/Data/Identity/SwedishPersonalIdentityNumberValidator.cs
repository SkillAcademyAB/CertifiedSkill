using System.Globalization;

namespace CertifiedSkill.Data.Identity
{
    public sealed class SwedishPersonalIdentityNumberValidator : IPersonalIdentityNumberValidator
    {
        private const int CanonicalLength = 12;
        private const int ChecksumLength = 10;

        public PersonalIdentityNumberValidationResult Validate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return PersonalIdentityNumberValidationResult.Failure(PersonalIdentityNumberValidationError.Empty);
            }

            var trimmed = input.Trim();
            if (!TryExtractDigits(trimmed, out var digitSequence, out var separator))
            {
                return PersonalIdentityNumberValidationResult.Failure(PersonalIdentityNumberValidationError.InvalidFormat);
            }

            if (!TryNormalizeToCanonicalFormat(digitSequence, separator, out var normalized))
            {
                return PersonalIdentityNumberValidationResult.Failure(PersonalIdentityNumberValidationError.InvalidDate);
            }

            if (!HasValidChecksum(normalized[^ChecksumLength..]))
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

        private static bool TryExtractDigits(string input, out string digits, out char? separator)
        {
            digits = string.Empty;
            separator = null;

            if (input.Length == ChecksumLength && IsDigitsOnly(input))
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
                separator = input[6];
                return true;
            }

            if (input.Length == CanonicalLength && IsDigitsOnly(input))
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
                separator = input[8];
                return true;
            }

            return false;
        }

        private static bool TryNormalizeToCanonicalFormat(string digitSequence, char? separator, out string normalized)
        {
            normalized = string.Empty;

            if (digitSequence.Length == CanonicalLength)
            {
                if (!IsValidDatePart(digitSequence[..8]))
                {
                    return false;
                }

                normalized = digitSequence;
                return true;
            }

            if (digitSequence.Length == ChecksumLength)
            {
                var century = ResolveCenturyPrefix(digitSequence[..6], separator);
                var candidate = $"{century}{digitSequence}";
                if (!IsValidDatePart(candidate[..8]))
                {
                    return false;
                }

                normalized = candidate;
                return true;
            }

            return false;
        }

        private static string ResolveCenturyPrefix(string shortDatePart, char? separator)
        {
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var yearPart = int.Parse(shortDatePart[..2], CultureInfo.InvariantCulture);

            if (separator == '+')
            {
                var maxYearForPlus = currentDate.Year - 100;
                var century = currentDate.Year / 100 - 1;
                var fullYear = century * 100 + yearPart;
                if (fullYear > maxYearForPlus)
                {
                    fullYear -= 100;
                }

                return (fullYear / 100).ToString("00", CultureInfo.InvariantCulture);
            }

            var currentCentury = currentDate.Year / 100;
            var guessedFullYear = currentCentury * 100 + yearPart;
            if (guessedFullYear > currentDate.Year)
            {
                guessedFullYear -= 100;
            }

            return (guessedFullYear / 100).ToString("00", CultureInfo.InvariantCulture);
        }

        private static bool IsValidDatePart(string datePart) =>
            DateOnly.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

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
