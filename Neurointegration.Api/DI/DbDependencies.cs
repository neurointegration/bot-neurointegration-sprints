using Common.Ydb.Schema;
using Neurointegration.Api.Storages.Answers;
using Neurointegration.Api.Storages.RoutineActions;
using Neurointegration.Api.Storages.Tables;

namespace Neurointegration.Api.DI;

public static class DbDependencies
{
    public static IServiceCollection AddDb(this IServiceCollection service)
    {
        service.AddSingleton<AnswerTableSchema>();
        service.AddSingleton<RoutineTableSchema>();

        return service;
    }
    
    public static IServiceCollection AddInitialize(this IServiceCollection service)
    {
        // service.AddSingleton<ITableSchema, AnswerTableSchema>();
        service.AddSingleton<ITableSchema, RoutineTableSchema>();
        
        service.AddSingleton<YdbInitializer>();

        return service;
    }
}