using BotTemplate.Models.Telegram;
using BotTemplate.Services.YDB.Repo;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ydb.Sdk.Value;

namespace BotTemplate.Services.YDB;

public class ScenarioStateRepository : IRepository
{
    public virtual string TableName => "scenario_state";

    private readonly IBotDatabase botDatabase;
    private readonly ILogger logger;

    public ScenarioStateRepository(IBotDatabase botDatabase, ILogger logger)
    {
        this.botDatabase = botDatabase;
        this.logger = logger;
    }
    
    public async Task StartNewScenario(
        long chatId,
        string scenarioId,
        DateTime? date = null,
        long currentSprintNumber = 0,
        int sprintReplyNumber = 0,
        string? data = null)
    {
        await botDatabase.ExecuteModify($@"
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
            {"$chat_id", YdbValue.MakeInt64(chatId)},
            {"$scenario_id", YdbValue.MakeUtf8(scenarioId)},
            {"$current_sprint_number", YdbValue.MakeInt64(currentSprintNumber)},
            {"$sprint_reply_number", YdbValue.MakeInt32(sprintReplyNumber)},
            {"$date", YdbValue.MakeOptionalDatetime(date)},
            {"$data", YdbValue.MakeOptionalJson(data)}
        });

        logger.LogInformation($"Начали для пользователя {chatId} сценарий {scenarioId}");
    }

    public async Task IncreaseAndGetNewMessage(long chatId)
    {
        var oldIndex = await GetIndexByChatId(chatId);
        if (oldIndex is null)
            return;

        var scenarioId = await GetScenarioIdByChatId(chatId);
        var newIndex = oldIndex.Value + 1;

        await botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;
            DECLARE $scenario_id AS Utf8;
            DECLARE $new_index AS Int32;

            UPDATE {TableName}
            SET index = $new_index, scenario_id = $scenario_id
            WHERE chat_id = $chat_id;
        ", new Dictionary<string, YdbValue?>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)},
            {"$scenario_id", YdbValue.MakeUtf8(scenarioId!)},
            {"$new_index", YdbValue.MakeInt32(newIndex)}
        });
    }

    public async Task DecreaseIndex(long chatId)
    {
        var oldIndex = await GetIndexByChatId(chatId);
        if (oldIndex is null)
            return;

        var scenarioId = await GetScenarioIdByChatId(chatId);
        var newIndex = oldIndex.Value - 1;

        await botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;
            DECLARE $scenario_id AS Utf8;
            DECLARE $new_index AS Int32;

            UPDATE {TableName}
            SET index = $new_index, scenario_id = $scenario_id
            WHERE chat_id = $chat_id;
        ", new Dictionary<string, YdbValue?>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)},
            {"$scenario_id", YdbValue.MakeUtf8(scenarioId!)},
            {"$new_index", YdbValue.MakeInt32(newIndex)}
        });
    }

    public async Task EndScenarioNoMatterWhat(long chatId)
    {
        var oldIndex = await GetIndexByChatId(chatId);
        if (oldIndex is null)
            return;
        await botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;

            DELETE FROM {TableName} WHERE chat_id = $chat_id;
        ", new Dictionary<string, YdbValue?>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)}
        });
    }

    public async Task<string?> GetScenarioIdByChatId(long chatId)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $chat_id AS Int64;

            SELECT scenario_id
            FROM {TableName}
            WHERE chat_id = $chat_id
        ", new Dictionary<string, YdbValue>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)}
        });

        if (rows is null)
        {
            return null;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any()
            ? null
            : rowsArray.First()["scenario_id"].GetUtf8();
    }

    public async Task<int?> GetIndexByChatId(long chatId)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $chat_id AS Int64;

            SELECT index
            FROM {TableName}
            WHERE chat_id = $chat_id
        ", new Dictionary<string, YdbValue>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)}
        });

        if (rows is null)
        {
            return null;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any()
            ? null
            : rowsArray.First()["index"].GetOptionalInt32();
    }

    public async Task<CurrentScenarioInfo?> GetInfoByChatId(long chatId)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $chat_id AS Int64;

            SELECT scenario_id, current_sprint_number, sprint_reply_number, index, date, data
            FROM {TableName}
            WHERE chat_id = $chat_id
        ", new Dictionary<string, YdbValue>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)}
        });

        if (rows is null)
        {
            return null;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any()
            ? null
            : new CurrentScenarioInfo
            {
                ChatId = chatId,
                ScenarioId = rowsArray.First()["scenario_id"].GetUtf8(),
                CurrentSprintNumber = rowsArray.First()["current_sprint_number"].GetOptionalInt64(),
                SprintReplyNumber = rowsArray.First()["sprint_reply_number"].GetOptionalInt32(),
                Index = rowsArray.First()["index"].GetOptionalInt32(),
                Date = rowsArray.First()["date"].GetOptionalDatetime(),
                Data = rowsArray.First()["data"].GetOptionalJson()
            };
    }

    public async Task UpdateData<T>(long chatId, T? scenarioData)
    {
        await botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;
            DECLARE $data AS Json?;

            UPDATE {TableName}
            SET data = $data
            WHERE chat_id = $chat_id;
        ", new Dictionary<string, YdbValue?>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)},
            {"$data", YdbValue.MakeOptionalJson(JsonConvert.SerializeObject(scenarioData))},
        });
    }

    public async Task CreateTable()
    {
        try
        {
            await botDatabase.ExecuteScheme($@"
            CREATE TABLE {TableName} (
                chat_id Int64 NOT NULL,
                scenario_id Utf8 NOT NULL,
                current_sprint_number Int64,
                sprint_reply_number Int32,
                index Int32,
                date DATETIME,
                data Json
                
                PRIMARY KEY (chat_id)
            )
        ");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}