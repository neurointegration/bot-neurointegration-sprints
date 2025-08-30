using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BotTemplate;

public record Configuration
{
    public string TelegramToken { get; set; }
    public string YdbEndpoint { get; set; }
    public string YdbPath { get; set; }
    public string LoginUrl { get; set; }
    public string? LoginBotUsername { get; set; }
    public string? IamTokenPath { get; set; }
    public string? BackendApiKey { get; set; }
    public string? BackendBaseUrl { get; set; }
    public string? TriggerFrequencyMinutes { get; set; }

    private Configuration()
    {
    }

    public static Configuration FromJson(string path)
    {
        var text = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<Configuration>(text) ?? throw new ArgumentException("Не заданы настройки бота");
    }

    public static Configuration FromEnvironment()
    {
        return new Configuration()
        {
            TelegramToken = Environment.GetEnvironmentVariable("NEURO_TELEGRMA_TOKEN") ??
                            throw new ArgumentException("Не задана переменная NEURO_TELEGRMA_TOKEN"),

            YdbEndpoint = Environment.GetEnvironmentVariable("NEURO_YDB_ENDPOINT") ??
                          throw new ArgumentException("Не задана переменная NEURO_YDB_ENDPOINT"),

            YdbPath = Environment.GetEnvironmentVariable("NEURO_YDB_PATH") ??
                      throw new ArgumentException("Не задана переменная NEURO_YDB_PATH"),

            LoginUrl = Environment.GetEnvironmentVariable("LOGIN_URL") ??
                      throw new ArgumentException("Не задана переменная LOGIN_URL"),

            LoginBotUsername = Environment.GetEnvironmentVariable("LOGIN_BOT_USERNAME"),
            IamTokenPath = Environment.GetEnvironmentVariable("IAM_TOKEN_PATH"),
            BackendApiKey = Environment.GetEnvironmentVariable("NEURO_BACKEND_APIKEY"),
            BackendBaseUrl = Environment.GetEnvironmentVariable("NEURO_BACKEND_URL"),
            TriggerFrequencyMinutes = Environment.GetEnvironmentVariable("NEURO_TRIGGER_FREQUENCY_MINUTES"),
        };
    }
}