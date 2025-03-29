using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ZodiacBuddy.Stages.Brave;

/// <summary>
/// Your buddy for the Zodiac Brave stage.
/// </summary>
internal class BraveManager : IDisposable {
    // private static readonly BonusLightLevel[] BonusLightValues = {
    //     #pragma warning disable format,SA1008,SA1025
    //     new(  4, 4660), // Feeble
    //     new(  8, 4661), // Faint
    //     new( 16, 4662), // Gentle
    //     new( 24, 4663), // Steady
    //     new( 48, 4664), // Forceful
    //     new( 64, 4665), // Nigh Sings
    //     #pragma warning restore format,SA1008,SA1025
    // };

    private readonly BraveWindow window;

    /// <summary>
    /// Initializes a new instance of the <see cref="BraveManager"/> class.
    /// </summary>
    public BraveManager() {
        this.window = new BraveWindow();
        
        Service.Framework.Update += this.OnUpdate;
        Service.Interface.UiBuilder.Draw += this.window.Draw;
        
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "RelicMagicite", AddonRelicMagiciteOnSetupDetour);
    }

    private static BraveConfiguration Configuration => Service.Configuration.Brave;

    /// <inheritdoc/>
    public void Dispose() {
        Service.Framework.Update -= this.OnUpdate;
        Service.Interface.UiBuilder.Draw -= this.window.Draw;
        
        Service.AddonLifecycle.UnregisterListener(AddonRelicMagiciteOnSetupDetour);
    }

    private void AddonRelicMagiciteOnSetupDetour(AddonEvent type, AddonArgs args)
        => this.UpdateRelicMagiciteAddon(0);

    private unsafe void UpdateRelicMagiciteAddon(int slot) {
        var item = Util.GetEquippedItem(slot);
        if (!BraveRelic.Items.ContainsKey(item.ItemId))
            return;

        var addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("RelicMagicite");
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

        if (Configuration.ShowNumbersInRelicMagicite) {
            var value = item.SpiritbondOrCollectability % 500;
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

    private void OnUpdate(IFramework framework) {
        try {
            if (!Configuration.DisplayRelicInfo) {
                this.window.ShowWindow = false;
                return;
            }

            var mainhand = Util.GetEquippedItem(0);
            var offhand = Util.GetEquippedItem(1);

            var shouldShowWindow =
                BraveRelic.Items.ContainsKey(mainhand.ItemId) ||
                BraveRelic.Items.ContainsKey(offhand.ItemId);

            this.window.ShowWindow = shouldShowWindow;
            this.window.MainHandItem = mainhand;
            this.window.OffhandItem = offhand;
        }
        catch (Exception ex) {
            Service.PluginLog.Error(ex, $"Unhandled error during {nameof(BraveManager)}.{nameof(this.OnUpdate)}");
        }
    }
}
