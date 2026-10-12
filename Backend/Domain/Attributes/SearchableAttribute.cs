namespace VF.Core.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class SearchableAttribute : Attribute
{
    public enum SearchType
    {
        Contains,
        Equals
    }

    public SearchType Type { get; }

    public SearchableAttribute(SearchType type = SearchType.Contains)
    {
        Type = type;
    }
}