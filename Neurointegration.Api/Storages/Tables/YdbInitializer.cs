using Neurointegration.Api.Storages.Tables.Ydb;

namespace Neurointegration.Api.Storages.Tables;

public class YdbInitializer
{
    private readonly List<ITableInitializer> initializers;

    public YdbInitializer(IEnumerable<ITableInitializer> initializers)
    {
        this.initializers = initializers.ToList();
    }

    public async Task CreateTables()
    {
        foreach (var tableInitializer in initializers)
        {
            // await tableInitializer.CreateTable();
        }
    }
}