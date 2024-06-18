namespace BotTemplate.Services.Telegram.Validators;

public class TimeRangeValidator : IValidator
{
    public bool IsValid(string? text)
    {
        if (text is null)
            return false;
        
        var trimmedText = text.Trim(' ');
        
        if (trimmedText.Length > 11)
            return false;
        
        if (trimmedText.Count(c => c == '-') != 1)
            return false;

        var timeSpanStrings = trimmedText.Split('-');

        if (!TimeSpan.TryParse(timeSpanStrings[0], out _))
            return false;
        
        if (!TimeSpan.TryParse(timeSpanStrings[1], out _))
            return false;

        return true;
    }
}