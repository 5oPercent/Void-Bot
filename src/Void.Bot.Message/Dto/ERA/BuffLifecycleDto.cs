using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class BuffLifecycleDto
{
    public bool CountEvents { get; set; }
    public string TriggerEventId { get; set; } = "hit";
    public int EventsPerStack { get; set; } = 1;
    public int StacksPerThreshold { get; set; } = 1;
    public int DecayMode { get; set; }
    public double DecayDelaySeconds { get; set; } = 3;
    public double DecayIntervalSeconds { get; set; } = 1;
    public int StacksPerDecay { get; set; } = 1;
    public bool ResetDecayOnTrigger { get; set; } = true;
}
