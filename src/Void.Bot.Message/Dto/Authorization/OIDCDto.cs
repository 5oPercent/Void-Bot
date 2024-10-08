namespace Void.Bot.Message.Dto.Authorization;

public class OIDCDto
{
    public string? Ns { get; set; }
    
    public string? Mode { get; set; }
    
    public string? Op_endpoint { get; set; }

    public string? Claimed_id { get; set; } = string.Empty;
    
    public string? Identity { get; set; }
    
    public string? Return_to { get; set; }
    
    public string? Response_nonce { get; set; }
    
    public string? Assoc_handle { get; set; }
    
    public string? Signed { get; set; }
    
    public string? Sig { get; set; }
    
    public string? Error { get; set; }
}