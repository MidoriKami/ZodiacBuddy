using System;

using Dalamud.Configuration;
using Dalamud.Game.Text;

namespace ZodiacBuddy
{
    /// <summary>
    /// Plugin configuration.
    /// </summary>
    [Serializable]
    public class PluginConfiguration : IPluginConfiguration
    {
        /// <summary>
        /// Gets or sets the configuration version.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether to echo the target before teleporting to a Brave target.
        /// </summary>
        public bool BraveEchoTarget { get; set; } = true;

        /// <summary>
        /// Gets or sets the channel that receives target information.
        /// </summary>
        public XivChatType BraveEchoChannel { get; set; } = XivChatType.Echo;

        /// <summary>
        /// Save the configuration to disk.
        /// </summary>
        public void Save()
            => Service.Interface.SavePluginConfig(this);
    }
}
