using Neurointegration.Api.DataModels.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BotTemplate.Models;

public record QuestionRequest
{
    [JsonProperty("time")]
    public int? Time { get; set; }
    
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("scenario_type")]
    public ScenarioType? ScenarioType { get; set; }

    // обязателен для корректной дессериализации
    public QuestionRequest()
    {
    }
}