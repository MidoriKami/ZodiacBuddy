using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

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

            this.receiveEventHook = new Hook<ReceiveEventDelegate>(Service.Address.AddonRelicNoteBookReceiveEventAddress, this.ReceiveEventDetour);
            this.receiveEventHook.Enable();
        }

        /// <summary>
        /// Not require AtkUnitBase.ReceiveEvent, but one step removed. Called when a2==25.
        /// </summary>
        /// <param name="addon">Type receiving the event.</param>
        /// <param name="which">Internal routing number.</param>
        /// <param name="eventData">Event data.</param>
        /// <param name="inputData">Keyboard and mouse data.</param>
        internal unsafe delegate void ReceiveEventDelegate(IntPtr addon, uint which, IntPtr eventData, IntPtr inputData);

        /// <inheritdoc/>
        public string Name => "Zodiac Buddy";

        /// <inheritdoc/>
        public void Dispose()
        {
            this.receiveEventHook?.Dispose();

            Service.CommandManager.RemoveHandler(Command);

            Service.Interface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
            Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;
        }

        private unsafe void ReceiveEventDetour(IntPtr addon, uint which, IntPtr eventData, IntPtr inputData)
        {
            try
            {
                var relicNote = FFXIVClientStructs.FFXIV.Client.Game.UI.RelicNote.Instance();
                if (relicNote == null)
                    return;

                var bookID = relicNote->RelicNoteID;

                var addonPtr = (AddonRelicNoteBook*)addon;
                var index = addonPtr->CategoryList->SelectedItemIndex;

                var eventDataPtr = (EventData*)eventData;
                var targetComponent = eventDataPtr->Target;

                unsafe static IntPtr GetOwnerNode(AtkComponentCheckBox* checkbox) => new(checkbox->AtkComponentButton.AtkComponentBase.OwnerNode);

                BraveTarget selectedTarget;

                if (index == 0)
                { // Enemies
                    var typeIndex = targetComponent switch
                    {
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy0.CheckBox) => 0,
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy1.CheckBox) => 1,
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy2.CheckBox) => 2,
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy3.CheckBox) => 3,
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy4.CheckBox) => 4,
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy5.CheckBox) => 5,
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy6.CheckBox) => 6,
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy7.CheckBox) => 7,
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy8.CheckBox) => 8,
                        IntPtr t when t == GetOwnerNode(addonPtr->Enemy9.CheckBox) => 9,
                        _ => throw new ArgumentException("Unexpected enemy index"),
                    };

                    selectedTarget = Datastore.BraveBooks[bookID].Enemies[typeIndex];
                    // PluginLog.Debug($"Enemies {selectedTarget.Name}");
                }
                else if (index == 1)
                { // Dungeons
                    var typeIndex = targetComponent switch
                    {
                        IntPtr t when t == GetOwnerNode(addonPtr->Dungeon0.CheckBox) => 0,
                        IntPtr t when t == GetOwnerNode(addonPtr->Dungeon1.CheckBox) => 1,
                        IntPtr t when t == GetOwnerNode(addonPtr->Dungeon2.CheckBox) => 2,
                        _ => throw new ArgumentException("Unexpected dungeon index"),
                    };

                    selectedTarget = Datastore.BraveBooks[bookID].Dungeons[typeIndex];
                    // PluginLog.Debug($"Dungeons {selectedTarget.Name}");
                }
                else if (index == 2)
                { // FATEs
                    var typeIndex = targetComponent switch
                    {
                        IntPtr t when t == GetOwnerNode(addonPtr->Fate0.CheckBox) => 0,
                        IntPtr t when t == GetOwnerNode(addonPtr->Fate1.CheckBox) => 1,
                        IntPtr t when t == GetOwnerNode(addonPtr->Fate2.CheckBox) => 2,
                        _ => throw new ArgumentException("Unexpected fate index"),
                    };

                    selectedTarget = Datastore.BraveBooks[bookID].Fates[typeIndex];
                    // PluginLog.Debug($"FATEs {selectedTarget.Name}");
                }
                else if (index == 3)
                { // Leves
                    var typeIndex = targetComponent switch
                    {
                        IntPtr t when t == GetOwnerNode(addonPtr->Leve0.CheckBox) => 0,
                        IntPtr t when t == GetOwnerNode(addonPtr->Leve1.CheckBox) => 1,
                        IntPtr t when t == GetOwnerNode(addonPtr->Leve2.CheckBox) => 2,
                        _ => throw new ArgumentException("Unexpected leve index"),
                    };

                    selectedTarget = Datastore.BraveBooks[bookID].Leves[typeIndex];
                    // PluginLog.Debug($"Leves {selectedTarget.Name}");
                }
                else
                {
                    throw new ArgumentException($"Unexpected list index: {index}");
                }

                var zoneName = !string.IsNullOrEmpty(selectedTarget.LocationName)
                    ? $"{selectedTarget.ZoneName}, {selectedTarget.LocationName}"
                    : selectedTarget.ZoneName;

                // PluginLog.Debug($"Target selected: {selectedTarget.Name} in {zoneName}.");
                if (Service.Configuration.BraveEchoTarget)
                    Service.ChatGui.Print($"[{this.Name}] Target selected: {selectedTarget.Name} in {zoneName}.");

                var aetheryteName = this.GetNearestAetheryte(selectedTarget.Position);
                if (string.IsNullOrEmpty(aetheryteName))
                {
                    if (index == 1)
                    { // Dungeons
                        Service.ChatGui.Print($"[{this.Name}] Maybe someday this will queue you for a dungeon.");
                    }
                    else
                    {
                        PluginLog.Warning($"Could not find an aetheryte for {zoneName}");
                        Service.ChatGui.Print($"[{this.Name}] Could not find an appropriate aetheryte. Please report this to the developer.");
                    }
                }
                else
                {
                    // PluginLog.Debug($"Aetheryte chosen: {aetheryteName}");
                    Service.GameGui.OpenMapWithMapLink(selectedTarget.Position);
                    if (!Service.CommandManager.ProcessCommand($"/tp {aetheryteName}"))
                    {
                        PluginLog.Warning($"Teleport command failed, teleporter is probably not installed");
                        Service.ChatGui.PrintError($"[{this.Name}] Teleport failed. This feature requires the plugin \"Teleporter\" to be installed.");
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }
        }

        private string GetNearestAetheryte(MapLinkPayload mapLink)
        {
            var closestAetheryteName = string.Empty;
            var closestDistance = double.MaxValue;

            static float ConvertRawPositionToMapCoordinate(int pos, float scale)
            {
                var c = scale / 100.0f;
                var scaledPos = pos * c / 1000.0f;

                return (41.0f / c * ((scaledPos + 1024.0f) / 2048.0f)) + 1.0f;
            }

            var aetherytes = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Aetheryte>()!;
            var mapMarkers = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.MapMarker>()!;

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
                    return string.Empty;
                }

                var aetherX = ConvertRawPositionToMapCoordinate(mapMarker.X, scale);
                var aetherY = ConvertRawPositionToMapCoordinate(mapMarker.Y, scale);
                var aetherName = aetheryte.PlaceName.Value!.Name;
                // PluginLog.Debug($"Aetheryte found: {aetherName} ({aetherX} ,{aetherY})");

                var distance = Math.Pow(aetherX - mapLink.XCoord, 2) + Math.Pow(aetherY - mapLink.YCoord, 2);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestAetheryteName = aetherName;
                }
            }

            return closestAetheryteName;
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