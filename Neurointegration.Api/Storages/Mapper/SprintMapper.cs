using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Settings;
using Ydb.Sdk.Value;

namespace Neurointegration.Api.Storages.Mapper;

public class SprintMapper
{
    public Sprint ToSprintEntity(ResultSet.Row row)
    {
        return new Sprint()
        {
            UserId = row[SprintDbSettings.UserIdField].GetInt64(),
            SprintNumber = row[SprintDbSettings.SprintNumberField].GetInt64(),
            SheetId = row[SprintDbSettings.SheetIdField].GetUtf8(),
            SprintStartDate = row[SprintDbSettings.SprintStartDateField].GetDate(),
            LifeCount = row[SprintDbSettings.LifeCountField].GetOptionalInt32() ?? 0,
            PleasureCount = row[SprintDbSettings.PleasureCountField].GetOptionalInt32() ?? 0,
            DriveCount =row[SprintDbSettings.DriveCountField].GetOptionalInt32() ?? 0 
        };
    }

    public string ToSheetsId(ResultSet.Row row)
    {
        return row[SprintDbSettings.SheetIdField].GetUtf8();
    }
}