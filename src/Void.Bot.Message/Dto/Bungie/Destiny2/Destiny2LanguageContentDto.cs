using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2LanguageContentDto
{
    public string En { get; set; } = string.Empty;
    
    public string Fr { get; set; } = string.Empty;
    
    public string Es { get; set; } = string.Empty;
    
    [JsonPropertyName("es-mx")]
    public string EsMx { get; set; } = string.Empty;
    
    public string De { get; set; } = string.Empty;
    
    public string It { get; set; } = string.Empty;
    
    public string Ja { get; set; } = string.Empty;
    
    [JsonPropertyName("pt-br")]
    public string PtBr { get; set; } = string.Empty;
    
    public string Ru { get; set; } = string.Empty;
    
    public string Pl { get; set; } = string.Empty;
    
    public string Ko { get; set; } = string.Empty;
    
    [JsonPropertyName("zh-cht")]
    public string ZhCht { get; set; } = string.Empty;
    
    [JsonPropertyName("zh-chs")]
    public string ZhChs { get; set; } = string.Empty;
}