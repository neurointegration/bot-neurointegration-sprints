using BotTemplate.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public abstract class BaseFunctionHandler<T> : YcFunction<string, Response>
{
    protected IServiceProvider Provider;
    protected Configuration Configuration;
    protected T HandleService;
    protected ILogger Logger;

    public Response FunctionHandler(string request, Context context)
    {
        Configuration = Configuration.FromEnvironment();
        var service = new ServiceCollection();
        Provider = service.BuildDeps(Configuration, LogCategoryName);
        HandleService = Provider.GetRequiredService<T>();
        Logger = Provider.GetRequiredService<ILogger>();
        Logger.LogInformation($"Запрос: {request}");

        var result = InnerHandleRequest(request, context).GetAwaiter().GetResult();
        return new Response(200, result);
    }

    protected abstract Task<string> InnerHandleRequest(string request, Context context);

    protected abstract string LogCategoryName { get; set; }
}