using System;
using System.Linq;
using System.Runtime.InteropServices;

using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

using Sheets = Lumina.Excel.GeneratedSheets;

namespace ZodiacBuddy
{
    /// <summary>
    /// Main plugin implementation.
    /// </summary>
    public sealed partial class ZodiacBuddyPlugin : IDalamudPlugin
    {
        private const string Command = "/pzodiac";

        private readonly WindowSystem windowSystem;
        private readonly ConfigWindow configWindow;
        private readonly Hook<ReceiveEventDelegate> receiveEventHook;
        private readonly OpenDutyDelegate openDuty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZodiacBuddyPlugin"/> class.
        /// </summary>
        /// <param name="pluginInterface">Dalamud plugin interface.</param>
        public ZodiacBuddyPlugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            FFXIVClientStructs.Resolver.Initialize();

            Service.Configuration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
            Service.Address = new PluginAddressResolver();
            Service.Address.Setup();

            this.configWindow = new();
            this.windowSystem = new("ZodiacBuddy");
            this.windowSystem.AddWindow(this.configWindow);

            Service.Interface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
            Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;

            Service.CommandManager.AddHandler(Command, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "Open a window to edit various settings.",
                ShowInHelp = true,
            });

            this.openDuty = Marshal.GetDelegateForFunctionPointer<OpenDutyDelegate>(Service.Address.AgentContentsFinderOpenRegularDutyAddress);

            this.receiveEventHook = new Hook<ReceiveEventDelegate>(Service.Address.AddonRelicNoteBookReceiveEventAddress, this.ReceiveEventDetour);
            this.receiveEventHook.Enable();
        }

        private delegate void ReceiveEventDelegate(IntPtr addon, uint which, IntPtr eventData, IntPtr inputData);

        private delegate IntPtr OpenDutyDelegate(IntPtr agent, uint contentFinderCondition, byte a3);

        /// <inheritdoc/>
        public string Name => "Zodiac Buddy";

        /// <inheritdoc/>
        public void Dispose()
        {
            this.receiveEventHook?.Dispose();

            Service.CommandManager.RemoveHandler(Command);

            Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;
            Service.Interface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
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
                Service.ChatGui.PrintError("Something horrible happened, please contact the developer.");
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

            Service.ChatGui.PrintError("Could not teleport, not attuned.");
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
                PluginLog.Error(ex, "Don't crash the game");
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

            unsafe static bool IsOwnerNode(IntPtr target, AtkComponentCheckBox* checkbox) => target == new IntPtr(checkbox->AtkComponentButton.AtkComponentBase.OwnerNode);

            var selectedTarget = targetComponent switch
            {
                // Enemies
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy0.CheckBox) => Datastore.BraveBooks[bookID].Enemies[0],
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy1.CheckBox) => Datastore.BraveBooks[bookID].Enemies[1],
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy2.CheckBox) => Datastore.BraveBooks[bookID].Enemies[2],
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy3.CheckBox) => Datastore.BraveBooks[bookID].Enemies[3],
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy4.CheckBox) => Datastore.BraveBooks[bookID].Enemies[4],
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy5.CheckBox) => Datastore.BraveBooks[bookID].Enemies[5],
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy6.CheckBox) => Datastore.BraveBooks[bookID].Enemies[6],
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy7.CheckBox) => Datastore.BraveBooks[bookID].Enemies[7],
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy8.CheckBox) => Datastore.BraveBooks[bookID].Enemies[8],
                IntPtr t when index == 0 && IsOwnerNode(t, addonPtr->Enemy9.CheckBox) => Datastore.BraveBooks[bookID].Enemies[9],
                // Dungeons
                IntPtr t when index == 1 && IsOwnerNode(t, addonPtr->Dungeon0.CheckBox) => Datastore.BraveBooks[bookID].Dungeons[0],
                IntPtr t when index == 1 && IsOwnerNode(t, addonPtr->Dungeon1.CheckBox) => Datastore.BraveBooks[bookID].Dungeons[1],
                IntPtr t when index == 1 && IsOwnerNode(t, addonPtr->Dungeon2.CheckBox) => Datastore.BraveBooks[bookID].Dungeons[2],
                // FATEs
                IntPtr t when index == 2 && IsOwnerNode(t, addonPtr->Fate0.CheckBox) => Datastore.BraveBooks[bookID].Fates[0],
                IntPtr t when index == 2 && IsOwnerNode(t, addonPtr->Fate1.CheckBox) => Datastore.BraveBooks[bookID].Fates[1],
                IntPtr t when index == 2 && IsOwnerNode(t, addonPtr->Fate2.CheckBox) => Datastore.BraveBooks[bookID].Fates[2],
                // Leves
                IntPtr t when index == 3 && IsOwnerNode(t, addonPtr->Leve0.CheckBox) => Datastore.BraveBooks[bookID].Leves[0],
                IntPtr t when index == 3 && IsOwnerNode(t, addonPtr->Leve1.CheckBox) => Datastore.BraveBooks[bookID].Leves[1],
                IntPtr t when index == 3 && IsOwnerNode(t, addonPtr->Leve2.CheckBox) => Datastore.BraveBooks[bookID].Leves[2],
                _ => throw new ArgumentException($"Unexpected index and/or node: {index}, {targetComponent:X}"),
            };

            var zoneName = !string.IsNullOrEmpty(selectedTarget.LocationName)
                ? $"{selectedTarget.ZoneName}, {selectedTarget.LocationName}"
                : selectedTarget.ZoneName;

            // PluginLog.Debug($"Target selected: {selectedTarget.Name} in {zoneName}.");
            if (Service.Configuration.BraveEchoTarget)
                Service.ChatGui.Print($"[{this.Name}] Target selected: {selectedTarget.Name} in {zoneName}.");

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

        private void OnOpenConfigUi()
            => this.configWindow.IsOpen = true;

        private void OnCommand(string command, string arguments)
        {
            this.configWindow.IsOpen = true;
        }

        /// <summary>
        /// A collection of targets for a single Trial of the Braves book.
        /// </summary>
        internal struct BraveBook
        {
            /// <summary>
            /// Gets the display name.
            /// </summary>
            public string Name { get; init; }

            /// <summary>
            /// Gets the target enemies.
            /// </summary>
            public BraveTarget[] Enemies { get; init; }

            /// <summary>
            /// Gets the target dungeons.
            /// </summary>
            public BraveTarget[] Dungeons { get; init; }

            /// <summary>
            /// Gets the target fates.
            /// </summary>
            public BraveTarget[] Fates { get; init; }

            /// <summary>
            /// Gets the target leves.
            /// </summary>
            public BraveTarget[] Leves { get; init; }
        }

        /// <summary>
        /// A single target for a Trial of the Braves book.
        /// </summary>
        internal struct BraveTarget
        {
            /// <summary>
            /// Gets the display name.
            /// </summary>
            public string Name { get; init; }

            /// <summary>
            /// Gets the zone name.
            /// </summary>
            public string ZoneName { get; init; }

            /// <summary>
            /// Gets the zone ID.
            /// </summary>
            public uint ZoneID { get; init; }

            /// <summary>
            /// Gets the contents finder condition ID.
            /// </summary>
            public uint ContentsFinderConditionID { get; init; }

            /// <summary>
            /// Gets the location name.
            /// </summary>
            public string LocationName { get; init; }

            /// <summary>
            /// Gets the position that this target is roughly at.
            /// </summary>
            public MapLinkPayload Position { get; init; }
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
}