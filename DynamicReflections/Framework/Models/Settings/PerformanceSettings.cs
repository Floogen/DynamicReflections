using System;

namespace DynamicReflections.Framework.Models.Settings
{
    public class PerformanceSettings
    {
        /// <summary>
        /// If true, some internal calculations (like water / puddle / mirror checks) can be cached
        /// to reduce CPU usage.
        /// </summary>
        public bool EnableSafeCaching { get; set; } = true;

        /// <summary>
        /// If true, NPC reflections can be updated less often and capped to reduce CPU usage.
        /// </summary>
        public bool EnableNpcThrottling { get; set; } = false;

        /// <summary>
        /// How often to update NPC reflections, in ticks. 1 = every tick, 2 = every other tick, etc.
        /// </summary>
        public int NpcUpdateIntervalTicks { get; set; } = 2;

        /// <summary>
        /// Maximum number of NPC reflections processed per location.
        /// </summary>
        public int MaxNpcReflections { get; set; } = 100;

        /// <summary>
        /// If true, checks for which mirrors are active can be throttled.
        /// </summary>
        public bool EnableMirrorThrottling { get; set; } = false;

        /// <summary>
        /// How often to re-evaluate which mirrors are active, in ticks.
        /// </summary>
        public int MirrorUpdateIntervalTicks { get; set; } = 2;

        /// <summary>
        /// If true, reflections for companions / wild animals can be throttled separately
        /// from normal NPCs.
        /// </summary>
        public bool EnableCompanionThrottling { get; set; } = false;

        /// <summary>
        /// How often to update reflections for companions / wild animals, in ticks.
        /// 1 = every tick.
        /// </summary>
        public int CompanionUpdateIntervalTicks { get; set; } = 2;

        /// <summary>
        /// Maximum number of reflections for companions / wild animals to process per location.
        /// </summary>
        public int MaxCompanionReflections { get; set; } = 100;
    }
}
