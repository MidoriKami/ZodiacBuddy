using System;
using System.Linq;
using System.Runtime.InteropServices;

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ZodiacBuddy.Atma.Data;

using Sheets = Lumina.Excel.GeneratedSheets;

namespace ZodiacBuddy.Atma;

/// <summary>
/// Your buddy for the Atma enhancement stage.
/// </summary>
internal partial class AtmaManager : IDisposable
{
    [Signature("48 89 74 24 ?? 57 48 83 EC 20 8D 42 FD 49 8B F0 48 8B F9 83 F8 09 77 2D", DetourName = nameof(ReceiveEventDetour))]
    private readonly Hook<ReceiveEventDelegate> receiveEventHook = null!; // AddonRelicNotebook.ReceiveEvent:Click

    [Signature("48 89 6C 24 ?? 48 89 74 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B F9 41 0F B6 E8")]
    private readonly OpenDutyDelegate openDuty = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="AtmaManager"/> class.
    /// </summary>
    public AtmaManager()
    {
        SignatureHelper.Initialise(this);
        this.receiveEventHook.Enable();
    }

    private delegate void ReceiveEventDelegate(IntPtr addon, uint which, IntPtr eventData, IntPtr inputData);

    private delegate IntPtr OpenDutyDelegate(IntPtr agent, uint contentFinderCondition, byte a3);

    /// <inheritdoc/>
    public void Dispose()
    {
        this.receiveEventHook?.Dispose();
    }

    private static uint GetNearestAetheryte(MapLinkPayload mapLink)
    {
        var closestAetheryteID = 0u;
        var closestDistance = double.MaxValue;

        static float ConvertRawPositionToMapCoordinate(int pos, float scale)
        {
            var c = scale / 100.0f;
            var scaledPos = pos * c / 1000.0f;

            return (41.0f / c * ((scaledPos + 1024.0f) / 2048.0f)) + 1.0f;
        }

        var aetherytes = Service.DataManager.GetExcelSheet<Sheets.Aetheryte>()!;
        var mapMarkers = Service.DataManager.GetExcelSheet<Sheets.MapMarker>()!;

        foreach (var aetheryte in aetherytes)
        {
            if (!aetheryte.IsAetheryte)
                continue;

            if (aetheryte.Territory.Value?.RowId != mapLink.TerritoryType.RowId)
                continue;

            var map = aetheryte.Map.Value!;
            var scale = map.SizeFactor;
            var name = map.PlaceName.Value!.Name.ToString();

            var mapMarker = mapMarkers.FirstOrDefault(m => m.DataType == 3 && m.DataKey == aetheryte.RowId);
            if (mapMarker == default)
            {
                // PluginLog.Debug($"Could not find aetheryte: {name}");
                return 0;
            }

            var aetherX = ConvertRawPositionToMapCoordinate(mapMarker.X, scale);
            var aetherY = ConvertRawPositionToMapCoordinate(mapMarker.Y, scale);

            // var aetheryteName = aetheryte.PlaceName.Value!;
            // PluginLog.Debug($"Aetheryte found: {aetherName} ({aetherX} ,{aetherY})");
            var distance = Math.Pow(aetherX - mapLink.XCoord, 2) + Math.Pow(aetherY - mapLink.YCoord, 2);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestAetheryteID = aetheryte.RowId;
            }
        }

