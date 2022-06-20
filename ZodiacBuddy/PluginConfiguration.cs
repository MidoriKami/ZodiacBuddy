using System;

using Dalamud.Configuration;
using Dalamud.Game.Text;
using Newtonsoft.Json;
using ZodiacBuddy.Novus;

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
        /// Gets or sets the chat channel.
        /// </summary>
        [JsonProperty("BraveEchoChannel")]
        public XivChatType ChatType { get; set; } = XivChatType.Echo;

        /// <summary>
        /// Gets or sets a value indicating whether to echo the target before teleporting to a Brave target.
        /// </summary>
        public bool BraveEchoTarget { get; set; } = true;

        /// <summary>
        /// Gets configuration for Novus relic.
        /// </summary>
        public NovusConfiguration NovusConfiguration { get; } = new();

        /// <summary>
        /// Save the configuration to disk.
        /// </summary>
        public void Save()
            => Service.Interface.SavePluginConfig(this);
    }
}
