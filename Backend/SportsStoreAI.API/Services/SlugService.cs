using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SportsStoreAI.API.Services;

public static partial class SlugService
{
    public static string Create(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var result = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        result = InvalidCharacters().Replace(result, "-");
        return result.Trim('-');
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex InvalidCharacters();
}
