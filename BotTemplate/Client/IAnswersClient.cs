using BotTemplate.Models;

namespace BotTemplate.Client;

public interface IAnswersClient
{
    Task SendAnswerAsync(long chatId, DateTime dateOnly, string answer, int answerNumber, ScenarioType scenarioType, bool replaceValue);
}