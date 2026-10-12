namespace Nobisoft.Core.Extensions;

public static class GuidExtensions
{
    public static string ToCleanString(this Guid guid)
    {
        return guid.ToString().Replace("-", "");
    }
}