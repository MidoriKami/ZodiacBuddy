using System;
using System.Threading;

using Dalamud.Game;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ZodiacBuddy.Novus.Data;
using ZodiacBuddy.Stages.Novus;

namespace ZodiacBuddy.Novus;

/// <summary>
/// Your buddy for the Novus stage.
/// </summary>
internal class NovusManager : IDisposable
{
    [Signature("40 56 48 83 EC 50 F3 0F 10 05", DetourName = nameof(AddonRelicGlassOnSetupDetour))]
    private readonly Hook<AddonRelicGlassOnSetupDelegate> addonRelicGlassOnSetupHook = null!;

    private readonly NovusWindow novusWindow;
    private readonly Timer resetTimer;
    private readonly Timer checkTimer;
    private readonly NovusHttpClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="NovusManager"/> class.
    /// </summary>
    public NovusManager()
    {
        this.novusWindow = new NovusWindow();

        Service.Framework.Update += this.OnUpdate;
        Service.Toasts.QuestToast += this.OnToast;
        Service.Interface.UiBuilder.Draw += this.novusWindow.Draw;

        this.client = new NovusHttpClient();

        var timeOfDay = DateTime.UtcNow.TimeOfDay;
        var nextEvenHour = timeOfDay.Hours % 2 == 0 ?
            TimeSpan.FromHours(timeOfDay.Hours + 2) :
            TimeSpan.FromHours(timeOfDay.Hours + 1);
        var delta = nextEvenHour - timeOfDay;

        this.resetTimer = new Timer(_ => this.ResetBonus(), null, delta, TimeSpan.FromHours(2));
        this.checkTimer = new Timer(_ => this.CheckBonus(), null, TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(5));

        SignatureHelper.Initialise(this);
        this.addonRelicGlassOnSetupHook?.Enable();
    }

    private delegate void AddonRelicGlassOnSetupDelegate(IntPtr addon, uint a2, IntPtr relicInfoPtr);

    private static NovusConfiguration Configuration => Service.Configuration.NovusConfiguration;

    /// <summary>
    /// Return the item equipped on the slot id.
    /// </summary>
    /// <param name="index">Slot index of the desired item.</param>
    /// <returns>Equipped item on the slot or the default item 0.</returns>
    public static unsafe InventoryItem GetEquippedItem(int index)
    {
        var im = InventoryManager.Instance();
        if (im == null)
            throw new Exception("InventoryManager was null");

        var equipped = im->GetInventoryContainer(InventoryType.EquippedItems);
        if (equipped == null)
            throw new Exception("EquippedItems was null");

        var slot = equipped->GetInventorySlot(index);
        if (slot == null)
            throw new Exception($"InventorySlot{index} was null");

        return *slot;
    }

    /// <summary>
    /// Update the bonus of light in configuration and notify the player.
    /// </summary>
    /// <param name="territoryId">Territory id of the bonus.</param>
    /// <param name="date">DateTime of detection.</param>
    /// <param name="message">Message to notify the player. Can be null to not send notification.</param>
    public static void UpdateLightBonus(uint? territoryId, DateTime? date, string? message)
    {
        Configuration.LightBonusTerritoryId = territoryId;
        Configuration.LightBonusDetection = date;
        Service.Configuration.Save();

        if (message == null)
            return;

        if (Configuration.NotifyLightBonusOnlyWhenEquipped && !HasNovusEquipped())
            return;

        Service.Plugin.PrintMessage(message);

        if (!Configuration.PlaySoundOnLightBonusNotification)
            return;

        Service.Sound.PlaySound(Sound.Sounds.Sound09);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.resetTimer?.Dispose();
        this.checkTimer?.Dispose();

        Service.Interface.UiBuilder.Draw -= this.novusWindow.Draw;
        Service.Toasts.QuestToast -= this.OnToast;

        this.addonRelicGlassOnSetupHook?.Disable();
    }

    private static bool HasNovusEquipped()
    {
        var mainhand = GetEquippedItem(0);
        var offhand = GetEquippedItem(1);

        return NovusRelic.Novus.ContainsKey(mainhand.ItemID) ||
               NovusRelic.Novus.ContainsKey(offhand.ItemID);
    }

