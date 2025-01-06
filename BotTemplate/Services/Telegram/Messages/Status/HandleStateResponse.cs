using BotTemplate.Services.Telegram.Commands;

namespace BotTemplate.Services.Telegram.Messages.Status;

public record HandleStateResponse : IMessageCommand
{
    public string Command => "/handleStateResponse";
    
    public async Task<CommandResponse<string>> Handle(IMessageSender messageSender, long chatId, string? userMessage)
    {
        string message;
        
        switch (userMessage)
        {
            case "Паника":
                message = "Замедли дыхание, приляг или сядь. Попробуй отвлечься, например, начни считать все синие предметы, которые видишь вокруг";
                break;
            case "Перевозбуждение":
                message = "Попробуй медленно и глубоко дышать. Если есть возможность, выполни небольшую физическую активность. Это поможет сбросить лишнюю энергию";
                break;
            case "Включенность":
                message = "Классно, что ты погружен в дело! Не забывай, что чрезмерная включенность в работу может привести к переутомлению";
                break;
            case "Баланс":
                message = "Отлично! Продолжай в том же духе! Хорошего тебе дня!";
                break;
            case "Расслабленность":
                message = "Если расслабленность мешает твоей продуктивности, то попробуй спланировать день/неделю.  Это помогает сосредоточиться на целях и побудить к действию";
                break;
            case "Пассивность":
                message = "Не требуй слишком много от себя. Иногда важно дать отдохнуть и позволить себе не делать ничего";
                break;
            case "Апатия":
                message = "Дай себе возможность отдохнуть. Лучше поспать или выпить некрепкий сладкий чай";
                break;
            default:
                message = "Я пока не знаю, что это за состояние";
                await messageSender.Say(message, chatId);
                return CommandResponse<string>.CreateFailedCommandResponse(message);
        }

        await messageSender.Say(message, chatId);
        return CommandResponse<string>.CreateSuccessfulCommandResponse(message);
    }
}