using Common.Ydb;
using Neurointegration.Api.Storages.Answers;

namespace Neurointegration.Api.Storages.Tables.Ydb;

public class AnswerTableInitializer: ITableInitializer
{
    private readonly YdbClient client;

    public AnswerTableInitializer(YdbClient client)
    {
        this.client = client;
    }
    
    public async Task CreateTable()
    {
        var tableProperties = new AnswerTableProperties();
        await client.ExecuteScheme(tableProperties.GetSchema());
    }
}