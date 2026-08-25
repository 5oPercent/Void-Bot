using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Component;

public class Destiny2ComponentMetricsDto
{
    public Destiny2MetricsDataDto Data { get; set; } = new();

    public Destiny2ComponentPrivacyEnum Privacy { get; set; }
}

public class Destiny2MetricsDataDto
{
    public Dictionary<uint, Destiny2MetricComponentDto> Metrics { get; set; } = [];

    public uint MetricsRootNodeHash { get; set; }
}

public class Destiny2MetricComponentDto
{
    public bool Invisible { get; set; }

    public Destiny2ObjectiveProgressDto ObjectiveProgress { get; set; } = new();
}

public class Destiny2ObjectiveProgressDto
{
    public uint ObjectiveHash { get; set; }

    public int? Progress { get; set; }

    public int CompletionValue { get; set; }

    public bool Complete { get; set; }

    public bool Visible { get; set; }
}
