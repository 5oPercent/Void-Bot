using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.Authorization;

public class OIDCDto
{
    [JsonPropertyName("ns")]
    public string Ns { get; set; } = string.Empty;

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = string.Empty;

    [JsonPropertyName("op_endpoint")]
    public string OpEndpoint { get; set; } = string.Empty;

    [JsonPropertyName("claimed_id")]
    public string ClaimedId { get; set; } = string.Empty;

    [JsonPropertyName("identity")]
    public string Identity { get; set; } = string.Empty;

    [JsonPropertyName("return_to")]
    public string ReturnTo { get; set; } = string.Empty;

    [JsonPropertyName("response_nonce")]
    public string ResponseNonce { get; set; } = string.Empty;

    [JsonPropertyName("assoc_handle")]
    public string AssocHandle { get; set; } = string.Empty;

    [JsonPropertyName("signed")]
    public string Signed { get; set; } = string.Empty;

    [JsonPropertyName("sig")]
    public string Sig { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}