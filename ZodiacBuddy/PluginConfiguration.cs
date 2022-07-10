using System;

using Dalamud.Configuration;
using Dalamud.Game.Text;
using Newtonsoft.Json;
using ZodiacBuddy.BonusLight;
using ZodiacBuddy.Stages.Brave;
using ZodiacBuddy.Stages.Novus;

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
        /// Gets the configuration for bonus light options.
        /// </summary>
        public BonusLightConfiguration BonusLight { get; } = new();

        /// <summary>
        /// Gets the configuration for Novus relics.
        /// </summary>
        public NovusConfiguration Novus { get; } = new();

        /// <summary>
        /// Gets configuration for Zodiac Brave relics.
        /// </summary>
        public BraveConfiguration Brave { get; } = new();

        /// <summary>
        /// Save the configuration to disk.
        /// </summary>
        public void Save()
            => Service.Interface.SavePluginConfig(this);
    }
}
