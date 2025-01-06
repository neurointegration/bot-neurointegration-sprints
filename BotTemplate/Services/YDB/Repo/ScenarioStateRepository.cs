using BotTemplate.Models.Telegram;
using BotTemplate.Services.YDB.Repo;
using Ydb.Sdk.Value;

namespace BotTemplate.Services.YDB;

public class ScenarioStateRepository : IRepository
{
    public virtual string TableName => "scenario_state";

    private readonly IBotDatabase _botDatabase;
    private readonly ScenariosRepository scenariosRepository;

    private ScenarioStateRepository(IBotDatabase botDatabase, ScenariosRepository scenariosRepository)
    {
        _botDatabase = botDatabase;
        this.scenariosRepository = scenariosRepository;
    }

    public async Task<string?> StartNewScenarioAndGetMessage(long chatId, string scenarioId, DateTime? date = null, long currentSprintNumber = 0,
        int sprintReplyNumber = 0)
    {
        var curIndex = await GetIndexByChatId(chatId);
        if (curIndex is not null)
            return null;

        await _botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;
            DECLARE $scenario_id AS Utf8;
            DECLARE $current_sprint_number AS Int64;
            DECLARE $sprint_reply_number AS Int32;
            DECLARE $date AS DATETIME?;

            INSERT INTO {TableName} ( chat_id, scenario_id, current_sprint_number, sprint_reply_number, index, date )
            VALUES ( $chat_id, $scenario_id, $current_sprint_number, $sprint_reply_number, 0, $date )
        ", new Dictionary<string, YdbValue?>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)},
            {"$scenario_id", YdbValue.MakeUtf8(scenarioId)},
            {"$current_sprint_number", YdbValue.MakeInt64(currentSprintNumber)},
            {"$sprint_reply_number", YdbValue.MakeInt32(sprintReplyNumber)},
            {"$date", YdbValue.MakeOptionalDatetime(date)}
        });

        var message = await scenariosRepository.GetMessageByScenarioIdAndMessageIndex(scenarioId, 0);
        return message;
    }

    public async Task<string?> IncreaseAndGetNewMessage(long chatId)
    {
        var oldIndex = await GetIndexByChatId(chatId);
        if (oldIndex is null)
            return null;

        var scenarioId = await GetScenarioIdByChatId(chatId);
        var newIndex = oldIndex.Value + 1;
        var message = await scenariosRepository.GetMessageByScenarioIdAndMessageIndex(scenarioId!, newIndex);

        await _botDatabase.ExecuteModify($@"
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

        return message;
    }

    public async Task DecreaseIndex(long chatId)
    {
        var oldIndex = await GetIndexByChatId(chatId);
        if (oldIndex is null)
            return;

        var scenarioId = await GetScenarioIdByChatId(chatId);
        var newIndex = oldIndex.Value - 1;

        await _botDatabase.ExecuteModify($@"
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

    public async Task TryEndScenario(long chatId)
    {
        var scenarioId = await GetScenarioIdByChatId(chatId);
        var oldIndex = await GetIndexByChatId(chatId);
        if (oldIndex is null)
            return;
        var key = await scenariosRepository.GetKeyByIndex(scenarioId!, oldIndex.Value);
        if (key is not null)
            return;
        await _botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;

            DELETE FROM {TableName} WHERE chat_id = $chat_id;
        ", new Dictionary<string, YdbValue?>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)}
        });
    }

    public async Task EndScenarioNoMatterWhat(long chatId)
    {
        var oldIndex = await GetIndexByChatId(chatId);
        if (oldIndex is null)
            return;
        await _botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;

            DELETE FROM {TableName} WHERE chat_id = $chat_id;
        ", new Dictionary<string, YdbValue?>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)}
        });
    }

    public async Task<string?> GetCurrentKey(long chatId)
    {
        var scenarioId = await GetScenarioIdByChatId(chatId);
        if (scenarioId is null)
            return null;

        var index = await GetIndexByChatId(chatId);

        return await scenariosRepository.GetKeyByIndex(scenarioId, index!.Value);
    }

    public async Task<string?> GetScenarioIdByChatId(long chatId)
    {
        var rows = await _botDatabase.ExecuteFind($@"
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
        var rows = await _botDatabase.ExecuteFind($@"
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
        var rows = await _botDatabase.ExecuteFind($@"
            DECLARE $chat_id AS Int64;

            SELECT scenario_id, current_sprint_number, sprint_reply_number, index, date
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
            };
    }

    public async Task ClearAll()
    {
        await _botDatabase.ExecuteScheme($@"
            DROP TABLE {TableName};
        ");
    }

    public async Task CreateTable()
    {
        try
        {
            await _botDatabase.ExecuteScheme($@"
            CREATE TABLE {TableName} (
                chat_id Int64 NOT NULL,
                scenario_id Utf8 NOT NULL,
                current_sprint_number Int64,
                sprint_reply_number Int32,
                index Int32,
                date DATETIME,
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