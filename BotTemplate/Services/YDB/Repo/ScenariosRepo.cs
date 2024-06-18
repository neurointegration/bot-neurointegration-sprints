using Ydb.Sdk.Value;

namespace BotTemplate.Services.YDB.Repo;

public class ScenariosRepo : IRepo
{
    protected virtual string TableName => "scenarios";

    private readonly IBotDatabase botDatabase;

    private ScenariosRepo(IBotDatabase botDatabase)
    {
        this.botDatabase = botDatabase;
    }

    public static async Task<ScenariosRepo> InitWithDatabase(IBotDatabase botDatabase)
    {
        var model = new ScenariosRepo(botDatabase);
        await model.CreateTable();
        return model;
    }
    
    public async Task<string?> GetMessageByScenarioIdAndMessageIndex(long scenarioId, int messageIndex)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $scenario_id AS Int64;
            DECLARE $message_index AS Int32;

            SELECT message
            FROM {TableName}
            WHERE scenario_id = $scenario_id AND message_index = $message_index
        ", new Dictionary<string, YdbValue>
        {
            {"$scenario_id", YdbValue.MakeInt64(scenarioId)},
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
    
    public async Task<string?[]> GetAllMessagesByScenarioId(long scenarioId)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $scenario_id AS Int64;

            SELECT message
            FROM {TableName}
            WHERE scenario_id = $scenario_id
        ", new Dictionary<string, YdbValue>
        {
            {"$scenario_id", YdbValue.MakeInt64(scenarioId)}
        });
        
        if (rows is null)
        {
            return Array.Empty<string>();
        }
        
        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any() 
            ? Array.Empty<string>() 
            : rowsArray.Select(message => message["message"].GetOptionalUtf8()).ToArray();
    }

    public async Task<string?> GetKeyByIndex(long scenarioId, int messageIndex)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $scenario_id AS Int64;
            DECLARE $message_index AS Int32;

            SELECT answer_key
            FROM {TableName}
            WHERE scenario_id = $scenario_id AND message_index = $message_index
        ", new Dictionary<string, YdbValue>
        {
            {"$scenario_id", YdbValue.MakeInt64(scenarioId)},
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
        await botDatabase.ExecuteScheme($@"
            CREATE TABLE {TableName} (
                scenario_id Int64 NOT NULL,
                message_index Int32 NOT NULL,
                message text,
                answer_key text,
                PRIMARY KEY (scenario_id, message_index)
            )
        ");

        var isThereAnyScenarios = await IsThereAnyScenarios();
        if (!isThereAnyScenarios)
        {
            await botDatabase.ExecuteModify($@"
            INSERT INTO {TableName} ( scenario_id, message_index, message, answer_key )
            VALUES ( 1, 0, '/stateMessage', 'key1' ),
                   ( 1, 1, '/handleStateResponse', null ),
                   ( 2, 0, 'Время заполнять стендап! Какие победы были сегодня?', 'key1' ),
                   ( 2, 1, 'Отлично! Какой лайв берешь на ближайшие сутки?', 'key2' ),
                   ( 2, 2, 'Что насчет кайфа?', 'key3' ),
                   ( 2, 3, 'А как там с драйвом?', 'key4' ),
                   ( 2, 4, 'Успехов. До завтра!', null ),
                   ( 3, 0, 'Что сделал по своим проектам на этой неделе?', 'key1' ),
                   ( 3, 1, 'А что не сделал по своим проектам на этой неделе?', 'key2' ),
                   ( 3, 2, 'Что влияло на твое состояние на этой неделе? Как в позитивном, так и в негативном ключе', 'key3' ),
                   ( 3, 3, 'Каким образом в поведении на этой неделе проявлялись орбиты? Как выруливал?', 'key4' ),
                   ( 3, 4, 'Что изменишь на следующей неделе?', 'key5' ),
                   ( 3, 5, 'Желаю успехов в новой неделе!', null ),
                   ( 4, 0, 'Какое главное изменение замечаешь?', 'key1' ),
                   ( 4, 1, 'Какие твои действия к этому привели?', 'key2' ),
                   ( 4, 2, 'Какие твои способности мне помогли?', 'key3' ),
                   ( 4, 3, 'Как изменились твои убеждения о том, что возможно?', 'key4' ),
                   ( 4, 4, 'Как изменились твои убеждения о себе и отношения с собой?', 'key5' ),
                   ( 4, 5, 'Какие возможности теперь для тебя доступны?', 'key6' ),
                   ( 4, 6, 'Желаю успехов в новой неделе!', null )
        ", new Dictionary<string, YdbValue?>());
        }
    }
}