using Neurointegration.Api.Settings;

namespace Neurointegration.Api.Storages.Tables.Ydb;

public class SprintTableInitializer: ITableInitializer
{
    private readonly YdbClient client;

    public SprintTableInitializer(YdbClient client)
    {
        this.client = client;
    }
    
    public async Task CreateTable()
    {
         await client.ExecuteScheme($@"
             CREATE TABLE {SprintDbSettings.TableName} (
                 {SprintDbSettings.UserIdField} Int64 NOT NULL,
                 {SprintDbSettings.SprintNumberField} Int64 NOT NULL,
                 {SprintDbSettings.SprintStartDateField} DATE NOT NULL,
                 {SprintDbSettings.SheetIdField} Utf8 NOT NULL,

                 PRIMARY KEY ({SprintDbSettings.UserIdField}, {SprintDbSettings.SprintNumberField})
             )
         ");
    }
}