using Microsoft.Extensions.DependencyInjection;
using Neurointegration.Api.DI;
using Neurointegration.Api.Settings;

namespace BotTemplate.Client;

public static class InitializeLocalClient
{
    public static IServiceCollection AddBackend(this IServiceCollection serviceCollection)
    {
        var settings = ApiSecretSettings.FromEnvironment();
        serviceCollection
            .AddInternalDependencies(settings)
            .AddSingleton<IBackendApiClient, LocalBackendApiClient>();

        return serviceCollection;
    }
}