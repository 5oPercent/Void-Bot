using System.ComponentModel.DataAnnotations;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

public interface IRequestDestiny2ByPlayerNameCode
{
    string DisplayName { get; set; }
    
    string DisplayNameCode { get; set; }

    BungieMembershipTypeEnum MembershipType { get; set; }
}