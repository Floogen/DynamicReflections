using DynamicReflections.Framework.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.External.GenericModConfigMenu
{
    public class ModConfig
    {
        public bool AreWaterReflectionsEnabled { get { return WaterReflectionSettings.AreReflectionsEnabled; } set { WaterReflectionSettings.AreReflectionsEnabled = value; } }
        public bool AreMirrorReflectionsEnabled { get; set; } = true;
        public bool ArePuddleReflectionsEnabled { get { return PuddleReflectionSettings.AreReflectionsEnabled; } set { PuddleReflectionSettings.AreReflectionsEnabled = value; } }
        public WaterSettings WaterReflectionSettings { get; set; } = new WaterSettings();
        public PuddleSettings PuddleReflectionSettings { get; set; } = new PuddleSettings();
    }
}
