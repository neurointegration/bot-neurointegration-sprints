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
        ScenarioType scenarioType,
        DateTime? date = null,
        long currentSprintNumber = 0,
        int sprintReplyNumber = 0,
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

            REPLACE INTO {TableName} (scenario_to_start_id, chat_id, scenario_id, scenario_type, current_sprint_number, sprint_reply_number, date, data )
            VALUES ( $scenario_to_start_id, $chat_id, $scenario_id, $scenario_type, $current_sprint_number, $sprint_reply_number, $date, $data)
        ", new Dictionary<string, YdbValue?>
        {
            {"$scenario_to_start_id", YdbValue.MakeUtf8(scenarioToStartId)},
            {"$chat_id", YdbValue.MakeInt64(chatId)},
            {"$scenario_id", YdbValue.MakeUtf8(scenarioId)},
            {"$scenario_type", YdbValue.MakeUtf8(scenarioType.ToString())},
            {"$current_sprint_number", YdbValue.MakeInt64(currentSprintNumber)},
            {"$sprint_reply_number", YdbValue.MakeInt32(sprintReplyNumber)},
            {"$date", YdbValue.MakeOptionalDatetime(date)},
            {"$data", YdbValue.MakeOptionalJson(data)}
        });

        logger.LogInformation($"Добавили для пользователя {chatId} возможность начать сценарий {scenarioId}");

        return scenarioToStartId;
    }

    public async Task<ScenarioToStart?> GetScenarioToStartAndDeleteIt(string scenarioToStartId)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $scenario_to_start_id AS Utf8;

            SELECT chat_id, scenario_id, scenario_type, current_sprint_number, sprint_reply_number, date, data
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

        await botDatabase.ExecuteModify($@"
            DECLARE $scenario_to_start_id AS Utf8;

            DELETE FROM {TableName} WHERE scenario_to_start_id = $scenario_to_start_id;
        ", new Dictionary<string, YdbValue?>
        {
            {"$scenario_to_start_id", YdbValue.MakeUtf8(scenarioToStartId)}
        });

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();
        if (!rowsArray.Any())
        {
            return null;
        }

        Enum.TryParse(rowsArray.First()["scenario_type"].GetUtf8(), out ScenarioType scenarioType);
        return new ScenarioToStart(
                rowsArray.First()["chat_id"].GetInt64(),
                rowsArray.First()["scenario_id"].GetUtf8(),
                scenarioType,
                rowsArray.First()["date"].GetOptionalDatetime(),
                rowsArray.First()["current_sprint_number"].GetInt64(),
                rowsArray.First()["sprint_reply_number"].GetInt32(),
                rowsArray.First()["data"].GetOptionalJson()
            );
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