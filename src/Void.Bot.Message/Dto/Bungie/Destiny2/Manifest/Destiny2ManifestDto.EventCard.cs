namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 事件卡片定义
/// </summary>
public class Destiny2ManifestEventCardDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 链接重定向路径
    /// </summary>
    public string LinkRedirectPath { get; set; } = string.Empty;

    /// <summary>
    /// 事件卡片颜色
    /// </summary>
    public Destiny2ManifestColorDefinitionDto Color { get; set; } = new();

    /// <summary>
    /// 事件卡片相关图像
    /// </summary>
    public Destiny2ManifestEventCardImagesDto Images { get; set; } = new();

    /// <summary>
    /// 所有权解锁标志哈希
    /// </summary>
    public uint OwnershipUnlockFlagHash { get; set; }

    /// <summary>
    /// 成就展示节点哈希
    /// </summary>
    public uint TriumphsPresentationNodeHash { get; set; }
    
    /// <summary>
    /// 印章展示节点哈希
    /// </summary>
    public uint SealPresentationNodeHash { get; set; }

    /// <summary>
    /// 事件卡片货币列表
    /// </summary>
    public List<object> EventCardCurrencyList { get; set; } = [];
    
    /// <summary>
    /// 票券货币物品哈希
    /// </summary>
    public uint TicketCurrencyItemHash { get; set; }
    
    /// <summary>
    /// 票券商人哈希
    /// </summary>
    public uint TicketVendorHash { get; set; }
    
    /// <summary>
    /// 票券商人分类哈希
    /// </summary>
    public uint TicketVendorCategoryHash { get; set; }

    /// <summary>
    /// 事件结束时间
    /// </summary>
    public long EndTime { get; set; }

    /// <summary>
    /// 结束时间覆盖解锁值哈希
    /// </summary>
    public uint EndTimeOverrideUnlockValueHash { get; set; }

    /// <summary>
    /// UI主题哈希
    /// </summary>
    public uint UiThemeHash { get; set; }

    /// <summary>
    /// 每周挑战展示节点哈希
    /// </summary>
    public uint WeeklyChallengesPresentationNodeHash { get; set; }
}

/// <summary>
/// 事件卡片图像资源定义
/// </summary>
public class Destiny2ManifestEventCardImagesDto
{
    /// <summary>
    /// 未拥有卡片封套图像路径
    /// </summary>
    public string UnownedCardSleeveImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 未拥有卡片封套包装图像路径
    /// </summary>
    public string UnownedCardSleeveWrapImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 卡片未完成图像路径
    /// </summary>
    public string CardIncompleteImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 卡片完成图像路径
    /// </summary>
    public string CardCompleteImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 卡片完成包装图像路径
    /// </summary>
    public string CardCompleteWrapImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 进度图标图像路径
    /// </summary>
    public string ProgressIconImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 主题背景图像路径
    /// </summary>
    public string ThemeBackgroundImagePath { get; set; } = string.Empty;
}

/// <summary>
/// 事件卡片清单
/// </summary>
public class Destiny2ManifestEventCardDto : Dictionary<string, Destiny2ManifestEventCardDefinitionDto>
{
}