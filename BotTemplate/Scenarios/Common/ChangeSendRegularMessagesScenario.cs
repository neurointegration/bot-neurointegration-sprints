using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using Neurointegration.Api.DataModels.Dto;

namespace BotTemplate.Scenarios.Common;

public class ChangeSendRegularMessagesScenario: IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private const string Command = CommandsConstants.ChangeSprintRegular;

    public ChangeSendRegularMessagesScenario(IBackendApiClient backendApiClient, IMessageSender messageSender)
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
        
        await backendApiClient.UpdateUser(new UpdateUser {UserId = chatId, SendRegularMessages = !getUser.Value.SendRegularMessages});
        var answer = !getUser.Value.SendRegularMessages 
            ? "Ура! Ты снова в деле! Обязательно укажи в настройках остальные данные для спринтов"
            : "Грустно. Если захочешь проходить спринты снова, то переходи в настройки и нажми на кнопку “Я хочу проходить спринты”";
        await messageSender.Say(answer, chatId);

        return true;
    }
}