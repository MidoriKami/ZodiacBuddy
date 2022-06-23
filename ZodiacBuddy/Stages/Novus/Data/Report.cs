using System;

namespace ZodiacBuddy.Stages.Novus.Data
{
    /// <summary>
    /// Report strut to communicate with the server.
    /// </summary>
    public struct Report
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Report"/> struct.
        /// </summary>
        /// <param name="datacenter">Datacenter id.</param>
        /// <param name="world">World id.</param>
        /// <param name="territory">Territory id.</param>
        /// <param name="date">Date of the report.</param>
        public Report(uint datacenter, uint world, uint territory, DateTime date)
        {
            this.Datacenter = datacenter;
            this.World = world;
            this.Territory = territory;
            this.Date = date;
        }

        /// <summary>
        /// Gets Datacenter id.
        /// </summary>
        public uint Datacenter { get; }

        /// <summary>
        /// Gets World id.
        /// </summary>
        public uint World { get; }

        /// <summary>
        /// Gets territory id.
        /// </summary>
        public uint Territory { get; }

        /// <summary>
        /// Gets report date.
        /// </summary>
        public DateTime Date { get; }
    }
}