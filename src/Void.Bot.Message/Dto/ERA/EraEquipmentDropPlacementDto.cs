namespace Void.Bot.Message.Dto.ERA;

/// <summary>由房主在触发时捕获的世界坐标和 Y 轴朝向（弧度）；后端仅保存，不校验距离。</summary>
public sealed class EraEquipmentDropPlacementDto
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double YawRadians { get; set; }
}
