using System.Reflection;
using Nobisoft.Core.Constants;

namespace Nobisoft.Core.Extensions;

public static class GenderTypeExtensions
{
    /// <summary>
    /// Ensure that the given gender is valid string constant
    /// </summary>
    /// <param name="entity">Current entity</param>
    /// <param name="gender">Gender to be checked</param>
    /// <returns></returns>
    public static bool EnsureValue(this GenderType entity, string gender)
    {
        if (gender.IsMissing())
            return false;
        // Get type
        var type = typeof(GenderType);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields.Any(f => f.GetValue(null)?.ToString()?.Equals(gender) ?? false);
    }

    /// <summary>
    /// Get list of all genders
    /// </summary>
    /// <param name="gender"></param>
    /// <returns></returns>
    public static ICollection<string> GetGenders(this GenderType gender)
    {
        // Get type
        var type = typeof(GenderType);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields.Where(f => f.GetValue(null) != null).Select(f => f.GetValue(null)?.ToString() ?? "").ToArray();
    }

    public static ICollection<string> GetGenders()
    {
        // Get type
        var type = typeof(GenderType);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields.Where(f => f.GetValue(null) != null).Select(f => f.GetValue(null)?.ToString() ?? "").ToArray();
    }
    public static string ToValuesString()
    {
        // Get type
        var type = typeof(GenderType);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return string.Join(", ",fields.Where(f => f.GetValue(null) != null).Select(f => f.GetValue(null)?.ToString() ?? "").ToArray());
    }
}