using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace BotTemplate.DI;

public static class InternalDependenciesExtensions
{
    public static IServiceCollection AddTgClient(this IServiceCollection serviceCollection, string token)
    {
        serviceCollection.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));

        return serviceCollection;
    }

    public static IServiceCollection AddMessageView(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IMessageView>(provider =>
            new HtmlMessageView(provider.GetRequiredService<ITelegramBotClient>(), provider.GetRequiredService<ILogger>()));

        return serviceCollection;
    }
    
    public static IServiceCollection AddBotDb(this IServiceCollection serviceCollection, Configuration configuration)
    {
        serviceCollection.AddSingleton<IBotDatabase>(provider => new BotDatabase(configuration));

        return serviceCollection;
    }
}