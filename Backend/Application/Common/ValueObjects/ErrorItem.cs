using System.Diagnostics.CodeAnalysis;

namespace Application.Common.ValueObjects;

[ExcludeFromCodeCoverage]
public class ErrorItem
{
    public string FieldName { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}