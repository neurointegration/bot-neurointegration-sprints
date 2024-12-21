using BotTemplate.Client;
using BotTemplate.DI;
using BotTemplate.Models;
using BotTemplate.Models.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class TriggerHandler : YcFunction<string, Response>
{
    public Response FunctionHandler(string request, Context context)
    {
        try
        {
            var countQuestions = HandleRequest(request).GetAwaiter().GetResult();
            return new Response(200, $"Количество вопросов: {countQuestions}");
        }
        catch (Exception e)
        {
            return new Response(500, $"Error {e}");
        }
    }

    private async Task<int> HandleRequest(string request)
    {
        var configuration = Configuration.FromEnvironment();
        var service = new ServiceCollection();
        var provider = service.BuildDeps(configuration, "TriggerHandler");

        var questionService = provider.GetRequiredService<QuestionService>();

        var logger = provider.GetRequiredService<ILogger>();
        logger.LogInformation($"Запрос: {request}");
        var parsedRequest = ParseRequest(request, configuration, logger);

        return await questionService.AskQuestions(parsedRequest);
    }

    private QuestionRequest ParseRequest(string request, Configuration configuration, ILogger logger)
    {
        var triggerFrequencyMinutes = int.Parse(configuration.TriggerFrequencyMinutes!);
        var questionRequest = JsonConvert.DeserializeObject<QuestionRequest>(request);
        if (questionRequest == null)
            questionRequest = new QuestionRequest();

        questionRequest.Time ??= triggerFrequencyMinutes;

        logger.LogInformation($"Запрос: {questionRequest}");

        return questionRequest;
    }
}