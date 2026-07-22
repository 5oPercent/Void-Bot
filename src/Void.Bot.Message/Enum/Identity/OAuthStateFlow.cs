using System.Text.Json.Serialization;

namespace Void.Bot.Message.Enum.Identity;

[JsonConverter(typeof(JsonStringEnumConverter<OAuthStateFlow>))]
public enum OAuthStateFlow
{
    [JsonStringEnumMemberName("login")]
    Login,

    [JsonStringEnumMemberName("link")]
    Link
}
