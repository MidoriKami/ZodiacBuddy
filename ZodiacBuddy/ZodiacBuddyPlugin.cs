using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ZodiacBuddy.Atma;
using ZodiacBuddy.Novus;

namespace ZodiacBuddy
{
    /// <summary>
    /// Main plugin implementation.
    /// </summary>
    public sealed partial class ZodiacBuddyPlugin : IDalamudPlugin
    {
        private const string Command = "/pzodiac";

        private readonly AtmaManager animusBuddy;
        private readonly NovusManager novusManager;

        private readonly WindowSystem windowSystem;
        private readonly ConfigWindow configWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZodiacBuddyPlugin"/> class.
        /// </summary>
        /// <param name="pluginInterface">Dalamud plugin interface.</param>
        public ZodiacBuddyPlugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            FFXIVClientStructs.Resolver.Initialize();

            Service.Plugin = this;
            Service.Configuration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();

            this.windowSystem = new("ZodiacBuddy");
            this.windowSystem.AddWindow(this.configWindow = new());

            Service.Interface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
            Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;

            Service.CommandManager.AddHandler(Command, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "Open a window to edit various settings.",
                ShowInHelp = true,
            });

            this.animusBuddy = new AtmaManager();
            this.novusManager = new NovusManager();
        }

        /// <inheritdoc/>
        public string Name => "Zodiac Buddy";

        /// <inheritdoc/>
        public void Dispose()
        {
            Service.CommandManager.RemoveHandler(Command);

            Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;
            Service.Interface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;

            this.animusBuddy?.Dispose();
            this.novusManager?.Dispose();
        }

        /// <summary>
        /// Print a message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        public void PrintMessage(SeString message)
        {
            var sb = new SeStringBuilder()
                .AddUiForeground(45)
                .AddText($"[{Service.Plugin.Name}] ")
                .AddUiForegroundOff()
                .Append(message);

            Service.ChatGui.PrintChat(new XivChatEntry()
            {
                Type = Service.Configuration.ChatType,
                Message = sb.BuiltString,
            });
        }

        /// <summary>
        /// Print an error message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        public void PrintError(string message)
        {
            Service.ChatGui.PrintError($"[{this.Name}] {message}");
        }

        private void OnOpenConfigUi()
            => this.configWindow.IsOpen = true;

        private void OnCommand(string command, string arguments)
        {
            this.configWindow.IsOpen = true;
        }
    }
}