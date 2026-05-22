using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MultiTenantSaaS.Shared.Utilities;

public static partial class SlugGenerator
{
    public static string From(string value, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required to generate a slug.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category is UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        }

        var slug = NonAlphanumericOrHyphenRegex().Replace(builder.ToString(), "-");
        slug = MultiHyphenRegex().Replace(slug, "-").Trim('-');

        if (slug.Length == 0)
        {
            slug = "tenant";
        }

        return slug.Length <= maxLength ? slug : slug[..maxLength].TrimEnd('-');
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonAlphanumericOrHyphenRegex();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultiHyphenRegex();
}
