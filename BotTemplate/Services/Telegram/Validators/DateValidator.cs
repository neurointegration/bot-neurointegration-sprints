using System.Globalization;

namespace BotTemplate.Services.Telegram.Validators;

public class DateValidator : IValidator
{
    public bool IsValid(string? text)
    {
        if (text is null)
            return false;
        
        var trimmedText = text.Trim(' ');

        return DateTime.TryParseExact(trimmedText, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}