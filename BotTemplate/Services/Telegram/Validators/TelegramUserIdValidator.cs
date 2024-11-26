namespace BotTemplate.Services.Telegram.Validators;

public class TelegramUserIdValidator : IValidator
{
    public bool IsValid(string? text)
    {
        if (text is null)
            return false;
        
        var trimmedText = text.Trim(' ');

        return long.TryParse(trimmedText, out _);
    }
}