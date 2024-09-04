using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Void.Bot.Core.Domain;

[Table("message_log")]
public class MessageLog : IEntity
{
    public MessageLog()
    {
        Id = Guid.NewGuid();
        CreatedDate = DateTimeOffset.Now;
    }
    
    [Column("id")]
    public Guid Id { get; set; }

    [Column("message_type"), StringLength(128)]
    public required string MessageType { get; set; }
    
    [Column("result_type"), StringLength(128)]
    public required string ResultType { get; set; }
    
    [Column("message_json")]
    public required string MessageJson { get; set; }
    
    [Column("result_json")]
    public required string ResultJson { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}