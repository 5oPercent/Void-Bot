using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运组件类型枚举
/// </summary>
public enum Destiny2ComponentTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 档案是最基本的组件，仅在调用 GetProfile 时相关。这将返回有关档案的基本信息，几乎没有什么：角色 ID 列表、上次登录时间的一些信息，以及最令人清醒的统计数据：你玩了多长时间。
    /// </summary>
    [Description("档案")]
    Profiles = 100,

    /// <summary>
    /// 仅适用于 GetProfile，这将返回有关可退款供应商物品的收据信息。
    /// </summary>
    [Description("供应商收据")]
    VendorReceipts = 101,

    /// <summary>
    /// 请求此项将获取档案级别的库存，例如你的保险库桶（是的，保险库实际上是位于你的档案上的库存桶）
    /// </summary>
    [Description("档案库存")]
    ProfileInventories = 102,

    /// <summary>
    /// 这将为你提供我们认为是“货币”的档案级物品的摘要，例如 Glimmer。我是说，如果 Destiny 2 中有 Glimmer。我没说有 Glimmer。
    /// </summary>
    [Description("档案货币")]
    ProfileCurrencies = 103,

    /// <summary>
    /// 这将为你提供存在于档案级别的任何进度相关信息，跨所有角色。
    /// </summary>
    [Description("档案进度")]
    ProfileProgression = 104,

    /// <summary>
    /// 这将为你提供该档案在其玩的每个平台上的银币信息。你只能为登录用户的档案请求此组件，如果你为其他档案请求它，将不会收到它。
    /// </summary>
    [Description("平台银币")]
    PlatformSilver = 105,

    /// <summary>
    /// 这将为你提供档案中每个角色的摘要信息。
    /// </summary>
    [Description("角色")]
    Characters = 200,

    /// <summary>
    /// 这将为你提供有关角色或相关角色的任何未装备物品的信息，如果你被允许查看它。你必须是该用户的身份验证用户，或者该用户必须允许在 Bungie.Net 设置中匿名查看其未装备物品，才能实际获得结果。
    /// </summary>
    [Description("角色库存")]
    CharacterInventories = 201,

    /// <summary>
    /// 这将为你提供与每个角色相关的进度（派系、经验等“等级”）的信息，如果你是当前身份验证用户或用户选择允许匿名查看其进度信息。
    /// </summary>
    [Description("角色进度")]
    CharacterProgressions = 202,

    /// <summary>
    /// 这将为你提供足够的信息，以便在你编写了 Destiny 角色的 3D 渲染库时渲染角色，或者“借用”我们的库。没关系，如果你使用它，我不会告诉任何人。我不是告密者。（实际上，我们不在乎你是否使用它 - 尽情使用吧）
    /// </summary>
    [Description("角色渲染数据")]
    CharacterRenderData = 203,

    /// <summary>
    /// 这将返回用户可以看到的活动信息及其限制，如果你是当前身份验证用户或用户选择允许匿名查看其进度信息。请注意，此返回的数据在某些情况下可能会出现问题且相对不可靠。我们最终会努力使其更加一致可靠。
    /// </summary>
    [Description("角色活动")]
    CharacterActivities = 204,

    /// <summary>
    /// 这将返回角色的装备物品信息。每个人都可以看到这些信息。
    /// </summary>
    [Description("角色装备")]
    CharacterEquipment = 205,

    /// <summary>
    /// 这将返回角色的负载信息。
    /// </summary>
    [Description("角色负载")]
    CharacterLoadouts = 206,

    /// <summary>
    /// 这将返回有关实例化物品的基本信息 - 它们是否可以装备、其跟踪状态以及许多常见需求的信息（当前伤害类型、主要属性值等）
    /// </summary>
    [Description("物品实例")]
    ItemInstances = 300,

    /// <summary>
    /// 物品可以绑定目标（DestinyObjectiveDefinition）。如果它们有，这将返回具有此类绑定目标的物品的信息。
    /// </summary>
    [Description("物品目标")]
    ItemObjectives = 301,

    /// <summary>
    /// 物品可以有特权（DestinySandboxPerkDefinition）。如果它们有，这将返回物品上激活的特权信息。
    /// </summary>
    [Description("物品特权")]
    ItemPerks = 302,

    /// <summary>
    /// 如果你只想渲染武器，这只是足够的信息来进行渲染。
    /// </summary>
    [Description("物品渲染数据")]
    ItemRenderData = 303,

    /// <summary>
    /// 物品可以有统计数据，例如射速。请求此组件将返回请求物品的统计数据（如果它们有统计数据）。
    /// </summary>
    [Description("物品统计")]
    ItemStats = 304,

    /// <summary>
    /// 物品可以有插槽，可以插入插头。请求此组件将返回所有与物品插槽相关的信息（如果它们有插槽）。
    /// </summary>
    [Description("物品插槽")]
    ItemSockets = 305,

    /// <summary>
    /// 物品可以有天赋网格，尽管这比以前少得多。请求此组件将返回有关激活节点和步骤的所有相关信息，就像过去的好时光一样。
    /// </summary>
    [Description("物品天赋网格")]
    ItemTalentGrids = 306,

    /// <summary>
    /// 未实例化的物品仍然有你需要知道的重要信息：你有多少，itemHash 以便你可以查找其 DestinyInventoryItemDefinition，它们是否被锁定等... 实例化和非实例化物品都将具有这些属性。你将自动获得库存组件 - 你只需要在调用 GetItem 时传递此项。
    /// </summary>
    [Description("物品通用数据")]
    ItemCommonData = 307,

    /// <summary>
    /// 作为“插头”的物品可以插入插槽。这将返回有关这些插头的状态以及它们为什么可以/不可以插入的状态。我听到你在笑，插入插头没什么好笑的。把你的头从沟里拿出来，注意听！
    /// </summary>
    [Description("物品插头状态")]
    ItemPlugStates = 308,

    /// <summary>
    /// 有时，插头上有目标。此数据可能非常大，因此我们将其拆分为自己的组件。请不要抓取它，除非你需要它。
    /// </summary>
    [Description("物品插头目标")]
    ItemPlugObjectives = 309,

    /// <summary>
    /// 有时，设计师会创建数千个可重复使用的插头，突然你的响应大小几乎达到 3MB，必须有所取舍。由于 Shadowkeep 中的插头更改导致插头数据对于最常见的用例来说变得不可行，可重复使用的插头被拆分为自己的组件。仅在你需要知道可以插入插槽的所有插头，并且在你的应用程序中“钻取”物品详细信息之前需要知道它时，请求此组件（例如，如果你正在进行某种有趣的排序或基于可用插头的聚合）。当你获得此组件时，你还需要将其与“插头集”数据结合使用，如果你想要所有可用插头的完整图片：此组件将仅返回具有每个物品状态数据的插头。请参阅插头集以获取具有角色、档案或无状态特定限制的可用插头。
    /// </summary>
    [Description("物品可重复使用的插头")]
    ItemReusablePlugs = 310,

    /// <summary>
    /// 获取供应商信息时，这将返回供应商或供应商的摘要信息。
    /// </summary>
    [Description("供应商")]
    Vendors = 400,

    /// <summary>
    /// 获取供应商信息时，这将返回供应商提供的物品类别信息。
    /// </summary>
    [Description("供应商类别")]
    VendorCategories = 401,

    /// <summary>
    /// 获取供应商信息时，这将返回供应商销售的物品信息。
    /// </summary>
    [Description("供应商销售")]
    VendorSales = 402,

    /// <summary>
    /// 请求此组件将返回帐户的展台状态：即已填写/获取的物品。但仅当你是当前身份验证用户或用户选择允许匿名查看其进度信息时。
    /// </summary>
    [Description("展台")]
    Kiosks = 500,

    /// <summary>
    /// 一个“快捷”组件，将为你提供请求角色可以用来确定某个操作（购买、插槽插入）是否具有所需货币的所有物品哈希/数量。（请记住，所有货币都是物品，有些供应商购买需要你可能不会传统上认为是“货币”的物品，例如插头/模组！）
    /// </summary>
    [Description("货币查找")]
    CurrencyLookups = 600,

    /// <summary>
    /// 返回有关所有“展示节点”的摘要状态信息。有关更多详细信息，请参阅 DestinyPresentationNodeDefinition，但要点是这些是游戏 UI 用于将收藏品和记录分类到层次结构中的实体。如果你想在自己的 UI 中执行类似的分类，可以请求并使用此数据：或者你可以跳过它并自己滚动。
    /// </summary>
    [Description("展示节点")]
    PresentationNodes = 700,

    /// <summary>
    /// 返回有关所有“收藏品”的摘要状态信息。这些是你在玩 Destiny 时发现的物品的记录，以及一些其他基本信息。有关详细信息，你将不得不调用专门用于此目的的单独端点。
    /// </summary>
    [Description("收藏品")]
    Collectibles = 800,

    /// <summary>
    /// 返回有关所有“记录”（游戏中也称为“胜利”）的摘要状态信息。我知道，这很混乱，因为还有“胜利时刻”，它们本身将表示为“胜利”。
    /// </summary>
    [Description("记录")]
    Records = 900,

    /// <summary>
    /// 返回 Bungie 认为是“临时”的信息：数据可能变化太频繁或来自非权威来源，以至于我们不认为数据是完全可信的，但在某些有限的用例中可能有用。我们不能保证此数据的及时性或一致性：买家自负风险。
    /// </summary>
    [Description("临时")]
    Transitory = 1000,

    /// <summary>
    /// 返回有关所有“指标”（游戏中也称为“统计追踪器”）的摘要状态信息。
    /// </summary>
    [Description("指标")]
    Metrics = 1100,

    /// <summary>
    /// 返回每个帐户或每个角色的本地化字符串变量哈希到值的映射。
    /// </summary>
    [Description("字符串变量")]
    StringVariables = 1200,

    /// <summary>
    /// 返回有关所有“可制作物品”即制作配方物品的摘要状态信息。
    /// </summary>
    [Description("可制作物品")]
    Craftables = 1300,

    /// <summary>
    /// 返回所有表扬和表扬节点的分数值。
    /// </summary>
    [Description("社交表扬")]
    SocialCommendations = 1400
}