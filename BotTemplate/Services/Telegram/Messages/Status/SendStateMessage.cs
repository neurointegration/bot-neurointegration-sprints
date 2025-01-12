using BotTemplate.Models;
using BotTemplate.Services.Telegram.Commands;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Status;

public record SendStateMessage: IMessageCommand
{
    public string Command => "/stateMessage";
    
    public async Task<CommandResponse<string>> Handle(IMessageSender messageSender, long chatId, string? userMessage)
    {
        // Buttons
        var redButton = new InlineKeyboardButton($"\ud83d\udd25 {CommandsConstants.StatusPanic} \ud83d\udd25");
        var orangeButton = new InlineKeyboardButton($"\u26a1\ufe0f {CommandsConstants.StatusOverexcitation} \u26a1\ufe0f");
        var yellowButton = new InlineKeyboardButton($"\u2b50 {CommandsConstants.StatusInclusion} \u2b50\ufe0f");
        var greenButton = new InlineKeyboardButton($"\ud83c\udf40 {CommandsConstants.StatusBalance} \ud83c\udf40");
        var lightBlueButton = new InlineKeyboardButton($"\ud83e\udd8b {CommandsConstants.StatusRelaxation} \ud83e\udd8b");
        var blueButton = new InlineKeyboardButton($"\ud83d\ude45\ud83c\udffb\u200d\u2642\ufe0f {CommandsConstants.StatusPassivity} \ud83d\ude45\ud83c\udffb\u200d\u2642\ufe0f");
        var purpleButton = new InlineKeyboardButton($"\u2614\ufe0f {CommandsConstants.StatusApathy} \u2614");

        redButton.CallbackData =  CommandsConstants.StatusPanic;
        orangeButton.CallbackData = CommandsConstants.StatusOverexcitation;
        yellowButton.CallbackData = CommandsConstants.StatusInclusion;
        greenButton.CallbackData = CommandsConstants.StatusBalance;
        lightBlueButton.CallbackData = CommandsConstants.StatusRelaxation;
        blueButton.CallbackData = CommandsConstants.StatusPassivity;
        purpleButton.CallbackData = CommandsConstants.StatusApathy;
    
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