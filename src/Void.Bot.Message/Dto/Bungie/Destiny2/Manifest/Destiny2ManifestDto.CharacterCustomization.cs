namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 角色自定义定义
/// </summary>
public class Destiny2ManifestCharacterCustomizationDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 性别哈希
    /// </summary>
    public uint GenderHash { get; set; }

    /// <summary>
    /// 种族哈希
    /// </summary>
    public uint RaceHash { get; set; }

    /// <summary>
    /// 贴花颜色选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto DecalColorOptions { get; set; } = new();
    
    /// <summary>
    /// 贴花选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto DecalOptions { get; set; } = new();
    
    /// <summary>
    /// 眼睛颜色选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto EyeColorOptions { get; set; } = new();

    /// <summary>
    /// 脸部选项电影主机模式ID列表
    /// </summary>
    public List<uint> FaceOptionCinematicHostPatternIds { get; set; } = [];
    
    /// <summary>
    /// 脸部选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto FaceOptions { get; set; } = new();
    
    /// <summary>
    /// 特征颜色选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto FeatureColorOptions { get; set; } = new();
    
    /// <summary>
    /// 特征选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto FeatureOptions { get; set; } = new();
    
    /// <summary>
    /// 头发颜色选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto HairColorOptions { get; set; } = new();
    
    /// <summary>
    /// 头发选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto HairOptions { get; set; } = new();
    
    /// <summary>
    /// 头盔偏好
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto HelmetPreferences { get; set; } = new();
    
    /// <summary>
    /// 唇色选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto LipColorOptions { get; set; } = new();
    
    /// <summary>
    /// 个性选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto PersonalityOptions { get; set; } = new();
    
    /// <summary>
    /// 肤色选项
    /// </summary>
    public Destiny2ManifestOptionDefinitionDto SkinColorOptions { get; set; } = new();
}

/// <summary>
/// 角色自定义清单
/// </summary>
public class Destiny2ManifestCharacterCustomizationDto : Dictionary<string, Destiny2ManifestCharacterCustomizationDefinitionDto>
{
}