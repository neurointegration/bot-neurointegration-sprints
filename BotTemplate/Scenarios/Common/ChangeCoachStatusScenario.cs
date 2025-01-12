using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Bottom;
using Neurointegration.Api.DataModels.Dto;

namespace BotTemplate.Scenarios.Common;

public class ChangeCoachStatusScenario: IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private const string Command = CommandsConstants.ChangeCoachStatus;

    public ChangeCoachStatusScenario(IBackendApiClient backendApiClient, IMessageSender messageSender)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
    }
    
    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        if (telegramEvent.Text != Command)
            return false;
        
        var chatId = telegramEvent.ChatId;
        
        var getUser = await backendApiClient.GetUser(chatId);
        if (!getUser.IsSuccess)
            await messageSender.Say(MessageConstants.UnknownErrorText, chatId);
        
        await backendApiClient.UpdateUser(new UpdateUser {UserId = chatId, IAmCoach = !getUser.Value.IAmCoach});
        var answer = !getUser.Value.IAmCoach ? "Теперь ты тренер" : "Теперь ты не тренер";
        await messageSender.SayWithMarkup(answer, chatId, BottomMessage.GetMessage().ReplyMarkup);
        return true;
    }
}