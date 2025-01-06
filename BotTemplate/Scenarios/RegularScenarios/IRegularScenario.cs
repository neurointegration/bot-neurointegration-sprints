
using BotTemplate.Models.Telegram;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Scenarios.RegularScenarios;

public interface IRegularScenario
{
    Task Start(Question question);

    Task Handle(TelegramEvent telegramEvent);
}