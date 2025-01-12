using Common.Ydb;
using Common.Ydb.Schema;

namespace Neurointegration.Api.Storages.Tables;

public class YdbInitializer
{
    private readonly YdbClient ydbClient;
    private readonly List<ITableSchema> schemas;

    public YdbInitializer(YdbClient ydbClient, IEnumerable<ITableSchema> tableSchemas)
    {
        this.ydbClient = ydbClient;
        this.schemas = tableSchemas.ToList();
    }

    public async Task CreateTables()
    {
        foreach (var schema in schemas)
        {
            await ydbClient.CreateTable(schema);
        }
    }
}