namespace Neurointegration.Api.Extensions;

public static class DayOfWeekExtensions
{
    public static DayOfWeek RussianFormat(this DayOfWeek dayOfWeek)
    {
        var intEnum = ((int) dayOfWeek + 6) % 7;
        return (DayOfWeek) intEnum;
    }
}