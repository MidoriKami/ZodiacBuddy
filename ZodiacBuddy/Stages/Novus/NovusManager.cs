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
/// Your buddy for the Novus stage.
/// </summary>
internal class NovusManager : IDisposable
{
    [Signature("40 56 48 83 EC 50 F3 0F 10 05", DetourName = nameof(AddonRelicGlassOnSetupDetour))]
    private readonly Hook<AddonRelicGlassOnSetupDelegate> addonRelicGlassOnSetupHook = null!;

    [Signature("E8 ?? ?? ?? ?? 4D 39 BE")]
    private readonly AlertFuncDelegate playSound = null!;

    private readonly NovusWindow novusWindow;
    private readonly Timer timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="NovusManager"/> class.
    /// </summary>
    public NovusManager()
    {
        this.novusWindow = new NovusWindow();

        Service.Framework.Update += this.OnUpdate;
        Service.Toasts.QuestToast += this.OnToast;
        Service.Interface.UiBuilder.Draw += this.novusWindow.Draw;

        this.timer = new Timer(_ => this.ResetBonus(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

        SignatureHelper.Initialise(this);
        this.addonRelicGlassOnSetupHook?.Enable();
    }

    private delegate void AddonRelicGlassOnSetupDelegate(IntPtr addon, uint a2, IntPtr relicInfoPtr);

    private delegate ulong AlertFuncDelegate(byte id, ulong unk1, ulong unk2);

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

    /// <inheritdoc/>
    public void Dispose()
    {
        this.timer?.Dispose();

        Service.Interface.UiBuilder.Draw -= this.novusWindow.Draw;
        Service.Toasts.QuestToast -= this.OnToast;

        this.addonRelicGlassOnSetupHook?.Disable();
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

    private void OnUpdate(Dalamud.Game.Framework framework)
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
                    this.UpdateLightBonus(null, null, $"\"{territoryLight.DutyName}\" no longer has the bonus of light.");
                }
                else
                {
                    // Update dateTime
                    this.UpdateLightBonus(territoryRowId, DateTime.UtcNow, null);
                }
            }
            else if (lightLevel.Intensity > territoryLight!.DefaultLightIntensity)
            {
                // New detection
                this.UpdateLightBonus(territoryRowId, DateTime.UtcNow, $"Light bonus detected on \"{territoryLight.DutyName}\"");
            }
        }
    }

    private void UpdateLightBonus(uint? territoryId, DateTime? date, string? message)
    {
        Configuration.LightBonusTerritoryId = territoryId;
        Configuration.LightBonusDetection = date;
        Service.Configuration.Save();

        if (message == null)
            return;

        Service.Plugin.PrintMessage(message);

        if (!Configuration.PlaySoundOnLightBonusNotification)
            return;

        this.playSound(0x2D, 0u, 0u);
    }

    private void ResetBonus()
    {
        // Reset bonus after 2h
        var dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(2));
        if (Configuration.LightBonusDetection != null &&
            Configuration.LightBonusDetection.Value < dt)
        {
            this.UpdateLightBonus(null, null, null);
        }
    }
}