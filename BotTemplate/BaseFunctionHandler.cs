using BotTemplate.DI;
using Microsoft.Extensions.DependencyInjection;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public abstract class BaseFunctionHandler<T> : YcFunction<string, Response>
{
    protected IServiceProvider provider;
    protected Configuration configuration;
    protected T handleService;

    public Response FunctionHandler(string request, Context context)
    {
        configuration = Configuration.FromEnvironment();
        var service = new ServiceCollection();
        provider = service.BuildDeps(configuration, "TriggerHandler");
        handleService = provider.GetRequiredService<T>();
        try
        {
            var result = InnerHandleRequest(request, context).GetAwaiter().GetResult();
            return new Response(200, result);
        }
        catch (Exception e)
        {
            return new Response(500, $"Error {e}");
        }
    }

    protected abstract Task<string> InnerHandleRequest(string request, Context context);

    protected abstract string LogCategoryName { get; set; }
}