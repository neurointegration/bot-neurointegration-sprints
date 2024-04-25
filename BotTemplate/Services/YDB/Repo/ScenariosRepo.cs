using System.Text;
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

    public async void AddNewScenario(string[] messages, string[] keys)
    {
        var maxId = await GetMaxScenarioIdOrNull();
        var scenarioId = maxId is not null 
            ? maxId.Value + 1 
            : 1;
        
        for (var i = 0; i < messages.Length; i++)
        {
            await botDatabase.ExecuteModify($@"
            DECLARE $scenario_id AS Int64;
            DECLARE $message_index AS Int32;
            DECLARE $message AS text;
            DECLARE $answer_key AS text;

            INSERT INTO {TableName} ( scenario_id, message_index, message, answer_key )
            VALUES ( $scenario_id, $message_index, $message, $answer_key )
        ", new Dictionary<string, YdbValue?>
            {
                { "$scenario_id", YdbValue.MakeInt64(scenarioId) },
                { "$message_index", YdbValue.MakeInt32(i) },
                { "$date_time", YdbValue.MakeString(Encoding.Default.GetBytes(messages[i])) },
                { "$answer_key", i != messages.Length - 1 
                    ? YdbValue.MakeUtf8(keys[i])
                    : null
                }
            });
        }
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
            VALUES ( 0, 0, 'Привет! Меня зовут Эмилия. Я буду помогать тебе в прохождении спринтов нейроинтеграции. Подробнее про нейтроитеграции можно узнать здесь - https://ru.neurointegration.org/science Прежде чем начать нашу работу я задам тебе пару вопросов. Напиши, пожалуйста, временной промежуток, когда тебе можно отправлять уведомления? Укажи время через - , например, 9:00-18:00', 'key1' ),
                   ( 0, 1, 'Отлично! Буду писать только в это время. А во сколько поставить уведомление о вечернем стендапе? Укажи только время. К примеру, 19:00', 'key2' ),
                   ( 0, 2, 'Супер! И последний вопрос. У тебя есть тренер? Если да, напиши его ник телеграма. Например, @superpupertrener', 'key3' ),
                   ( 0, 3, 'Спасибо за твои ответы! Я буду отправлять три раза в день в неожиданное для тебя время вопрос “Как ты себя сейчас чувствуешь?” с вариантами ответа. Также буду напоминать заполнить вечерний стендап и рефлексию. Отвечая на мои вопросы, я создам персональную exsel-табличку, где буду хранить твои ответы. Была рада знакомству!', null )
        ", new Dictionary<string, YdbValue?>());
        }
    }
}