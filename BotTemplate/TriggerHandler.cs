using BotTemplate.Client;
using BotTemplate.Models;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Grpc.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
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
        var logger = new ConsoleLogger();
        logger.ForType<string>().Info(request);
        var configuration = Configuration.FromEnvironment();
        var tgClient = new TelegramBotClient(configuration.TelegramToken);
        var view = new HtmlMessageView(tgClient);
        var botDatabase = new BotDatabase(configuration);
        var backendApiClient = InitializeLocalClient.Init().GetService<IBackendApiClient>() ?? throw new ArgumentException("Не задан клиент бэкенда");
        var triggerFrequencyMinutes = int.Parse(configuration.TriggerFrequencyMinutes!);
        
        var questionRequest = JsonConvert.DeserializeObject<QuestionRequest>(request)!;
        questionRequest.Time ??= triggerFrequencyMinutes;
        logger.ForType<string>().Info(questionRequest.ToString());

        var questionService = new QuestionService(view, botDatabase, backendApiClient);
        return await questionService.AskQuestions(questionRequest);
    }
}