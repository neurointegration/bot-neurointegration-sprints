using Common.Ydb;
using Neurointegration.Api.Settings;

namespace Neurointegration.Api.Storages.Tables.Ydb;

public class UsersTableInitializer : ITableInitializer
{
    private readonly YdbClient client;

    public UsersTableInitializer(YdbClient client)
    {
        this.client = client;
    }

    public async Task CreateTable()
    {
        await client.ExecuteScheme($@"
            CREATE TABLE {UserDbSettings.TableName} (
                {UserDbSettings.UserIdField} Int64 NOT NULL,
                {UserDbSettings.EmailField} Utf8 NOT NULL,
                {UserDbSettings.UsernameField} Utf8 NOT NULL,
                {UserDbSettings.IAmCoachField} Bool NOT NULL,
                {UserDbSettings.SendRegularMessagesField} Bool NOT NULL,
                
                {UserDbSettings.EveningStandUpTimeField} Interval,
                {UserDbSettings.WeekReflectionTime} Interval,
                {UserDbSettings.MessageStartTimeField} Interval,
                {UserDbSettings.MessageEndTimeField} Interval,

                PRIMARY KEY ({UserDbSettings.UserIdField})
            )
        ");
    }
}