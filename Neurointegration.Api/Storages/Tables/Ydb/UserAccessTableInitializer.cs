using Neurointegration.Api.Settings;

namespace Neurointegration.Api.Storages.Tables.Ydb;

public class UserAccessTableInitializer : ITableInitializer
{
    private readonly YdbClient client;

    public UserAccessTableInitializer(YdbClient client)
    {
        this.client = client;
    }

    public async Task CreateTable()
    {
        await client.ExecuteScheme($@"
            CREATE TABLE {UserAccessDbSettings.TableName} (
                {UserAccessDbSettings.GrantedUserIdField} Int64 NOT NULL,
                {UserAccessDbSettings.OwnerUserIdField} Int64 NOT NULL,
                {UserAccessDbSettings.PermissionIdField} Utf8 NOT NULL,
                {UserAccessDbSettings.SheetIdField} Utf8 NOT NULL,
                PRIMARY KEY ({UserAccessDbSettings.GrantedUserIdField}, {UserAccessDbSettings.OwnerUserIdField}, {UserAccessDbSettings.SheetIdField})
            )");
    }
}