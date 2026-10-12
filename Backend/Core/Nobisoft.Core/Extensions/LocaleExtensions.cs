using System.Reflection;
using Nobisoft.Core.Constants;

namespace Nobisoft.Core.Extensions;

public static class LocaleExtensions
{
    /// <summary>
    /// Ensure that the given locale is valid string constant
    /// </summary>
    /// <param name="it">Current entity</param>
    /// <param name="locale">Gender to be checked</param>
    /// <returns></returns>
    public static bool EnsureValue(this Locale it, string locale)
    {
        if (locale.IsMissing())
            return false;
        // Get type
        var type = typeof(Locale);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields.Any(f => f.GetValue(null)?.ToString()?.Equals(locale) ?? false);
    }

    /// <summary>
    /// Get list of all locales
    /// </summary>
    /// <param name="locale"></param>
    /// <returns></returns>
    public static ICollection<string> GetGenders(this Locale locale)
    {
        // Get type
        var type = typeof(Locale);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields.Where(f => f.GetValue(null) != null).Select(f => f.GetValue(null)?.ToString() ?? "").ToArray();
    }

    public static ICollection<string> GetLocales()
    {
        // Get type
        var type = typeof(Locale);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields.Where(f => f.GetValue(null) != null).Select(f => f.GetValue(null)?.ToString() ?? "").ToArray();
    }

    public static string ToValuesString()
    {
        // Get type
        var type = typeof(Locale);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return string.Join(", ",fields.Where(f => f.GetValue(null) != null).Select(f => f.GetValue(null)?.ToString() ?? "").ToArray());
    }
}