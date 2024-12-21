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
        
        if (!TimeSpan.TryParse(timeSpanStrings[0], out var timeSpan1))
            return false;
            
        if (timeSpan1 >= TimeSpan.FromHours(24))
            return false;
        
        if (!TimeSpan.TryParse(timeSpanStrings[1], out var timeSpan2))
            return false;
            
        if (timeSpan2 >= TimeSpan.FromHours(24))
            return false;

        return true;
    }
}