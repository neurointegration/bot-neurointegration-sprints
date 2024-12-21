namespace BotTemplate.Services.Telegram.Validators;

public class TimeValidator : IValidator
{
    public bool IsValid(string? text)
    {
        if (text is null)
            return false;
        
        var trimmedText = text.Trim(' ');
        
        if (!TimeSpan.TryParse(trimmedText, out var timeSpan))
            return false;

        return timeSpan < TimeSpan.FromHours(24);
    }
}