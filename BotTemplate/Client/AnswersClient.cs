using BotTemplate.Models;

namespace BotTemplate.Client;

public class AnswersClient : IAnswersClient
{
    private static readonly HttpClient Client = new();
    
    public async Task SendAnswerAsync(long chatId,
        DateTime date,
        string answer,
        int answerNumber,
        ScenarioType scenarioType,
        bool replaceValue)
    {
        var requestBody = new Dictionary<string, string>
        {
            { "chatId", chatId.ToString() },
            { "date", date.Date.ToString("dd.MM.yyyy") },
            { "answer", answer },
            { "answerNumber", answerNumber.ToString() },
            { "scenarioType", scenarioType.ToString() },
            { "replaceValue", replaceValue.ToString() }
        };

        var content = new FormUrlEncodedContent(requestBody);

        var response = await Client.PostAsync("bla-bla-bla", content);
    }
}