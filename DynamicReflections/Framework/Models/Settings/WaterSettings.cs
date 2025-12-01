using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.Settings
{
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public class WaterSettings
    {
        // Note: This property can only override disabling, it cannot force a user to enable reflections
        public const string MapProperty_IsEnabled = "AreWaterReflectionsEnabled";
        public bool AreReflectionsEnabled { get; set; } = true;

        public const string MapProperty_ReflectionDirection = "WaterReflectionDirection";
        public Direction ReflectionDirection { get; set; } = Direction.South;

        public const string MapProperty_ReflectionOverlay = "WaterReflectionOverlay";
        public Color ReflectionOverlay { get; set; } = Color.White;

        public const string MapProperty_ReflectionOffset = "WaterReflectionOffset";
        public Vector2 PlayerReflectionOffset { get; set; } = new Vector2(0f, 0.5f);
        public const string MapProperty_NPCReflectionOffset = "WaterNPCReflectionOffset";
        public Vector2 NPCReflectionOffset { get; set; } = new Vector2(0f, 0.7f);
        public const string MapProperty_CompanionReflectionOffset = "WaterCompanionReflectionOffset";
        public Vector2 CompanionReflectionOffset { get; set; } = new Vector2(0f, 0.3f);

        public const string MapProperty_IsReflectionWavy = "IsWaterReflectionWavy";
        public bool IsReflectionWavy { get; set; } = true;

        public const string MapProperty_WaveSpeed = "WaterReflectionWaveSpeed";
        public float WaveSpeed { get; set; } = 2f;

        public const string MapProperty_WaveAmplitude = "WaterReflectionWaveAmplitude";
        public float WaveAmplitude { get; set; } = 0.001f;

        public const string MapProperty_WaveFrequency = "WaterReflectionWaveFrequency";
        public float WaveFrequency { get; set; } = 200f;


        public bool OverrideDefaultSettings { get; set; }

        public void Reset(WaterSettings referencedSettings = null)
        {
            if (referencedSettings is null)
            {
                AreReflectionsEnabled = true;
                ReflectionDirection = Direction.South;
                ReflectionOverlay = Color.White;
                PlayerReflectionOffset = new Vector2(0f, 0.5f);
                NPCReflectionOffset = new Vector2(0f, 0.7f);
                CompanionReflectionOffset = new Vector2(0f, 0.3f);
                IsReflectionWavy = true;
                WaveSpeed = 2f;
                WaveAmplitude = 0.001f;
                WaveFrequency = 200f;
                OverrideDefaultSettings = false;
            }
            else
            {
                AreReflectionsEnabled = referencedSettings.AreReflectionsEnabled;
                ReflectionDirection = referencedSettings.ReflectionDirection;
                ReflectionOverlay = referencedSettings.ReflectionOverlay;
                PlayerReflectionOffset = referencedSettings.PlayerReflectionOffset;
                NPCReflectionOffset = referencedSettings.NPCReflectionOffset;
                CompanionReflectionOffset = referencedSettings.CompanionReflectionOffset;
                IsReflectionWavy = referencedSettings.IsReflectionWavy;
                WaveSpeed = referencedSettings.WaveSpeed;
                WaveAmplitude = referencedSettings.WaveAmplitude;
                WaveFrequency = referencedSettings.WaveFrequency;
            }
        }

        public bool IsFacingCorrectDirection(int direction)
        {
            if (direction == 0 && ReflectionDirection == Direction.North)
            {
                return true;
            }
            else if (direction == 2 && ReflectionDirection == Direction.South)
            {
                return true;
            }

            return false;
        }
    }
}