namespace Void.Bot.Message.Enum.ERA;

public enum EraTeamJoinMode
{
    Public = 0,
    HostFriends = 1,
    InvitationOnly = 2,
    Solo = 3
}

public enum EraTeamInvitationPermission
{
    AnyMember = 0,
    HostOnly = 1
}

public enum EraTeamInvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Withdrawn = 3,
    Expired = 4,
    Invalidated = 5
}
