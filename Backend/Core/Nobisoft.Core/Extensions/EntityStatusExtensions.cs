using System.Reflection;
using Nobisoft.Core.Constants;

namespace Nobisoft.Core.Extensions;

public static class EntityStatusExtensions
{
    /// <summary>
    /// Ensure that the given status is valid string constant
    /// </summary>
    /// <param name="entity">Current entity</param>
    /// <param name="status">Status to be checked</param>
    /// <returns></returns>
    public static bool EnsureValue(this EntityStatus entity, string status)
    {
        if (status.IsMissing())
            return false;
        // Get type
        var type = typeof(EntityStatus);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields.Any(f => f.GetValue(null)?.ToString()?.Equals(status) ?? false);
    }

    /// <summary>
    /// Get list of all status
    /// </summary>
    /// <param name="entityStatus"></param>
    /// <returns></returns>
    public static ICollection<string> GetStatus(this EntityStatus entityStatus)
    {
        // Get type
        var type = typeof(EntityStatus);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields.Where(f => f.GetValue(null) != null).Select(f => f.GetValue(null)?.ToString() ?? "").ToArray();
    }
    public static ICollection<string> GetStatus()
    {
        // Get type
        var type = typeof(EntityStatus);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields.Where(f => f.GetValue(null) != null).Select(f => f.GetValue(null)?.ToString() ?? "").ToArray();
    }
    public static string ToValuesString()
    {
        // Get type
        var type = typeof(EntityStatus);
        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        return string.Join(", ",fields.Where(f => f.GetValue(null) != null).Select(f => f.GetValue(null)?.ToString() ?? "").ToArray());
    }

}