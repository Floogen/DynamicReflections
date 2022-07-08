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
        public bool IsEnabled { get; set; } = true;

        public const string MapProperty_ReflectionDirection = "WaterReflectionDirection";
        public Direction ReflectionDirection { get; set; } = Direction.South;

        public const string MapProperty_ReflectionOverlay = "WaterReflectionOverlay";
        public Color ReflectionOverlay { get; set; } = Color.White;
        private Color _actualReflectionOverlay;

        public const string MapProperty_ReflectionOffset = "WaterReflectionOffset";
        public Vector2 ReflectionOffset { get; set; } = new Vector2(0f, 1.5f);

        public const string MapProperty_IsReflectionWavy = "IsWaterReflectionWavy";
        public bool IsReflectionWavy { get; set; } = false;

        public const string MapProperty_WaveSpeed = "WaterReflectionWaveSpeed";
        public float WaveSpeed { get; set; } = 1f;

        public const string MapProperty_WaveAmplitude = "WaterReflectionWaveAmplitude";
        public float WaveAmplitude { get; set; } = 0.01f;

        public const string MapProperty_WaveFrequency = "WaterReflectionWaveFrequency";
        public float WaveFrequency { get; set; } = 50f;

        public void Reset(WaterSettings referencedSettings = null)
        {
            if (referencedSettings is null)
            {
                IsEnabled = true;
                ReflectionDirection = Direction.South;
                ReflectionOverlay = Color.White;
                ReflectionOffset = new Vector2(0f, 1.5f);
                IsReflectionWavy = false;
                WaveSpeed = 1f;
                WaveAmplitude = 0.01f;
                WaveFrequency = 50f;
            }
            else
            {
                IsEnabled = referencedSettings.IsEnabled;
                ReflectionDirection = referencedSettings.ReflectionDirection;
                ReflectionOverlay = referencedSettings.ReflectionOverlay;
                ReflectionOffset = referencedSettings.ReflectionOffset;
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
