﻿using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.Settings
{
    public class PuddleSettings
    {
        // Note: This property can only override disabling, it cannot force a user to enable reflections
        public const string MapProperty_IsEnabled = "ArePuddleReflectionsEnabled";
        public bool AreReflectionsEnabled { get; set; } = true;

        public const string MapProperty_ShouldGeneratePuddles = "ShouldGeneratePuddles";
        public bool ShouldGeneratePuddles { get; set; } = true;

        public const string MapProperty_ShouldPlaySplashSound = "ShouldPlaySplashSound";
        public bool ShouldPlaySplashSound { get; set; } = true;

        public const string MapProperty_ShouldRainSplashPuddles = "ShouldRainSplashPuddles";
        public bool ShouldRainSplashPuddles { get; set; } = true;


        public const string MapProperty_PuddlePercentageWhileRaining = "PuddlePercentageWhileRaining";
        public int PuddlePercentageWhileRaining { get; set; } = 20;

        public const string MapProperty_PuddlePercentageAfterRaining = "PuddlePercentageAfterRaining";
        public int PuddlePercentageAfterRaining { get; set; } = 10;

        public const string MapProperty_MillisecondsBetweenRaindropSplashes = "MillisecondsBetweenRaindropSplashes";
        public int MillisecondsBetweenRaindropSplashes { get; set; } = 500;

        public const string MapProperty_PuddleColor = "PuddleColor";
        public Color PuddleColor { get; set; } = new Color(91, 91, 91, 91);

        public const string MapProperty_RippleColor = "RippleColor";
        public Color RippleColor { get; set; } = new Color(255, 255, 255, 155);

        public void Reset(PuddleSettings referencedSettings = null)
        {
            if (referencedSettings is null)
            {
                AreReflectionsEnabled = true;
                ShouldGeneratePuddles = true;
                ShouldPlaySplashSound = true;
                ShouldRainSplashPuddles = true;
                PuddlePercentageWhileRaining = 20;
                PuddlePercentageAfterRaining = 10;
                MillisecondsBetweenRaindropSplashes = 500;
                PuddleColor = new Color(91, 91, 91, 91);
                RippleColor = new Color(255, 255, 255, 155);
            }
            else
            {
                AreReflectionsEnabled = referencedSettings.AreReflectionsEnabled;
                ShouldGeneratePuddles = referencedSettings.ShouldGeneratePuddles;
                ShouldPlaySplashSound = referencedSettings.ShouldPlaySplashSound;
                ShouldRainSplashPuddles = referencedSettings.ShouldRainSplashPuddles;
                PuddlePercentageWhileRaining = referencedSettings.PuddlePercentageWhileRaining;
                PuddlePercentageAfterRaining = referencedSettings.PuddlePercentageAfterRaining;
                MillisecondsBetweenRaindropSplashes = referencedSettings.MillisecondsBetweenRaindropSplashes;
                PuddleColor = referencedSettings.PuddleColor;
                RippleColor = referencedSettings.RippleColor;
            }
        }
    }
}