using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using DynamicReflections.Framework.Models;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using DynamicReflections.Framework.Patches.Core;
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

        internal static Effect effect;
        internal static RenderTarget2D renderTarget;


        // TODO: Implement these map / tile properties
        // Note: These water reflection map properties override the player's config for the current map (if set)
        // Map property - WaterReflectionsEnabled - T or F
        // Map property - WaterReflectionDirection - 0 or 2
        // Map property - WaterReflectionOpacity - 0.0 to 1.0
        // Map property - WaterReflectionIsWavy - T or F
        // Map property - WaterReflectionWavySpeed - Any float
        // Map property - WaterReflectionWavyAmplitude - Any float
        // Map property - WaterReflectionWavyFrequency - Any float

        // Tile property - IsMirror - T or F
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
            if (renderTarget is not null)
            {
                renderTarget.Dispose();
            }

            renderTarget = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);
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

            // Check current map for tiles with IsMirror
            var map = currentLocation.Map;
            if (map is null || (map.GetLayer("Back") is var backLayer && backLayer is null))
            {
                return;
            }

            for (int x = 0; x < backLayer.LayerWidth; x++)
            {
                for (int y = 0; y < backLayer.LayerHeight; y++)
                {
                    if (IsMirrorTile(currentLocation, x, y, true) is false)
                    {
                        continue;
                    }


                    // Check to see if another IsMirror exists directly below it
                    int actualBaseY;
                    for (actualBaseY = y; actualBaseY < backLayer.LayerHeight; actualBaseY++)
                    {
                        if (IsMirrorTile(currentLocation, x, actualBaseY, true) is false)
                        {
                            actualBaseY -= 1;
                            break;
                        }
                    }

                    var point = new Point(x, actualBaseY);
                    if (DynamicReflections.mapMirrors.ContainsKey(point) is false)
                    {
                        DynamicReflections.mapMirrors[point] = new Mirror() { TilePosition = point, Height = Math.Max(1, actualBaseY - y) };
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
                if (DynamicReflections.isWavyReflection && DynamicReflections.effect is not null)
                {
                    var phase = effect.Parameters["Phase"].GetValueSingle();
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

                    effect.Parameters["Phase"].SetValue(phase);
                    effect.Parameters["Frequency"].SetValue(50f);
                    effect.Parameters["Amplitude"].SetValue(0.01f);
                }
            }

            DynamicReflections.shouldDrawMirrorReflection = false;
            if (DynamicReflections.areMirrorReflectionsEnabled)
            {
                // TODO: Determine the reflection position in relation to the base IsMirror tile
                var playerWorldPosition = Game1.player.Position;
                var playerTilePosition = Game1.player.getTileLocationPoint();

                DynamicReflections.activeMirrorPositions.Clear();
                foreach (var mirror in DynamicReflections.mapMirrors.Values)
                {
                    mirror.IsEnabled = false;

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
            effect = new Effect(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(modHelper.DirectoryPath, "Framework", "Assets", "wavy.mgfx")));
            effect.CurrentTechnique = effect.Techniques["Wavy"];

            //effect = modHelper.ModContent.Load<Effect>(Path.Combine("Framework", "Assets", "wavy.xnb"));
            //monitor.Log($"TESTING: {effect is null}", LogLevel.Debug);

            renderTarget = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);
        }

        private bool IsMirrorTile(GameLocation location, int x, int y, bool requireEnabled = false)
        {
            string isMirrorProperty = location.doesTileHavePropertyNoNull(x, y, "IsMirror", "Back");
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
