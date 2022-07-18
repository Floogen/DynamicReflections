using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.Settings
{
    public class SkySettings
    {
        // Note: This property can only override disabling, it cannot force a user to enable reflections
        public const string MapProperty_IsEnabled = "AreSkyReflectionsEnabled";
        public bool AreReflectionsEnabled { get; set; } = true;

        public const string MapProperty_StarDensityPercentage = "StarDensityPercentage";
        public int StarDensityPercentage { get; set; } = 55;

        public const string MapProperty_AreShootingStarsEnabled = "AreShootingStarsEnabled";
        public bool AreShootingStarsEnabled { get; set; } = true;

        public const string MapProperty_MillisecondsBetweenShootingStarAttempt = "MillisecondsBetweenShootingStarAttempt";
        public int MillisecondsBetweenShootingStarAttempt { get; set; } = 5000;

        public const string MapProperty_MaxShootingStarAttemptsPerInterval = "MaxShootingStarAttemptsPerInterval";
        public int MaxShootingStarAttemptsPerInterval { get; set; } = 5;

        public const string MapProperty_CometChance = "CometChance";
        public int CometChance { get; set; } = 10;

        public const string MapProperty_MillisecondsBetweenShootingStarAttemptDuringMeteorShower = "MillisecondsBetweenShootingStarAttemptDuringMeteorShower";
        public int MillisecondsBetweenShootingStarAttemptDuringMeteorShower { get; set; } = 250;


        public bool OverrideDefaultSettings { get; set; }

        public void Reset(SkySettings referencedSettings = null)
        {
            if (referencedSettings is null)
            {
                AreReflectionsEnabled = true;
                StarDensityPercentage = 55;
                AreShootingStarsEnabled = true;
                MillisecondsBetweenShootingStarAttempt = 5000;
                MaxShootingStarAttemptsPerInterval = 5;
                CometChance = 10;
                MillisecondsBetweenShootingStarAttemptDuringMeteorShower = 250;
                OverrideDefaultSettings = false;
            }
            else
            {
                AreReflectionsEnabled = referencedSettings.AreReflectionsEnabled;
                StarDensityPercentage = referencedSettings.StarDensityPercentage;
                AreShootingStarsEnabled = referencedSettings.AreShootingStarsEnabled;
                MillisecondsBetweenShootingStarAttempt = referencedSettings.MillisecondsBetweenShootingStarAttempt;
                MaxShootingStarAttemptsPerInterval = referencedSettings.MaxShootingStarAttemptsPerInterval;
                CometChance = referencedSettings.CometChance;
                MillisecondsBetweenShootingStarAttemptDuringMeteorShower = referencedSettings.MillisecondsBetweenShootingStarAttemptDuringMeteorShower;
            }
        }
    }
}
