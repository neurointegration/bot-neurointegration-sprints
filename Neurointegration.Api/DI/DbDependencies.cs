using Neurointegration.Api.Storages.Tables;
using Neurointegration.Api.Storages.Tables.Ydb;

namespace Neurointegration.Api.DI;

public static class DbDependencies
{
    public static IServiceCollection AddDb(this IServiceCollection service)
    {
        service.AddSingleton<ITableInitializer, UsersTableInitializer>();
        service.AddSingleton<ITableInitializer, UserAccessTableInitializer>();
        service.AddSingleton<ITableInitializer, QuestionTableInitializer>();
        service.AddSingleton<ITableInitializer, SprintTableInitializer>();
        service.AddSingleton<YdbInitializer>();

        return service;
    }
}