using System;
using System.Threading;

using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ZodiacBuddy.Novus.Data;

namespace ZodiacBuddy.Novus;

/// <summary>
/// Main implementation for Novus relic.
/// </summary>
public class NovusManager : IDisposable
{
    [Signature(
        "40 56 48 83 ec 50 f3 0f 10 05 ?? ?? ?? 00 45 33 c9 48 89 6c 24 68 0f 57 c9 49 8b e8 4c 89 74 24 78 45 33 c0",
        DetourName = nameof(AddonRelicGlassOnSetupDetour))]
    private readonly Hook<AddonRelicGlassOnSetupDelegate> addonRelicGlassOnSetupHook = null!;

    [Signature("E8 ?? ?? ?? ?? 4D 39 BE")]
    private readonly AlertFuncDelegate playSound = null!;

    private readonly NovusConfiguration novusConfiguration;
    private readonly NovusWindow novusWindow;
    private readonly Timer timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="NovusManager"/> class.
    /// </summary>
    public NovusManager()
    {
        this.novusConfiguration = Service.Configuration.NovusConfiguration;
        this.novusWindow = new NovusWindow(this.novusConfiguration);

        Service.Interface.UiBuilder.Draw += this.novusWindow.Draw;
        Service.Toasts.QuestToast += this.OnToast;

        this.timer = new Timer(_ => this.ResetBonus(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

        SignatureHelper.Initialise(this);
        this.addonRelicGlassOnSetupHook?.Enable();
    }

    private delegate void AddonRelicGlassOnSetupDelegate(long addon, ulong p2, long relicInfoPtr);

    private delegate ulong AlertFuncDelegate(byte id, ulong unk1, ulong unk2);

    /// <summary>
    /// Return the item equipped on the slot id.
    /// </summary>
    /// <param name="slot">slot id of the desired item.</param>
    /// <returns>Equipped item on the slot or the default item 0.</returns>
    public static unsafe InventoryItem GetEquippedItem(int slot)
    {
        return *InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(slot);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.timer.Dispose();
        Service.Interface.UiBuilder.Draw -= this.novusWindow.Draw;
        Service.Toasts.QuestToast -= this.OnToast;
        this.addonRelicGlassOnSetupHook?.Disable();
    }

    private static uint GetEquippedItemId(int slot)
    {
        return GetEquippedItem(slot).ItemID;
    }

    private void AddonRelicGlassOnSetupDetour(long addonRelicGlass, ulong p2, long relicInfoPtr)
    {
        this.addonRelicGlassOnSetupHook.Original(addonRelicGlass, p2, relicInfoPtr);
        try
        {
            this.UpdateRelicGlassAddon(true);
            this.UpdateRelicGlassAddon(false);
        }
        catch (Exception e)
        {
            PluginLog.Error(e, "AddonRelicGlassOnSetupDetour Error");
        }
    }

    private unsafe void UpdateRelicGlassAddon(bool isFirstRelic)
    {
        var item = GetEquippedItem(isFirstRelic ? 0 : 1);
        if (!NovusRelic.Novus.ContainsKey(item.ItemID)) return;

        var componentNodeId = isFirstRelic ? 4u : 5u;
        var addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("RelicGlass", 1);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (addon == null) return;

        var componentNode = (AtkComponentNode*)addon->GetNodeById(componentNodeId);
        if (componentNode == null) return;

        var lightText = (AtkTextNode*)componentNode->Component->UldManager.SearchNodeById(8);
        lightText->SetText($"{lightText->NodeText.ToString()} {item.Spiritbond}/2000");

        if (!this.novusConfiguration.DontPlayRelicGlassAnimation) return;
        var analyseText = (AtkTextNode*)componentNode->Component->UldManager.SearchNodeById(7);
        analyseText->SetText(lightText->NodeText.ToString());
    }

    private void OnToast(ref SeString message, ref QuestToastOptions options, ref bool isHandled)
    {
        if (isHandled) return;

        // Avoid double display if SnS equipped
        if (NovusRelic.Novus.ContainsKey(GetEquippedItemId(0)) &&
            NovusRelic.Novus.TryGetValue(GetEquippedItemId(1), out var relicName) &&
            message.ToString().Contains(relicName)) return;

        foreach (var lightLevel in LightLevel.Values)
        {
            if (!message.ToString().Contains(lightLevel.Message)) continue;

            this.PrintChat($"Light Intensity has increased by {lightLevel.Intensity}.");

            var territoryRowId = Service.ClientState.TerritoryType;
            if (!NovusDuty.Dictionary.TryGetValue(territoryRowId, out var territoryLight)) return;

            if (this.novusConfiguration.LightBonusTerritoryId == territoryRowId)
            {
                if (lightLevel.Intensity <= territoryLight.DefaultLightIntensity)
                {
                    // No longer light bonus
                    this.UpdateLightBonus(null, null, $"\"{territoryLight.DutyName}\" has no longer the bonus of light.");
                }
                else
                {
                    // Update dateTime
                    this.UpdateLightBonus(territoryRowId, DateTime.UtcNow, null);
                }
            }
            else if (lightLevel.Intensity > territoryLight.DefaultLightIntensity)
            {
                // New detection
                this.UpdateLightBonus(territoryRowId, DateTime.UtcNow, $"Light bonus detected on \"{territoryLight.DutyName}\"");
            }
        }
    }

    private void ResetBonus()
    {
        // Reset bonus after 2h
        var dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(2));
        if (this.novusConfiguration.LightBonusDetection != null &&
            this.novusConfiguration.LightBonusDetection.Value < dt)
        {
            this.UpdateLightBonus(null, null, null);
        }
    }

    private void UpdateLightBonus(uint? territoryId, DateTime? date, string? message)
    {
        this.novusConfiguration.LightBonusTerritoryId = territoryId;
        this.novusConfiguration.LightBonusDetection = date;
        Service.Configuration.Save();

        if (message == null) return;
        this.PrintChat(message);

        if (!this.novusConfiguration.PlaySoundOnLightBonusNotification) return;
        this.playSound(0x2D, 0u, 0u);
    }

    private void PrintChat(string message)
    {
        Service.ChatGui.PrintChat(new XivChatEntry()
        {
            Type = Service.Configuration.BraveEchoChannel,
            Message = message,
        });
    }
}