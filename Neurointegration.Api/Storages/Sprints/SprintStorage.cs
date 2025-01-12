using Common.Ydb;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;
using Neurointegration.Api.Excpetions;
using Neurointegration.Api.Settings;
using Neurointegration.Api.Storages.Mapper;
using Ydb.Sdk.Value;

namespace Neurointegration.Api.Storages.Sprints;

public class SprintStorage : ISprintStorage
{
    private readonly YdbClient ydbClient;
    private readonly SprintMapper sprintMapper;

    public SprintStorage(YdbClient ydbClient, SprintMapper sprintMapper)
    {
        this.ydbClient = ydbClient;
        this.sprintMapper = sprintMapper;
    }

    public async Task<List<string>> GetUserGoogleSheets(long userId)
    {
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${SprintDbSettings.UserIdField} AS Int64;

            SELECT {SprintDbSettings.SheetIdField}
            FROM {SprintDbSettings.TableName}
            WHERE {SprintDbSettings.UserIdField} = ${SprintDbSettings.UserIdField}
        ", new Dictionary<string, YdbValue>
        {
            {$"${SprintDbSettings.UserIdField}", YdbValue.MakeInt64(userId)}
        });

        return rows.Select(row => sprintMapper.ToSheetsId(row)).ToList();
    }

    public async Task<List<Sprint>> GetUserSprints(long userId)
    {
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${SprintDbSettings.UserIdField} AS Int64;

            SELECT *
            FROM {SprintDbSettings.TableName}
            WHERE {SprintDbSettings.UserIdField} = ${SprintDbSettings.UserIdField}
        ", new Dictionary<string, YdbValue>
        {
            {$"${SprintDbSettings.UserIdField}", YdbValue.MakeInt64(userId)}
        });

        return rows.Select(row => sprintMapper.ToSprintEntity(row)).ToList();
    }

    public async Task SaveOrUpdate(Sprint sprint)
    {
        await ydbClient.ExecuteDataQuery($@"
             DECLARE ${SprintDbSettings.UserIdField} AS Int64;
             DECLARE ${SprintDbSettings.SprintNumberField} AS Int64;
             DECLARE ${SprintDbSettings.SheetIdField} AS text;
             DECLARE ${SprintDbSettings.SprintStartDateField} AS DATE;

             REPLACE INTO {SprintDbSettings.TableName} 
                ( {SprintDbSettings.UserIdField}, {SprintDbSettings.SprintNumberField}, {SprintDbSettings.SheetIdField},
                  {SprintDbSettings.SprintStartDateField} )
             VALUES ( ${SprintDbSettings.UserIdField}, ${SprintDbSettings.SprintNumberField}, ${SprintDbSettings.SheetIdField},
                        ${SprintDbSettings.SprintStartDateField} )",
            new Dictionary<string, YdbValue>()
            {
                {$"${SprintDbSettings.UserIdField}", YdbValue.MakeInt64(sprint.UserId)},
                {$"${SprintDbSettings.SprintNumberField}", YdbValue.MakeInt64(sprint.SprintNumber)},
                {$"${SprintDbSettings.SheetIdField}", YdbValue.MakeUtf8(sprint.SheetId)},
                {$"${SprintDbSettings.SprintStartDateField}", YdbValue.MakeDate(sprint.SprintStartDate)}
            });
    }

    public async Task<Result<Sprint>> GetSprint(long userId, long sprintNumber)
    {
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${SprintDbSettings.UserIdField} AS Int64;
            DECLARE ${SprintDbSettings.SprintNumberField} AS Int64;

            SELECT *
            FROM {SprintDbSettings.TableName}
            WHERE {SprintDbSettings.UserIdField} = ${SprintDbSettings.UserIdField} AND
                  {SprintDbSettings.SprintNumberField} = ${SprintDbSettings.SprintNumberField}
        ", new Dictionary<string, YdbValue>
        {
            {$"${SprintDbSettings.UserIdField}", YdbValue.MakeInt64(userId)},
            {$"${SprintDbSettings.SprintNumberField}", YdbValue.MakeInt64(sprintNumber)}
        });

        var rowsList = rows.ToList();
        if (rowsList.Count == 0)
            return Result<Sprint>.Fail(Error.NotFound($"Спринт не найден. userId={userId} и sprintNumber={sprintNumber}"));

        return Result<Sprint>.Success(sprintMapper.ToSprintEntity(rowsList[0]));
    }
}