using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using DynamicReflections.Framework.Models;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using DynamicReflections.Framework.Patches.SMAPI;
using DynamicReflections.Framework.Patches.Tiles;
using DynamicReflections.Framework.Patches.Tools;

namespace DynamicReflections
{
    public class DynamicReflections : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        // Config options
        internal static bool areWaterReflectionsEnabled = true;
        internal static bool areMirrorReflectionsEnabled = true;

        // Water reflection variables
        internal static float waterReflectionYOffset = 1.5f;
        internal static Vector2? waterReflectionPosition;
        internal static Vector2? waterReflectionTilePosition;
        internal static bool shouldDrawWaterReflection;
        internal static bool isDrawingWaterReflection;
        internal static bool isFilteringWater;
        internal static bool isWavyReflection = true;

        // Mirror reflection variables
        internal static FarmerSprite mirrorReflectionSprite;
        internal static Dictionary<Point, Mirror> mapMirrors = new Dictionary<Point, Mirror>();
        internal static List<Point> activeMirrorPositions = new List<Point>();
        internal static float mirrorReflectionYOffset = 1.5f;
        internal static bool shouldDrawMirrorReflection;
        internal static bool isDrawingMirrorReflection;
        internal static bool isFilteringMirror;
        internal static bool shouldOverrideHorizontalFlip;

        // Effects and RenderTarget2Ds
        internal static Effect opacityEffect;
        internal static Effect waterReflectionEffect;
        internal static Effect mirrorReflectionEffect;
        internal static RenderTarget2D playerWaterReflectionRender;
        internal static RenderTarget2D[] rawPlayerMirrorReflectionRenders;
        internal static RenderTarget2D[] modifiedPlayerMirrorReflectionRenders;
        internal static RenderTarget2D mirrorsRenderTarget;
        internal static RasterizerState rasterizer;


        // TODO: Implement these map / tile properties
        // Note: These water reflection map properties override the player's config for the current map (if set)
        // Map property - WaterReflectionsEnabled - T or F
        // Map property - WaterReflectionDirection - 0 or 2
        // Map property - WaterReflectionOpacity - 0.0 to 1.0
        // Map property - WaterReflectionIsWavy - T or F
        // Map property - WaterReflectionWavySpeed - Any float
        // Map property - WaterReflectionWavyAmplitude - Any float
        // Map property - WaterReflectionWavyFrequency - Any float

        // Tile property - IsMirrorBase - T or F
        // Tile property - MirrorHeight - Any int
        // Tile property - MirrorWidth - Any int
        // Tile property - MirrorReflectionScale - Any float
        // Tile property - MirrorReflectionOpacity - 0.0 to 1.0
        // Tile property - MirrorReflectionYOffset - Any float


        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply patches
                new LayerPatch(monitor, modHelper).Apply(harmony);
                new DisplayDevicePatch(monitor, modHelper).Apply(harmony);
                new ToolPatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Hook into the required events
            helper.Events.Display.WindowResized += OnWindowResized;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnWindowResized(object sender, StardewModdingAPI.Events.WindowResizedEventArgs e)
        {
            if (playerWaterReflectionRender is not null)
            {
                playerWaterReflectionRender.Dispose();
            }

            playerWaterReflectionRender = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            if (mirrorsRenderTarget is not null)
            {
                mirrorsRenderTarget.Dispose();
            }

            mirrorsRenderTarget = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            foreach (var mirrorPlayerRender in rawPlayerMirrorReflectionRenders)
            {
                if (mirrorPlayerRender is not null)
                {
                    mirrorPlayerRender.Dispose();
                }
            }
            rawPlayerMirrorReflectionRenders = new RenderTarget2D[3];
            for (int i = 0; i < 3; i++)
            {
                rawPlayerMirrorReflectionRenders[i] = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);
            }

