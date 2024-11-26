namespace BotTemplate.Services.Telegram.Validators;

public interface IValidator
{
    bool IsValid(string? text);
}