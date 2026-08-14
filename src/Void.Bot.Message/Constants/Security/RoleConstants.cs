namespace Void.Bot.Message.Constants.Security;

public class RoleConstants
{
    public static readonly Guid SuperAdminId = Guid.Parse("10000000-0000-0000-0000-000000000000");

    public const string SuperAdmin = nameof(SuperAdmin);

    public const string SuperAdminBootstrapClaim = "super_admin";
}