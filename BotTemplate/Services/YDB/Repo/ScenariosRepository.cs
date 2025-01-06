using Ydb.Sdk.Value;

namespace BotTemplate.Services.YDB.Repo;

public class ScenariosRepository : IRepository
{
    protected virtual string TableName => "scenarios2";

    private readonly IBotDatabase botDatabase;

    public ScenariosRepository(IBotDatabase botDatabase)
    {
        this.botDatabase = botDatabase;
    }

    public async Task<string?> GetMessageByScenarioIdAndMessageIndex(string scenarioId, int messageIndex)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $scenario_id AS Utf8;
            DECLARE $message_index AS Int32;

            SELECT message
            FROM {TableName}
            WHERE scenario_id = $scenario_id AND message_index = $message_index
        ", new Dictionary<string, YdbValue>
        {
            {"$scenario_id", YdbValue.MakeUtf8(scenarioId)},
            {"$message_index", YdbValue.MakeInt32(messageIndex)}
        });

        if (rows is null)
        {
            return null;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any()
            ? null
            : rowsArray.First()["message"].GetOptionalUtf8();
    }

    public async Task<string?> GetKeyByIndex(string scenarioId, int messageIndex)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $scenario_id AS Utf8;
            DECLARE $message_index AS Int32;

            SELECT answer_key
            FROM {TableName}
            WHERE scenario_id = $scenario_id AND message_index = $message_index
        ", new Dictionary<string, YdbValue>
        {
            {"$scenario_id", YdbValue.MakeUtf8(scenarioId)},
            {"$message_index", YdbValue.MakeInt32(messageIndex)}
        });

        if (rows is null)
        {
            return null;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any()
            ? null
            : rowsArray.First()["answer_key"].GetOptionalUtf8();
    }

    private async Task<long?> GetMaxScenarioIdOrNull()
    {
        var rows = await botDatabase.ExecuteFind($@"
            SELECT MAX(scenario_id) max_scenario_id
            FROM {TableName}
        ", new Dictionary<string, YdbValue>());

        if (rows is null)
        {
            return null;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any()
            ? null
            : rowsArray.First()["max_scenario_id"].GetOptionalInt64();
    }

    private async Task<bool> IsThereAnyScenarios()
    {
        var rows = await botDatabase.ExecuteFind($@"
            SELECT DISTINCT (scenario_id)
            FROM {TableName}
        ", new Dictionary<string, YdbValue>());

        if (rows is null)
        {
            return false;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return rowsArray.Any();
    }

    public async Task ClearAll()
    {
        await botDatabase.ExecuteScheme($@"
            DROP TABLE {TableName};
        ");
    }

    public async Task CreateTable()
    {
        try
        {
            await botDatabase.ExecuteScheme($@"
            CREATE TABLE {TableName} (
                scenario_id Utf8 NOT NULL,
                message_index Int32 NOT NULL,
                message text,
                answer_key text,
                PRIMARY KEY (scenario_id, message_index)
            )
        ");
        }
        catch (Exception)
        {
            // ignored
        }

        var isThereAnyScenarios = await IsThereAnyScenarios();
        if (!isThereAnyScenarios)
        {
            await botDatabase.ExecuteModify($@"
            INSERT INTO {TableName} ( scenario_id, message_index, message, answer_key )
            VALUES ( 'regular_status', 0, '/stateMessage', 'key1' ),
                   ( 'regular_status', 1, '/handleStateResponse', null ),
                   ( 'regular_evening_standup', 0, 'Время заполнять стендап! Какие победы были сегодня?', 'key1' ),
                   ( 'regular_evening_standup', 1, 'Отлично! Какой лайв берешь на ближайшие сутки?', 'key2' ),
                   ( 'regular_evening_standup', 2, 'Что насчет кайфа?', 'key3' ),
                   ( 'regular_evening_standup', 3, 'А как там с драйвом?', 'key4' ),
                   ( 'regular_evening_standup', 4, 'Успехов. До завтра!', null ),
                   ( 'regular_weekend_reflection', 0, 'Что сделал по своим проектам на этой неделе?', 'key1' ),
                   ( 'regular_weekend_reflection', 1, 'А что не сделал по своим проектам на этой неделе?', 'key2' ),
                   ( 'regular_weekend_reflection', 2, 'Что влияло на твое состояние на этой неделе? Как в позитивном, так и в негативном ключе', 'key3' ),
                   ( 'regular_weekend_reflection', 3, 'Каким образом в поведении на этой неделе проявлялись орбиты? Как выруливал?', 'key4' ),
                   ( 'regular_weekend_reflection', 4, 'Что изменишь на следующей неделе?', 'key5' ),
                   ( 'regular_weekend_reflection', 5, 'Желаю успехов в новой неделе!', null ),
                   ( 'last_regular_weekend_reflection', 0, 'Какое главное изменение замечаешь?', 'key1' ),
                   ( 'last_regular_weekend_reflection', 1, 'Какие твои действия к этому привели?', 'key2' ),
                   ( 'last_regular_weekend_reflection', 2, 'Какие твои способности мне помогли?', 'key3' ),
                   ( 'last_regular_weekend_reflection', 3, 'Как изменились твои убеждения о том, что возможно?', 'key4' ),
                   ( 'last_regular_weekend_reflection', 4, 'Как изменились твои убеждения о себе и отношения с собой?', 'key5' ),
                   ( 'last_regular_weekend_reflection', 5, 'Какие возможности теперь для тебя доступны?', 'key6' ),
                   ( 'last_regular_weekend_reflection', 6, 'Желаю успехов в новой неделе!', null )
        ", new Dictionary<string, YdbValue?>());
        }
    }
}