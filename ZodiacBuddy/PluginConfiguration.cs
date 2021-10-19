using System;

using Dalamud.Configuration;

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
        /// Save the configuration to disk.
        /// </summary>
        public void Save()
            => Service.Interface.SavePluginConfig(this);
    }
}
