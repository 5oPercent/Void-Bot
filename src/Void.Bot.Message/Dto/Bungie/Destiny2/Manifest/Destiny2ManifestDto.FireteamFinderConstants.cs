namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2火力战队查找器常量定义，描述游戏中火力战队查找系统的全局配置
/// </summary>
public class Destiny2ManifestFireteamFinderConstantsDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含火力战队查找器系统的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 火力战队查找器活动图表根类别哈希列表，定义火力战队查找器中活动类别的顶层分类
    /// </summary>
    public List<uint> FireteamFinderActivityGraphRootCategoryHashes { get; set; } = [];
    
    /// <summary>
    /// 所有火力战队查找器活动哈希列表，包含所有支持火力战队查找功能的活动哈希值
    /// </summary>
    public List<uint> AllFireteamFinderActivityHashes { get; set; } = [];

    /// <summary>
    /// 守护者誓言显示属性，描述火力战队查找器中守护者誓言的显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto GuardianOathDisplayProperties { get; set; } = new();

    /// <summary>
    /// 守护者誓言信条列表，包含火力战队查找器中守护者需遵守的行为准则
    /// </summary>
    public List<Destiny2ManifestDisplayPropertiesDefinitionDto> GuardianOathTenets { get; set; } = [];
}

/// <summary>
/// 命运2火力战队查找器常量清单，包含火力战队查找系统全局配置的定义集合
/// </summary>
public class Destiny2ManifestFireteamFinderConstantsDto : Dictionary<string, Destiny2ManifestFireteamFinderConstantsDefinitionDto>
{
}