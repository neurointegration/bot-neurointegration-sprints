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
            return new Response(200, $"{countQuestions}");
        }
        catch (Exception e)
        {
            return new Response(500, $"Error {e}");
        }
    }

    private async Task<int> HandleRequest(string request)
    {
        var configuration = Configuration.FromEnvironment();
        var provider = BuildDeps(configuration);

        var questionService = provider.GetRequiredService<QuestionService>();

        var logger = provider.GetRequiredService<ILogger>();
        logger.LogInformation($"Accept request = {request}");
        var parsedRequest = ParseRequest(request, configuration, logger);


        return await questionService.AskQuestions(parsedRequest);
    }

    private IServiceProvider BuildDeps(Configuration configuration)
    {
        var service = new ServiceCollection();
        using var factory = LoggerFactory.Create(builder => builder.AddSimpleConsole());

        return service
            .AddSingleton<ILogger>(factory.CreateLogger("TriggerHandler"))
            .AddBackend()
            .AddTgClient(configuration.TelegramToken)
            .AddMessageView()
            .AddBotDb(configuration)
            .AddSingleton<QuestionService>()
            .BuildServiceProvider();
    }

    private QuestionRequest ParseRequest(string request, Configuration configuration, ILogger logger)
    {
        var triggerFrequencyMinutes = int.Parse(configuration.TriggerFrequencyMinutes!);
        var questionRequest = JsonConvert.DeserializeObject<QuestionRequest>(request);
        if (questionRequest == null)
            questionRequest = new QuestionRequest();

        questionRequest.Time ??= triggerFrequencyMinutes;

        logger.LogInformation($"Parsed request = {questionRequest}");

        return questionRequest;
    }
}