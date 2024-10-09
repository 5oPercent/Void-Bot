using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Void.Bot.Core.Domain;

[Table("message_log")]
public class MessageLog : IEntity, IEntityCreated
{
    public MessageLog()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.Now;
        MessageType = string.Empty;
        MessageJson = string.Empty;
        ResultType = string.Empty;
        ResultJson = string.Empty;
    }
    
    [Column("id")]
    public Guid Id { get; set; }

    [Column("message_type"), StringLength(128)]
    public string MessageType { get; set; }
    
    [Column("result_type"), StringLength(128)]
    public string ResultType { get; set; }
    
    [Column("message_json")]
    public string MessageJson { get; set; }
    
    [Column("result_json")]
    public string ResultJson { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedAt { get; set; }
}