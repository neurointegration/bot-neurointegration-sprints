using System.Net.Mail;

namespace BotTemplate.Services.Telegram.Validators;

public class EmailValidator : IValidator
{
    public bool IsValid(string? email)
    {
        if (email is null)
            return false;
        
        var trimmedEmail = email.Trim(' ');

        return MailAddress.TryCreate(trimmedEmail, out _);
    }
}