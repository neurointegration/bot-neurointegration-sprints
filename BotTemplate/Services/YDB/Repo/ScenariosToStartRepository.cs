using BotTemplate.Models.ScenariosData;
using BotTemplate.Services.YDB.Repo;
using Microsoft.Extensions.Logging;
using Neurointegration.Api.DataModels.Models;
using Ydb.Sdk.Value;

namespace BotTemplate.Services.YDB;

public class ScenariosToStartRepository : IRepository
{
    public virtual string TableName => "scenarios_to_start";

    private readonly IBotDatabase botDatabase;
    private readonly ILogger logger;

    public ScenariosToStartRepository(IBotDatabase botDatabase, ILogger logger)
    {
        this.botDatabase = botDatabase;
        this.logger = logger;
    }
    
    public async Task<string> AddNewScenarioToStartAndGetItsId(
        long chatId,
        string scenarioId,
        int priority,
        ScenarioType scenarioType,
        DateTime? date,
        long currentSprintNumber,
        int sprintReplyNumber,
        bool isDelayed,
        string? data = null)
    {
        var scenarioToStartId = Guid.NewGuid().ToString();

        await botDatabase.ExecuteModify($@"
            DECLARE $scenario_to_start_id AS Utf8;
            DECLARE $chat_id AS Int64;
            DECLARE $scenario_id AS Utf8;
            DECLARE $scenario_type AS Utf8;
            DECLARE $current_sprint_number AS Int64;
            DECLARE $sprint_reply_number AS Int32;
            DECLARE $date AS DATETIME?;
            DECLARE $data AS Json?;
            DECLARE $priority AS Int32;
            DECLARE $is_delayed AS Utf8;

            REPLACE INTO {TableName} (scenario_to_start_id, chat_id, scenario_id, scenario_type, current_sprint_number, sprint_reply_number, date, data, priority, is_delayed )
            VALUES ( $scenario_to_start_id, $chat_id, $scenario_id, $scenario_type, $current_sprint_number, $sprint_reply_number, $date, $data, $priority, $is_delayed)
        ", new Dictionary<string, YdbValue?>
        {
            {"$scenario_to_start_id", YdbValue.MakeUtf8(scenarioToStartId)},
            {"$chat_id", YdbValue.MakeInt64(chatId)},
            {"$scenario_id", YdbValue.MakeUtf8(scenarioId)},
            {"$scenario_type", YdbValue.MakeUtf8(scenarioType.ToString())},
            {"$current_sprint_number", YdbValue.MakeInt64(currentSprintNumber)},
            {"$sprint_reply_number", YdbValue.MakeInt32(sprintReplyNumber)},
            {"$date", YdbValue.MakeOptionalDatetime(date)},
            {"$data", YdbValue.MakeOptionalJson(data)},
            {"$priority", YdbValue.MakeInt32(priority)},
            {"$is_delayed", YdbValue.MakeBool(isDelayed)}
        });

        logger.LogInformation($"Добавили для пользователя {chatId} возможность начать сценарий {scenarioId}");

        return scenarioToStartId;
    }

    public async Task<ScenarioToStart?> GetScenarioToStart(string scenarioToStartId)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $scenario_to_start_id AS Utf8;

            SELECT chat_id, scenario_id, scenario_type, current_sprint_number, sprint_reply_number, date, data, priority, is_delayed
            FROM {TableName}
            WHERE scenario_to_start_id = $scenario_to_start_id
        ", new Dictionary<string, YdbValue>
        {
            {"$scenario_to_start_id", YdbValue.MakeUtf8(scenarioToStartId)}
        });

        if (rows is null)
        {
            return null;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();
        if (!rowsArray.Any())
        {
            return null;
        }

        var isScenarioTypeParseSuccess = Enum.TryParse(rowsArray.First()["scenario_type"].GetUtf8(), out ScenarioType scenarioType);
        if (!isScenarioTypeParseSuccess)
            return null;

        return new ScenarioToStart(
                rowsArray.First()["chat_id"].GetInt64(),
                rowsArray.First()["scenario_id"].GetUtf8(),
                scenarioType,
                rowsArray.First()["date"].GetOptionalDatetime(),
                rowsArray.First()["current_sprint_number"].GetInt64(),
                rowsArray.First()["sprint_reply_number"].GetInt32(),
                rowsArray.First()["data"].GetOptionalJson(),
                rowsArray.First()["priority"].GetInt32(),
                rowsArray.First()["is_delayed"].GetBool()
            );
    }

    public async Task DeleteScenarioToStart(string scenarioToStartId) {
        await botDatabase.ExecuteModify($@"
            DECLARE $scenario_to_start_id AS Utf8;

            DELETE FROM {TableName} WHERE scenario_to_start_id = $scenario_to_start_id;
        ", new Dictionary<string, YdbValue?>
        {
            {"$scenario_to_start_id", YdbValue.MakeUtf8(scenarioToStartId)}
        });
    }

    public async Task CreateTable()
    {
        try
        {
            await botDatabase.ExecuteScheme($@"
            CREATE TABLE {TableName} (
                scenario_to_start_id Utf8 NOT NULL,
                chat_id Int64 NOT NULL,
                scenario_id Utf8 NOT NULL,
                scenario_type Utf8 NOT NULL,
                current_sprint_number Int64 NOT NULL,
                sprint_reply_number Int32 NOT NULL,
                is_delayed Bool NOT NULL,
                priority Int32 NOT NULL,
                date DATETIME,
                data Json,
                
                PRIMARY KEY (scenario_to_start_id)
            )
        ");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}