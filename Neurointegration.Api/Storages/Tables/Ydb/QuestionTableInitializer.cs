using Neurointegration.Api.Settings;

namespace Neurointegration.Api.Storages.Tables.Ydb;

public class QuestionTableInitializer : ITableInitializer
{
    private readonly YdbClient client;

    public QuestionTableInitializer(YdbClient client)
    {
        this.client = client;
    }

    public async Task CreateTable()
    {
        await client.ExecuteScheme($@"
            CREATE TABLE {QuestionDbSettings.TableName} (
                {QuestionDbSettings.DateField} Datetime NOT NULL,
                {QuestionDbSettings.UserIdField} Int64 NOT NULL,
                {QuestionDbSettings.ScenarioTypeField} Utf8 NOT NULL,
                {QuestionDbSettings.PriorityField} Int32 NOT NULL,
                {QuestionDbSettings.SprintReplyNumberField} Int32 NOT NULL,
                {QuestionDbSettings.SprintNumberField} Int64 NOT NULL,
                PRIMARY KEY ({QuestionDbSettings.DateField}, {QuestionDbSettings.UserIdField})
            )");
    }
}