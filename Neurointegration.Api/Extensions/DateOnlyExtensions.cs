namespace Neurointegration.Api.Extensions;

public static class DateOnlyExtensions
{
    public static string ToGoogleDateString(this DateOnly dateOnly)
    {
        return $"{dateOnly.Month}/{dateOnly.Day}/{dateOnly.Year}";
    }
}