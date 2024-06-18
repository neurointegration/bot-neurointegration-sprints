namespace BotTemplate.Services.Telegram.Validators;

public class TimeValidator : IValidator
{
    public bool IsValid(string? text)
    {
        if (text is null)
            return false;
        
        var trimmedText = text.Trim(' ');
        
        return TimeSpan.TryParse(trimmedText, out _);
    }
}