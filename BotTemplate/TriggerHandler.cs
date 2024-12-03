using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Grpc.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class TriggerHandler : YcFunction<string, Response>
{
    public Response FunctionHandler(string request, Context context)
    {
        var logger = new ConsoleLogger();
        logger.ForType<string>().Info(request);
        try
        {
            var countQuestions = HandleRequest(request).GetAwaiter().GetResult();
            return new Response(200, $"{countQuestions}");
        }
        catch (Exception e)
        {
            logger.ForType<Exception>().Error(e, "Error");
            return new Response(500, $"Error {e}");
        }
    }
    
    private async Task<int> HandleRequest(string request)
    {
        var configuration = Configuration.FromEnvironment();
        var tgClient = new TelegramBotClient(configuration.TelegramToken);
        var view = new HtmlMessageView(tgClient);
        var botDatabase = new BotDatabase(configuration);
        var backendApiClient = InitializeLocalClient.Init().GetService<IBackendApiClient>() ?? throw new ArgumentException("Не задан клиент бэкенда");
        var triggerFrequencyMinutes = int.Parse(configuration.TriggerFrequencyMinutes!);
        var successParse = int.TryParse(request, out var time);
        if (!successParse)
            time = triggerFrequencyMinutes;

        var questionService = new QuestionService(view, botDatabase, backendApiClient, time);
        return await questionService.AskQuestions();
    }
}