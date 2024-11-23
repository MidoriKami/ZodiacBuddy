using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using ZodiacBuddy.BonusLight;

namespace ZodiacBuddy;

/// <summary>
/// Static class used to store debug and check functions for dev.
/// </summary>
public static class DebugTools {
    /// <summary>
    /// Check that all the territory id have a name in Lumina.
    /// <p/>
    /// If a territory doesn't have a name, it's id have probably changed and a message is display in chat.
    /// </summary>
    public static void CheckBonusLightDutyTerritories() {
        foreach (var bonusLightDuty in BonusLightDuty.GetDataset()) {
            if (string.IsNullOrWhiteSpace(bonusLightDuty.Value.DutyName)) {
                    
                var sb = new SeStringBuilder()
	                .AddUiForeground("[ZodiacBuddy] ", 45)
                    .Append($"Invalid territory id {bonusLightDuty.Key}");
                    
                Service.ChatGui.Print(new XivChatEntry {
                    Type = XivChatType.Echo,
                    Message = sb.BuiltString,
                });
            }
        }
    }
}