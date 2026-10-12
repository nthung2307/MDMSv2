namespace Application.Extensions;

public static class IntegerExtensions
{
    /// <summary>
    /// Page size must be greater than 0 and less than 100
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsValidPageSize(this int value)
    {
        return value is > 0 and <= 300;
    }

    /// <summary>
    /// Current page must be greater than 0
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsValidCurrentPage(this int value)
    {
        return value is > 0;
    }
}