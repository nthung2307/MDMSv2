using System.Reflection;
using System.Text.Json;
using PropertyInfo = Nobisoft.Core.ValueObjects.PropertyInfo;

namespace Nobisoft.Core.Extensions;

public static class TypeExtensions
{
    public static ICollection<PropertyInfo> GetClassPropertiesInfo(this Type classType, IReadOnlyCollection<string> blacklistField)
    {
        var propertiesInfo = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var filteredPropertiesInfo = propertiesInfo.Where(x => blacklistField.All(y => y != x.Name)).Select(x => new PropertyInfo()
        {
            Name = x.Name,
            Value = x.PropertyType.IsEnum ? JsonSerializer.Serialize(x.PropertyType.GetFields().Where(fi => fi.IsLiteral).Select(fi => new {fi.Name, Value = fi.GetRawConstantValue()}).ToList()) : null,
            DataType = x.PropertyType.GenericTypeArguments.Any() ? x.PropertyType.GenericTypeArguments.First().Name : x.PropertyType.Name
        }).ToList();
        return filteredPropertiesInfo;
    }
}