            foreach (var mirrorPlayerRender in modifiedPlayerMirrorReflectionRenders)
            {
                if (mirrorPlayerRender is not null)
                {
                    mirrorPlayerRender.Dispose();
                }
            }
            modifiedPlayerMirrorReflectionRenders = new RenderTarget2D[3];
            for (int i = 0; i < 3; i++)
            {
                modifiedPlayerMirrorReflectionRenders[i] = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);
            }

            if (rasterizer is not null)
            {
                rasterizer.Dispose();
            }

            rasterizer = new RasterizerState();
            rasterizer.CullMode = CullMode.CullClockwiseFace;
        }


        private void OnWarped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (Context.IsWorldReady is false || e.NewLocation is null)
            {
                return;
            }
            var currentLocation = e.NewLocation;

            // Clear the old base points out
            DynamicReflections.mapMirrors.Clear();

            // Check current map for tiles with IsMirrorBase
            var map = currentLocation.Map;
            if (map is null || (map.GetLayer("Mirrors") is var mirrorLayer && mirrorLayer is null))
            {
                return;
            }

            for (int x = 0; x < mirrorLayer.LayerWidth; x++)
            {
                for (int y = 0; y < mirrorLayer.LayerHeight; y++)
                {
                    if (IsMirrorBaseTile(currentLocation, x, y, true) is false)
                    {
                        continue;
                    }

                    var point = new Point(x, y);
                    if (DynamicReflections.mapMirrors.ContainsKey(point) is false)
                    {
                        DynamicReflections.mapMirrors[point] = new Mirror()
                        {
                            TilePosition = point,
                            Height = GetMirrorHeight(currentLocation, x, y) - 1,
                            Width = GetMirrorWidth(currentLocation, x, y),
                            ReflectionScale = GetMirrorScale(currentLocation, x, y), // TODO: Implement this property
                            ReflectionOpacity = GetMirrorOpacity(currentLocation, x, y), // TODO: Implement this property
                            ReflectionOffset = GetMirrorOffset(currentLocation, x, y)
                        };
                    }
                }
            }
        }

        private void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady is false)
            {
                return;
            }

            DynamicReflections.shouldDrawWaterReflection = false;
            if (DynamicReflections.areWaterReflectionsEnabled)
            {
                var playerPosition = Game1.player.Position;
                playerPosition.Y += (Game1.player.FacingDirection == 2 ? DynamicReflections.waterReflectionYOffset : 1.5f) * 64;
                DynamicReflections.waterReflectionPosition = playerPosition;
                DynamicReflections.waterReflectionTilePosition = playerPosition / 64f;

                // Hide the reflection if it will show up out of bounds on the map or not drawn on water tile
                var waterReflectionPosition = DynamicReflections.waterReflectionTilePosition.Value;
                for (int yOffset = 0; yOffset <= Math.Ceiling(DynamicReflections.waterReflectionYOffset); yOffset++)
                {
                    var tilePosition = waterReflectionPosition + new Vector2(0, yOffset);
                    if (IsWaterReflectiveTile(Game1.currentLocation, (int)tilePosition.X, (int)tilePosition.Y) is true)
                    {
                        DynamicReflections.shouldDrawWaterReflection = true;
                    }
                }

                var speed = 1f;
                if (DynamicReflections.isWavyReflection && DynamicReflections.waterReflectionEffect is not null)
                {
                    var phase = waterReflectionEffect.Parameters["Phase"].GetValueSingle();
                    //monitor.Log($"TESTING: {phase}", LogLevel.Debug);
                    if (phase >= 2 * Math.PI)
                    {
                        //phase = -(float)(2 * Math.PI);
                    }
                    else
                    {
                        //phase += (float)(1);
                    }
                    phase += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds * speed;

                    waterReflectionEffect.Parameters["Phase"].SetValue(phase);
                    waterReflectionEffect.Parameters["Frequency"].SetValue(50f);
                    waterReflectionEffect.Parameters["Amplitude"].SetValue(0.01f);
                }
            }

            DynamicReflections.shouldDrawMirrorReflection = false;
            if (DynamicReflections.areMirrorReflectionsEnabled)
            {
                // TODO: Determine the reflection position in relation to the base IsMirrorBase tile
                var playerWorldPosition = Game1.player.Position;
                var playerTilePosition = Game1.player.getTileLocationPoint();

                DynamicReflections.activeMirrorPositions.Clear();
                foreach (var mirror in DynamicReflections.mapMirrors.Values)
                {
                    mirror.IsEnabled = false;

                    // Limit the amount of active Mirrors to the amount of available reflection renders
                    if (activeMirrorPositions.Count >= DynamicReflections.rawPlayerMirrorReflectionRenders.Length)
                    {
                        break;
                    }

                    var mirrorRange = mirror.TilePosition.Y + mirror.Height;
                    if (mirrorRange - 1 <= playerTilePosition.Y && playerTilePosition.Y <= mirrorRange + 1)
                    {
                        mirror.IsEnabled = true;

                        var playerDistanceFromBase = mirror.WorldPosition.Y - playerWorldPosition.Y;
                        var adjustedPosition = new Vector2(playerWorldPosition.X, mirror.WorldPosition.Y + playerDistanceFromBase + 64f);
                        mirror.PlayerReflectionPosition = adjustedPosition;

                        DynamicReflections.shouldDrawMirrorReflection = true;
                        DynamicReflections.activeMirrorPositions.Add(mirror.TilePosition);
                    }
                }

                if (DynamicReflections.mirrorReflectionSprite is null)
                {
                    DynamicReflections.mirrorReflectionSprite = new FarmerSprite(Game1.player.FarmerSprite.textureName.Value);
                }

                if (Game1.player.FacingDirection == 0 && DynamicReflections.mirrorReflectionSprite.PauseForSingleAnimation is false && Game1.player.UsingTool is false)
                {
                    bool isCarrying = Game1.player.IsCarrying();
                    if (Game1.player.isMoving())
                    {
                        if (Game1.player.running && !isCarrying)
                        {
                            DynamicReflections.mirrorReflectionSprite.animate(32, Game1.currentGameTime);
                        }
                        else if (Game1.player.running)
                        {
                            DynamicReflections.mirrorReflectionSprite.animate(128, Game1.currentGameTime);
                        }
                        else if (isCarrying)
                        {
                            DynamicReflections.mirrorReflectionSprite.animate(96, Game1.currentGameTime);
                        }
                        else
                        {
                            DynamicReflections.mirrorReflectionSprite.animate(0, Game1.currentGameTime);
                        }
                    }
                    else if (Game1.player.IsCarrying())
                    {
                        DynamicReflections.mirrorReflectionSprite.setCurrentFrame(128);
                    }
                    else
                    {
                        DynamicReflections.mirrorReflectionSprite.setCurrentFrame(32);
                    }
                }
            }
        }

        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // Compile via the command: mgfxc wavy.fx wavy.mgfx
            opacityEffect = new Effect(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(modHelper.DirectoryPath, "Framework", "Assets", "opacity.mgfx")));
            mirrorReflectionEffect = new Effect(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(modHelper.DirectoryPath, "Framework", "Assets", "mask.mgfx")));

            waterReflectionEffect = new Effect(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(modHelper.DirectoryPath, "Framework", "Assets", "wavy.mgfx")));
            waterReflectionEffect.CurrentTechnique = waterReflectionEffect.Techniques["Wavy"];

            //effect = modHelper.ModContent.Load<Effect>(Path.Combine("Framework", "Assets", "wavy.xnb"));
            //monitor.Log($"TESTING: {effect is null}", LogLevel.Debug);

            // Create the RenderTarget2D and RasterizerState for use by the water reflection
            playerWaterReflectionRender = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            mirrorsRenderTarget = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            rawPlayerMirrorReflectionRenders = new RenderTarget2D[3];
            for (int i = 0; i < 3; i++)
            {
                rawPlayerMirrorReflectionRenders[i] = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);
            }

            modifiedPlayerMirrorReflectionRenders = new RenderTarget2D[3];
            for (int i = 0; i < 3; i++)
            {
                modifiedPlayerMirrorReflectionRenders[i] = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);
            }

            rasterizer = new RasterizerState();
            rasterizer.CullMode = CullMode.CullClockwiseFace;
        }

        private bool IsMirrorBaseTile(GameLocation location, int x, int y, bool requireEnabled = false)
        {
            string isMirrorProperty = location.doesTileHavePropertyNoNull(x, y, "IsMirrorBase", "Mirrors");
            if (String.IsNullOrEmpty(isMirrorProperty))
            {
                return false;
            }

            if (requireEnabled is true && isMirrorProperty.Equals("T", StringComparison.OrdinalIgnoreCase) is false && isMirrorProperty.Equals("True", StringComparison.OrdinalIgnoreCase) is false)
            {
                return false;
            }

            return true;
        }

        private int GetMirrorHeight(GameLocation location, int x, int y)
        {
            string mirrorHeightProperty = location.doesTileHavePropertyNoNull(x, y, "MirrorHeight", "Mirrors");
            if (String.IsNullOrEmpty(mirrorHeightProperty))
            {
                return -1;
            }

            if (Int32.TryParse(mirrorHeightProperty, out int mirrorHeightValue) is false)
            {
                return 1;
            }

            return mirrorHeightValue;
        }

        private int GetMirrorWidth(GameLocation location, int x, int y)
        {
            string mirrorWidthProperty = location.doesTileHavePropertyNoNull(x, y, "MirrorWidth", "Mirrors");
            if (String.IsNullOrEmpty(mirrorWidthProperty))
            {
                return -1;
            }

            if (Int32.TryParse(mirrorWidthProperty, out int mirrorWidthValue) is false)
            {
                return 1;
            }

            return mirrorWidthValue;
        }

        private float GetMirrorScale(GameLocation location, int x, int y)
        {
            string mirrorScaleProperty = location.doesTileHavePropertyNoNull(x, y, "MirrorScale", "Mirrors");
            if (String.IsNullOrEmpty(mirrorScaleProperty))
            {
                return 1f;
            }

            if (float.TryParse(mirrorScaleProperty, out float mirrorScaleValue) is false)
            {
                return 1f;
            }

            return mirrorScaleValue;
        }

        private float GetMirrorOpacity(GameLocation location, int x, int y)
        {
            string mirrorOpacityProperty = location.doesTileHavePropertyNoNull(x, y, "MirrorOpacity", "Mirrors");
            if (String.IsNullOrEmpty(mirrorOpacityProperty))
            {
                return 1f;
            }

            if (float.TryParse(mirrorOpacityProperty, out float mirrorOpacityValue) is false)
            {
                return 1f;
            }

            return mirrorOpacityValue;
        }

        private Vector2 GetMirrorOffset(GameLocation location, int x, int y)
        {
            string mirrorOffsetProperty = location.doesTileHavePropertyNoNull(x, y, "MirrorOffset", "Mirrors");
            if (String.IsNullOrEmpty(mirrorOffsetProperty))
            {
                return Vector2.Zero;
            }

            if (mirrorOffsetProperty.Contains(" ") is false)
            {
                return Vector2.Zero;
            }

            var xOffsetText = mirrorOffsetProperty.Split(" ")[0];
            var yOffsetText = mirrorOffsetProperty.Split(" ")[1];
            if (float.TryParse(xOffsetText, out float xOffsetValue) is false || float.TryParse(yOffsetText, out float yOffsetValue) is false)
            {
                return Vector2.Zero;
            }

            return new Vector2(xOffsetValue, yOffsetValue);
        }

        private bool IsWaterReflectiveTile(GameLocation location, int x, int y)
        {
            return location.isWaterTile(x, y);
        }

        internal static int GetReflectedDirection(int initialDirection, bool isMirror = false)
        {
            if (initialDirection == 0)
            {
                return isMirror is true ? 2 : 0;
            }
            else if (initialDirection == 2)
            {
                return isMirror is true ? 0 : 2;
            }

            return initialDirection;
        }
    }
}
