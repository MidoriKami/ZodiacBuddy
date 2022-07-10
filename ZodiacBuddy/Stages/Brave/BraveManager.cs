using System;

using Dalamud.Game;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ZodiacBuddy.BonusLight;

namespace ZodiacBuddy.Stages.Brave;

/// <summary>
/// Your buddy for the Zodiac Brave stage.
/// </summary>
internal class BraveManager : IDisposable
{
    private static readonly BonusLightLevel[] BonusLightValues =
    {
        #pragma warning disable format,SA1008,SA1025
        new(  4, 4660), // Feeble
        new(  8, 4661), // Faint
        new( 16, 4662), // Gentle
        new( 24, 4663), // Steady
        new( 48, 4664), // Forceful
        new( 64, 4665), // Nigh Sings
        #pragma warning restore format,SA1008,SA1025
    };

    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 F3 0F 10 05 ?? ?? ?? ?? 49 8B D8", DetourName = nameof(AddonRelicMagiciteOnSetupDetour))]
    private readonly Hook<AddonRelicGlassOnSetupDelegate> addonRelicMagiciteOnSetupHook = null!;
    private readonly BraveWindow window;

    /// <summary>
    /// Initializes a new instance of the <see cref="BraveManager"/> class.
    /// </summary>
    public BraveManager()
    {
        this.window = new BraveWindow();

        Service.Framework.Update += this.OnUpdate;
        Service.Interface.UiBuilder.Draw += this.window.Draw;

        SignatureHelper.Initialise(this);
        this.addonRelicMagiciteOnSetupHook?.Enable();
    }

    private delegate void AddonRelicGlassOnSetupDelegate(IntPtr addon, uint a2, IntPtr relicInfoPtr);

    private static BraveConfiguration Configuration => Service.Configuration.Brave;

    private static BonusLightConfiguration LightConfiguration => Service.Configuration.BonusLight;

    /// <inheritdoc/>
    public void Dispose()
    {
        Service.Interface.UiBuilder.Draw -= this.window.Draw;

        this.addonRelicMagiciteOnSetupHook?.Disable();
    }

    private void AddonRelicMagiciteOnSetupDetour(IntPtr addonRelicMagicite, uint a2, IntPtr relicInfoPtr)
    {
        this.addonRelicMagiciteOnSetupHook.Original(addonRelicMagicite, a2, relicInfoPtr);

        try
        {
            this.UpdateRelicMagiciteAddon(0);
            // this.UpdateRelicMagiciteAddon(1, ???); // TODO component ID
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, $"Unhandled error during {nameof(BraveManager)}.{nameof(this.AddonRelicMagiciteOnSetupDetour)}");
        }
    }

    private unsafe void UpdateRelicMagiciteAddon(int slot)
    {
        var item = Util.GetEquippedItem(slot);
        if (!BraveRelic.Items.ContainsKey(item.ItemID))
            return;

        var addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("RelicMagicite", 1);
        if (addon == null)
            return;

        // var componentNode = (AtkComponentNode*)addon->UldManager.SearchNodeById(nodeID);
        // if (componentNode == null)
        //     return;

        // var lightText = (AtkTextNode*)componentNode->Component->UldManager.SearchNodeById(9);
        // if (lightText == null)
        //     return;

        var lightText = (AtkTextNode*)addon->UldManager.SearchNodeById(9);
        if (lightText == null)
            return;

        if (Configuration.ShowNumbersInRelicMagicite)
        {
            var value = item.Spiritbond % 500;
            lightText->SetText($"{lightText->NodeText}\n{value / 2}/40");
        }

        if (!Configuration.DontPlayRelicMagiciteAnimation)
            return;

        // var analyzeText = (AtkTextNode*)componentNode->Component->UldManager.SearchNodeById(8);
        // if (analyzeText == null)
        //     return;

        var analyzeText = (AtkTextNode*)addon->UldManager.SearchNodeById(8);
        if (analyzeText == null)
            return;

        analyzeText->SetText(lightText->NodeText.ToString());
    }

    private void OnUpdate(Framework framework)
    {
        try
        {
            this.OnUpdateInner();
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, $"Unhandled error during {nameof(BraveManager)}.{nameof(this.OnUpdate)}");
        }
    }

    private void OnUpdateInner()
    {
        if (!Configuration.DisplayRelicInfo)
        {
            this.window.ShowWindow = false;
            return;
        }

        var mainhand = Util.GetEquippedItem(0);
        var offhand = Util.GetEquippedItem(1);

        var shouldShowWindow =
            BraveRelic.Items.ContainsKey(mainhand.ItemID) ||
            BraveRelic.Items.ContainsKey(offhand.ItemID);

        this.window.ShowWindow = shouldShowWindow;
        this.window.MainhandItem = mainhand;
        this.window.OffhandItem = offhand;
    }
}
