using BotTemplate.Models;
using BotTemplate.Services.Telegram;
using Newtonsoft.Json;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class TriggerHandler : BaseFunctionHandler<QuestionService>
{
    protected override string LogCategoryName { get; set; } = "TriggerHandler";
    
    protected override async Task<string> InnerHandleRequest(string request, Context context)
    {
        var parsedRequest = ParseRequest(request);

        var askResult = await handleService.AskQuestions(parsedRequest);
        return $"Количество вопросов: {askResult}";
    }

    private QuestionRequest ParseRequest(string request)
    {
        var triggerFrequencyMinutes = int.Parse(configuration.TriggerFrequencyMinutes!);
        var questionRequest = JsonConvert.DeserializeObject<QuestionRequest>(request);
        if (questionRequest == null)
            questionRequest = new QuestionRequest();

        questionRequest.Time ??= triggerFrequencyMinutes;

        return questionRequest;
    }
}