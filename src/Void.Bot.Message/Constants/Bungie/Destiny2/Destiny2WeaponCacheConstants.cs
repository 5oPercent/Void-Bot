namespace Void.Bot.Message.Constants.Bungie.Destiny2;

public static class Destiny2WeaponCacheConstants
{
    public const string Prefix = $"{Destiny2Constants.Destiny2}:Weapon";

    public static string GetTypesKey(string version, string language)
        => $"{Prefix}:{version}:{language}:Types";

    public static string GetPerkVariantGroupsKey(string version, string language)
        => $"{Prefix}:{version}:{language}:PerkVariantGroups";

    public static string GetWeaponTypeKey(string version, string language, int weaponType)
        => $"{Prefix}:{version}:{language}:Type:{weaponType}";
}