        return closestAetheryteID;
    }

    private unsafe bool Teleport(uint aetheryteID)
    {
        if (Service.ClientState.LocalPlayer == null)
            return false;

        var telepo = Telepo.Instance();
        if (telepo == null)
        {
            Service.Plugin.PrintError("Something horrible happened, please contact the developer.");
            PluginLog.Error("Could not teleport: Telepo is missing.");
            return false;
        }

        if (telepo->TeleportList.Size() == 0)
            telepo->UpdateAetheryteList();

        foreach (var aetheryte in telepo->TeleportList.Span)
        {
            if (aetheryte.AetheryteId == aetheryteID)
                return telepo->Teleport(aetheryteID, 0);
        }

        Service.Plugin.PrintError("Could not teleport, not attuned.");
        return false;
    }

    private unsafe bool ShowDutyFinder(uint cfcID)
    {
        if (cfcID == 0)
            return false;

        var framework = Framework.Instance();
        var uiModule = framework->GetUiModule();
        var agentModule = uiModule->GetAgentModule();
        var cfAgent = (IntPtr)agentModule->GetAgentByInternalId(AgentId.ContentsFinder);

        this.openDuty(cfAgent, cfcID, 0);
        return true;
    }

    private unsafe void ReceiveEventDetour(IntPtr addon, uint which, IntPtr eventData, IntPtr inputData)
    {
        try
        {
            this.ReceiveEvent(addon, eventData);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Exception during hook: AddonRelicNotebook.ReceiveEvent:Click");
        }
    }

    private unsafe void ReceiveEvent(IntPtr addon, IntPtr eventData)
    {
        var relicNote = RelicNote.Instance();
        if (relicNote == null)
            return;

        var bookID = relicNote->RelicNoteID;

        var addonPtr = (AddonRelicNoteBook*)addon;
        var index = addonPtr->CategoryList->SelectedItemIndex;

        var eventDataPtr = (EventData*)eventData;
        var targetComponent = eventDataPtr->Target;

        unsafe static bool IsOwnerNode(IntPtr target, AtkComponentCheckBox* checkbox)
            => target == new IntPtr(checkbox->AtkComponentButton.AtkComponentBase.OwnerNode);

        var selectedTarget = targetComponent switch
        {
            // Enemies
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy0.CheckBox) => BraveBook.GetValue(bookID).Enemies[0],
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy1.CheckBox) => BraveBook.GetValue(bookID).Enemies[1],
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy2.CheckBox) => BraveBook.GetValue(bookID).Enemies[2],
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy3.CheckBox) => BraveBook.GetValue(bookID).Enemies[3],
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy4.CheckBox) => BraveBook.GetValue(bookID).Enemies[4],
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy5.CheckBox) => BraveBook.GetValue(bookID).Enemies[5],
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy6.CheckBox) => BraveBook.GetValue(bookID).Enemies[6],
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy7.CheckBox) => BraveBook.GetValue(bookID).Enemies[7],
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy8.CheckBox) => BraveBook.GetValue(bookID).Enemies[8],
            IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy9.CheckBox) => BraveBook.GetValue(bookID).Enemies[9],
            // Dungeons
            IntPtr t when index == 1 && IsOwnerNode(t, addonPtr->Dungeon0.CheckBox) => BraveBook.GetValue(bookID).Dungeons[0],
            IntPtr t when index == 1 && IsOwnerNode(t, addonPtr->Dungeon1.CheckBox) => BraveBook.GetValue(bookID).Dungeons[1],
            IntPtr t when index == 1 && IsOwnerNode(t, addonPtr->Dungeon2.CheckBox) => BraveBook.GetValue(bookID).Dungeons[2],
            // FATEs
            IntPtr t when index == 2 && IsOwnerNode(t, addonPtr->Fate0.CheckBox) => BraveBook.GetValue(bookID).Fates[0],
            IntPtr t when index == 2 && IsOwnerNode(t, addonPtr->Fate1.CheckBox) => BraveBook.GetValue(bookID).Fates[1],
            IntPtr t when index == 2 && IsOwnerNode(t, addonPtr->Fate2.CheckBox) => BraveBook.GetValue(bookID).Fates[2],
            // Leves
            IntPtr t when index == 3 && IsOwnerNode(t, addonPtr->Leve0.CheckBox) => BraveBook.GetValue(bookID).Leves[0],
            IntPtr t when index == 3 && IsOwnerNode(t, addonPtr->Leve1.CheckBox) => BraveBook.GetValue(bookID).Leves[1],
            IntPtr t when index == 3 && IsOwnerNode(t, addonPtr->Leve2.CheckBox) => BraveBook.GetValue(bookID).Leves[2],
            _ => throw new ArgumentException($"Unexpected index and/or node: {index}, {targetComponent:X}"),
        };

        var zoneName = !string.IsNullOrEmpty(selectedTarget.LocationName)
            ? $"{selectedTarget.LocationName}, {selectedTarget.ZoneName}"
            : selectedTarget.ZoneName;

        // PluginLog.Debug($"Target selected: {selectedTarget.Name} in {zoneName}.");
        if (Service.Configuration.BraveEchoTarget)
        {
            var sb = new SeStringBuilder()
                .AddText("Target selected: ")
                .AddUiForeground(62)
                .AddText(selectedTarget.Name)
                .AddUiForegroundOff();

            if (index == 3) // leves
                sb.AddText($" from {selectedTarget.Issuer}");

            sb.AddText($" in {zoneName}.");

            Service.Plugin.PrintMessage(sb.BuiltString);
        }

        var aetheryteId = GetNearestAetheryte(selectedTarget.Position);
        if (aetheryteId == 0)
        {
            if (index == 1)
            {
                // Dungeons
                this.ShowDutyFinder(selectedTarget.ContentsFinderConditionID);
            }
            else
            {
                PluginLog.Warning($"Could not find an aetheryte for {zoneName}");
            }
        }
        else
        {
            Service.GameGui.OpenMapWithMapLink(selectedTarget.Position);
            this.Teleport(aetheryteId);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct EventData
    {
        [FieldOffset(0x8)]
        public IntPtr Target;

        [FieldOffset(0x10)]
        public IntPtr Addon;
    }
}
