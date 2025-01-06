using BotTemplate.Services.Telegram.Commands;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Status;

public record SendStateMessage: IMessageCommand
{
    public string Command => "/stateMessage";
    
    public async Task<CommandResponse<string>> Handle(IMessageSender messageSender, long chatId, string? userMessage)
    {
        // Buttons
        var redButton = new InlineKeyboardButton("\ud83d\udd25 Паника \ud83d\udd25");
        var orangeButton = new InlineKeyboardButton("\u26a1\ufe0f Перевозбуждение \u26a1\ufe0f");
        var yellowButton = new InlineKeyboardButton("\u2b50 Включенность \u2b50\ufe0f");
        var greenButton = new InlineKeyboardButton("\ud83c\udf40 Баланс \ud83c\udf40");
        var lightBlueButton = new InlineKeyboardButton("\ud83e\udd8b Расслабленность \ud83e\udd8b");
        var blueButton = new InlineKeyboardButton("\ud83d\ude45\ud83c\udffb\u200d\u2642\ufe0f Пассивность \ud83d\ude45\ud83c\udffb\u200d\u2642\ufe0f");
        var purpleButton = new InlineKeyboardButton("\u2614\ufe0f Апатия \u2614");

        redButton.CallbackData = "Паника";
        orangeButton.CallbackData = "Перевозбуждение";
        yellowButton.CallbackData = "Включенность";
        greenButton.CallbackData = "Баланс";
        lightBlueButton.CallbackData = "Расслабленность";
        blueButton.CallbackData = "Пассивность";
        purpleButton.CallbackData = "Апатия";
    
        var buttons = new[] 
        { 
            new[] { redButton }, 
            new[] { orangeButton }, 
            new[] { yellowButton }, 
            new[] { greenButton },
            new[] { lightBlueButton },
            new[] { blueButton },
            new[] { purpleButton } 
        };
    
        // Keyboard markup
        var inline = new InlineKeyboardMarkup(buttons);
    
        // Send message!
        await messageSender.SayWithMarkup("Как ты себя сейчас чувствуешь?", chatId, inline);
        return CommandResponse<string>.CreateSuccessfulCommandResponse();
    }
}