    private void AddonRelicGlassOnSetupDetour(IntPtr addonRelicGlass, uint a2, IntPtr relicInfoPtr)
    {
        this.addonRelicGlassOnSetupHook.Original(addonRelicGlass, a2, relicInfoPtr);

        try
        {
            this.UpdateRelicGlassAddon(0, 4u);
            this.UpdateRelicGlassAddon(1, 5u);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Exception during hook: AddonRelicGlassOnSetup");
        }
    }

    private unsafe void UpdateRelicGlassAddon(int slot, uint nodeID)
    {
        var item = GetEquippedItem(slot);
        if (!NovusRelic.Novus.ContainsKey(item.ItemID))
            return;

        var addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("RelicGlass", 1);
        if (addon == null)
            return;

        var componentNode = (AtkComponentNode*)addon->GetNodeById(nodeID);
        if (componentNode == null)
            return;

        var lightText = (AtkTextNode*)componentNode->Component->UldManager.SearchNodeById(8);
        if (lightText == null)
            return;

        if (Configuration.ShowNumbersInRelicGlass)
            lightText->SetText($"{lightText->NodeText} {item.Spiritbond}/2000");

        if (!Configuration.DontPlayRelicGlassAnimation)
            return;

        var analyseText = (AtkTextNode*)componentNode->Component->UldManager.SearchNodeById(7);
        if (analyseText == null)
            return;

        analyseText->SetText(lightText->NodeText.ToString());
    }

    private void OnUpdate(Framework framework)
    {
        if (!Configuration.DisplayNovusInfo)
        {
            this.novusWindow.ShowWindow = false;
            return;
        }

        var mainhand = GetEquippedItem(0);
        var offhand = GetEquippedItem(1);

        var shouldShowWindow =
            NovusRelic.Novus.ContainsKey(mainhand.ItemID) ||
            NovusRelic.Novus.ContainsKey(offhand.ItemID);

        this.novusWindow.ShowWindow = shouldShowWindow;
        this.novusWindow.MainhandItem = mainhand;
        this.novusWindow.OffhandItem = offhand;
    }

    private void OnToast(ref SeString message, ref QuestToastOptions options, ref bool isHandled)
    {
        if (isHandled)
            return;

        // Avoid double display if SnS equipped
        if (NovusRelic.Novus.ContainsKey(GetEquippedItem(0).ItemID) &&
            NovusRelic.Novus.TryGetValue(GetEquippedItem(1).ItemID, out var relicName) &&
            message.ToString().Contains(relicName))
            return;

        foreach (var lightLevel in LightLevel.Values)
        {
            if (!message.ToString().Contains(lightLevel.Message))
                continue;

            Service.Plugin.PrintMessage($"Light Intensity has increased by {lightLevel.Intensity}.");

            var territoryRowId = Service.ClientState.TerritoryType;
            if (!NovusDuty.TryGetValue(territoryRowId, out var territoryLight))
                return;

            if (Configuration.LightBonusTerritoryId == territoryRowId)
            {
                if (lightLevel.Intensity <= territoryLight!.DefaultLightIntensity)
                {
                    // No longer light bonus
                    UpdateLightBonus(null, null, $"\"{territoryLight.DutyName}\" no longer has the bonus of light.");
                }
                else
                {
                    // Update dateTime
                    UpdateLightBonus(territoryRowId, DateTime.UtcNow, null);
                }
            }
            else if (lightLevel.Intensity > territoryLight!.DefaultLightIntensity)
            {
                // New detection
                UpdateLightBonus(territoryRowId, DateTime.UtcNow, $"Light bonus detected on \"{territoryLight.DutyName}\"");
                this.client.SendReport(territoryRowId);
            }
        }
    }

    private void CheckBonus()
    {
        if (Configuration.LightBonusDetection == null)
        {
            this.client.RetrieveLastReport();
        }
    }

    private void ResetBonus()
    {
        UpdateLightBonus(null, null, null);
    }
}