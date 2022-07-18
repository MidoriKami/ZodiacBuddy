using System;

namespace ZodiacBuddy.InformationWindow
{
    /// <summary>
    /// Configuration class for weapons information window.
    /// </summary>
    public class InformationWindowConfiguration
    {
        /// <summary>
        /// Default color for the bonus light information progress bar.
        /// </summary>
        [NonSerialized]
        private static readonly uint DefaultProgressColor = 0xFF943463;

        /// <summary>
        /// Gets or sets a value indicating whether to enable manual resizing of the weapons information window.
        /// </summary>
        public bool ManualSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable click through the weapons information window.
        /// </summary>
        public bool ClickThrough { get; set; }

        /// <summary>
        /// Gets or sets the color of the progress bar on weapons information.
        /// </summary>
        public uint ProgressColor { get; set; } = DefaultProgressColor;

        /// <summary>
        /// Gets or sets a value indicating whether to resize automatically the progress bar.
        /// </summary>
        public bool ProgressAutoSize { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating the size of the progress bar.
        /// </summary>
        public int ProgressSize { get; set; } = 130;

        /// <summary>
        /// Reset ProgressColor to it's default value.
        /// </summary>
        public void ResetProgressColor()
        {
            this.ProgressColor = DefaultProgressColor;
        }
    }
}