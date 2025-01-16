using BotTemplate.Services.YDB.Repo;
using Microsoft.Extensions.Logging;
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
        DateTime? date = null,
        long currentSprintNumber = 0,
        int sprintReplyNumber = 0,
        string? data = null)
    {
        var scenarioToStartId = Guid.NewGuid().ToString();

        await botDatabase.ExecuteModify($@"
            DECLARE $scenario_to_start_id AS Utf8,
            DECLARE $chat_id AS Int64;
            DECLARE $scenario_id AS Utf8;
            DECLARE $current_sprint_number AS Int64;
            DECLARE $sprint_reply_number AS Int32;
            DECLARE $date AS DATETIME?;
            DECLARE $data AS Json?;

            REPLACE INTO {TableName} ( chat_id, scenario_id, current_sprint_number, sprint_reply_number, index, date, data )
            VALUES ( $chat_id, $scenario_id, $current_sprint_number, $sprint_reply_number, 0, $date, $data)
        ", new Dictionary<string, YdbValue?>
        {
            {"$scenario_to_start_id", YdbValue.MakeUtf8(scenarioToStartId)},
            {"$chat_id", YdbValue.MakeInt64(chatId)},
            {"$scenario_id", YdbValue.MakeUtf8(scenarioId)},
            {"$current_sprint_number", YdbValue.MakeInt64(currentSprintNumber)},
            {"$sprint_reply_number", YdbValue.MakeInt32(sprintReplyNumber)},
            {"$date", YdbValue.MakeOptionalDatetime(date)},
            {"$data", YdbValue.MakeOptionalJson(data)}
        });

        logger.LogInformation($"Добавили для пользователя {chatId} возможность начать сценарий {scenarioId}");

        return scenarioToStartId;
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
                current_sprint_number Int64,
                sprint_reply_number Int32,
                index Int32,
                date DATETIME,
                data Json
                
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