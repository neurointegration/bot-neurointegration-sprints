using Neurointegration.Api.DataModels.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BotTemplate.Models;

public record QuestionRequest
{
    [JsonProperty("time", Required = Required.AllowNull)]
    public int? Time { get; set; }
    
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("scenario_type", Required = Required.AllowNull)]
    public ScenarioType? ScenarioType { get; set; }

    public QuestionRequest()
    {
        
    }
}