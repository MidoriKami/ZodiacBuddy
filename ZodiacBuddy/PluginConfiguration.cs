using Dalamud.Configuration;
using Dalamud.Game.Text;
using Newtonsoft.Json;
using ZodiacBuddy.BonusLight;
using ZodiacBuddy.InformationWindow;
using ZodiacBuddy.Stages.Brave;
using ZodiacBuddy.Stages.Novus;

namespace ZodiacBuddy;

public class PluginConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 1;

    [JsonProperty("BraveEchoChannel")] public XivChatType ChatType { get; set; } = XivChatType.Echo;

    public bool BraveEchoTarget { get; set; } = true;

    public BonusLightConfiguration BonusLight { get; } = new();

    public NovusConfiguration Novus { get; } = new();

    public BraveConfiguration Brave { get; } = new();

    public InformationWindowConfiguration InformationWindow { get; } = new();

    public void Save() => Service.Interface.SavePluginConfig(this);
}