using DynamicReflections.Framework.Models.Settings;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.External.GenericModConfigMenu
{
    public class ModConfig
    {
        public bool AreWaterReflectionsEnabled { get; set; } = true;
        public bool AreMirrorReflectionsEnabled { get; set; } = true;
        public bool ArePuddleReflectionsEnabled { get; set; } = true;
        public bool AreNPCReflectionsEnabled { get; set; } = true;
        public bool AreSkyReflectionsEnabled { get; set; } = true;
        public WaterSettings WaterReflectionSettings { get; set; } = new WaterSettings();
        public PuddleSettings PuddleReflectionSettings { get; set; } = new PuddleSettings();
        public Dictionary<string, WaterSettings> LocalWaterReflectionSettings { get; set; } = new Dictionary<string, WaterSettings>();
        public Dictionary<string, PuddleSettings> LocalPuddleReflectionSettings { get; set; } = new Dictionary<string, PuddleSettings>();
        public SButton QuickMenuKey { get; set; } = SButton.R;

        public WaterSettings GetCurrentWaterSettings(GameLocation location)
        {
            if (location is null || LocalWaterReflectionSettings is null || LocalWaterReflectionSettings.ContainsKey(location.NameOrUniqueName) is false || LocalWaterReflectionSettings[location.NameOrUniqueName] is null || LocalWaterReflectionSettings[location.NameOrUniqueName].OverrideDefaultSettings is false)
            {
                return WaterReflectionSettings;
            }

            return LocalWaterReflectionSettings[location.NameOrUniqueName];
        }

        public PuddleSettings GetCurrentPuddleSettings(GameLocation location)
        {
            if (location is null || LocalPuddleReflectionSettings is null || LocalPuddleReflectionSettings.ContainsKey(location.NameOrUniqueName) is false || LocalPuddleReflectionSettings[location.NameOrUniqueName] is null || LocalPuddleReflectionSettings[location.NameOrUniqueName].OverrideDefaultSettings is false)
            {
                return PuddleReflectionSettings;
            }

            return LocalPuddleReflectionSettings[location.NameOrUniqueName];
        }
    }
}
