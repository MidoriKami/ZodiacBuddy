using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ZodiacBuddy.BonusLight;
using ZodiacBuddy.Stages.Atma;
using ZodiacBuddy.Stages.Brave;
using ZodiacBuddy.Stages.Novus;

namespace ZodiacBuddy;

/// <summary>
/// Main plugin implementation.
/// </summary>
public sealed class ZodiacBuddyPlugin : IDalamudPlugin {
    private const string Command = "/pzodiac";

    private readonly AtmaManager animusBuddy;
    private readonly NovusManager novusManager;
    private readonly BraveManager braveManager;

    private readonly WindowSystem windowSystem;
    private readonly ConfigWindow configWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZodiacBuddyPlugin"/> class.
    /// </summary>
    /// <param name="pluginInterface">Dalamud plugin interface.</param>
    public ZodiacBuddyPlugin(IDalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Service>();

        Service.Plugin = this;
        Service.Configuration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();

        this.windowSystem = new WindowSystem("ZodiacBuddy");
        this.windowSystem.AddWindow(this.configWindow = new ConfigWindow());

        Service.Interface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
        Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;

        Service.CommandManager.AddHandler(Command, new CommandInfo(this.OnCommand) {
            HelpMessage = "Open a window to edit various settings.",
            ShowInHelp = true,
        });

        Service.BonusLightManager = new BonusLightManager();
        this.animusBuddy = new AtmaManager();
        this.novusManager = new NovusManager();
        this.braveManager = new BraveManager();
    }

    /// <inheritdoc/>
    public void Dispose() {
        Service.CommandManager.RemoveHandler(Command);

        Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;
        Service.Interface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;

        this.animusBuddy.Dispose();
        this.novusManager.Dispose();
        this.braveManager.Dispose();
        Service.BonusLightManager.Dispose();
    }

    /// <summary>
    /// Print a message.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public void PrintMessage(SeString message) {
        var sb = new SeStringBuilder()
            .AddUiForeground(45)
            .AddText("[ZodiacBuddy] ")
            .AddUiForegroundOff()
            .Append(message);

        Service.ChatGui.Print(new XivChatEntry {
            Type = Service.Configuration.ChatType,
            Message = sb.BuiltString,
        });
    }

    /// <summary>
    /// Print an error message.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public static void PrintError(string message)
        => Service.ChatGui.PrintError($"[ZodiacBuddy] {message}");

    private void OnOpenConfigUi()
        => this.configWindow.IsOpen = true;

    private void OnCommand(string command, string arguments)
        => this.configWindow.IsOpen = true;
}