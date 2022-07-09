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
using System.Linq;
using DynamicReflections.Framework.Patches.Objects;
using DynamicReflections.Framework.Utilities;
using DynamicReflections.Framework.Managers;
using DynamicReflections.Framework.Models.Settings;
using System.Text.Json;
using DynamicReflections.Framework.External.GenericModConfigMenu;

namespace DynamicReflections
{
    public class DynamicReflections : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        // Managers
        internal static ApiManager apiManager;
        internal static MirrorsManager mirrorsManager;

        // Config options
        internal static ModConfig modConfig;
        internal static WaterSettings currentWaterSettings;

        // Water reflection variables
        internal static Vector2? waterReflectionPosition;
        internal static Vector2? waterReflectionTilePosition;
        internal static bool shouldDrawWaterReflection;
        internal static bool isDrawingWaterReflection;
        internal static bool isFilteringWater;

        // Mirror reflection variables
        internal static FarmerSprite mirrorReflectionSprite;
        internal static Dictionary<Point, Mirror> mirrors = new Dictionary<Point, Mirror>();
        internal static List<Point> activeMirrorPositions = new List<Point>();
        internal static bool shouldDrawMirrorReflection;
        internal static bool isDrawingMirrorReflection;
        internal static bool isFilteringMirror;

        // Effects and RenderTarget2Ds
        internal static Effect waterReflectionEffect;
        internal static Effect mirrorReflectionEffect;
        internal static RenderTarget2D playerWaterReflectionRender;
        internal static RenderTarget2D[] composedPlayerMirrorReflectionRenders;
        internal static RenderTarget2D[] maskedPlayerMirrorReflectionRenders;
        internal static RenderTarget2D inBetweenRenderTarget;
        internal static RenderTarget2D mirrorsLayerRenderTarget;
        internal static RenderTarget2D mirrorsFurnitureRenderTarget;
        internal static RasterizerState rasterizer;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Load the managers
            apiManager = new ApiManager(monitor);
            mirrorsManager = new MirrorsManager();

            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply patches
                new LayerPatch(monitor, modHelper).Apply(harmony);
                new DisplayDevicePatch(monitor, modHelper).Apply(harmony);
                new ToolPatch(monitor, modHelper).Apply(harmony);
                new FurniturePatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in the debug commands
            helper.ConsoleCommands.Add("dr_reload", "Reloads all Dynamic Reflections content packs.\n\nUsage: dr_reload", delegate { this.LoadContentPacks(); this.DetectMirrorsForActiveLocation(); });

            // Hook into the required events
            helper.Events.Display.WindowResized += OnWindowResized;
            helper.Events.World.FurnitureListChanged += OnFurnitureListChanged;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnWindowResized(object sender, StardewModdingAPI.Events.WindowResizedEventArgs e)
        {
            LoadRenderers();
        }

        private void OnFurnitureListChanged(object sender, StardewModdingAPI.Events.FurnitureListChangedEventArgs e)
        {
            if (e.IsCurrentLocation is false)
            {
                return;
            }

            // Attempt to add any DGA mirrors
            foreach (var furniture in e.Added)
            {
                if (DynamicReflections.mirrorsManager.GetSettings(furniture.Name) is MirrorSettings baseSettings && baseSettings is not null)
                {
                    var point = new Point((int)furniture.TileLocation.X, (int)furniture.TileLocation.Y);
                    var settings = new MirrorSettings()
                    {
                        Dimensions = new Rectangle(baseSettings.Dimensions.X, baseSettings.Dimensions.Y, baseSettings.Dimensions.Width, baseSettings.Dimensions.Height),
                        ReflectionOffset = baseSettings.ReflectionOffset,
                        ReflectionOverlay = baseSettings.ReflectionOverlay,
                        ReflectionScale = baseSettings.ReflectionScale
                    };

                    DynamicReflections.mirrors[point] = new Mirror()
                    {
                        FurnitureLink = furniture,
                        TilePosition = point,
                        Settings = settings
                    };
                }
            }

            // Attempt to remove any DGA mirrors
            foreach (var furniture in e.Removed)
            {
                if (DynamicReflections.mirrorsManager.GetSettings(furniture.Name) is MirrorSettings baseSettings && baseSettings is not null)
                {
                    var point = new Point((int)furniture.TileLocation.X, (int)furniture.TileLocation.Y);
                    foreach (var mirrorPosition in DynamicReflections.mirrors.Keys.ToList())
                    {
                        if (DynamicReflections.mirrors[mirrorPosition].FurnitureLink is not null && mirrorPosition.X == point.X && mirrorPosition.Y == point.Y)
                        {
                            DynamicReflections.mirrors.Remove(mirrorPosition);
                        }
                    }
                }
            }
        }

        private void OnWarped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            SetWaterReflectionSettings();
            DetectMirrorsForActiveLocation();
        }

        private void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady is false)
            {
                return;
            }

            // Handle mirror-viewport issue
            if (Game1.getMostRecentViewportMotion() != Vector2.Zero && DynamicReflections.activeMirrorPositions.Count > 0)
            {
                // TODO: Determine why maskedPlayerMirrorReflectionRenders aren't updating when the viewport moves
                for (int i = 0; i < DynamicReflections.activeMirrorPositions.Count; i++)
                {
                    var position = DynamicReflections.activeMirrorPositions[i];
                    if (DynamicReflections.mirrors.ContainsKey(position) && DynamicReflections.mirrors[position].FurnitureLink is not null)
                    {
                        if (maskedPlayerMirrorReflectionRenders[i] is not null)
                        {
                            maskedPlayerMirrorReflectionRenders[i].Dispose();
                        }

                        maskedPlayerMirrorReflectionRenders[i] = new RenderTarget2D(
                        Game1.graphics.GraphicsDevice,
                        Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                        Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                        false,
                        Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                        DepthFormat.None);
                    }
                }
            }

            // Handle the water reflection
            DynamicReflections.shouldDrawWaterReflection = false;
            if (DynamicReflections.modConfig.AreWaterReflectionsEnabled && currentWaterSettings is not null)
            {
                var positionInverter = currentWaterSettings.ReflectionDirection == Direction.North && currentWaterSettings.ReflectionOffset.Y > 0 ? -1 : 1;
                var playerPosition = Game1.player.Position;
                playerPosition += currentWaterSettings.ReflectionOffset * 64 * positionInverter;
                DynamicReflections.waterReflectionPosition = playerPosition;
                DynamicReflections.waterReflectionTilePosition = playerPosition / 64f;

                // Hide the reflection if it will show up out of bounds on the map or not drawn on water tile
                var waterReflectionPosition = DynamicReflections.waterReflectionTilePosition.Value;
                for (int yOffset = -1; yOffset <= Math.Ceiling(currentWaterSettings.ReflectionOffset.Y); yOffset++)
                {
                    var tilePosition = waterReflectionPosition + new Vector2(0, yOffset);
                    if (IsWaterReflectiveTile(Game1.currentLocation, (int)tilePosition.X - 1, (int)tilePosition.Y) is true || IsWaterReflectiveTile(Game1.currentLocation, (int)tilePosition.X, (int)tilePosition.Y) is true || IsWaterReflectiveTile(Game1.currentLocation, (int)tilePosition.X + 1, (int)tilePosition.Y) is true)
                    {
                        DynamicReflections.shouldDrawWaterReflection = true;
                        break;
                    }
                }

                // Handle the wavy effect if enabled
                if (currentWaterSettings.IsReflectionWavy && DynamicReflections.waterReflectionEffect is not null)
                {
                    var phase = waterReflectionEffect.Parameters["Phase"].GetValueSingle();
                    phase += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds * currentWaterSettings.WaveSpeed;

                    waterReflectionEffect.Parameters["Phase"].SetValue(phase);
                    waterReflectionEffect.Parameters["Frequency"].SetValue(currentWaterSettings.WaveFrequency);
                    waterReflectionEffect.Parameters["Amplitude"].SetValue(currentWaterSettings.WaveAmplitude);
                }
            }

            // Handle the mirror reflections
            DynamicReflections.shouldDrawMirrorReflection = false;
            if (DynamicReflections.modConfig.AreMirrorReflectionsEnabled)
            {
                var playerWorldPosition = Game1.player.Position;
                var playerTilePosition = Game1.player.getTileLocationPoint();

                DynamicReflections.activeMirrorPositions.Clear();
                foreach (var mirror in DynamicReflections.mirrors.Values.OrderByDescending(m => m.TilePosition.Y))
                {
                    mirror.IsEnabled = false;

                    // Limit the amount of active Mirrors to the amount of available reflection renders
                    if (activeMirrorPositions.Count >= DynamicReflections.maskedPlayerMirrorReflectionRenders.Length)
                    {
                        break;
                    }

                    var mirrorWidth = mirror.TilePosition.X + (mirror.FurnitureLink is not null ? (int)Math.Ceiling(mirror.Settings.Dimensions.Width / 16f) : mirror.Settings.Dimensions.Width);
                    if (mirror.TilePosition.X - 1 <= playerTilePosition.X && playerTilePosition.X <= mirrorWidth)
                    {
                        var mirrorRange = mirror.TilePosition.Y + (mirror.FurnitureLink is not null ? (int)Math.Ceiling(mirror.Settings.Dimensions.Height / 16f) : mirror.Settings.Dimensions.Height);
                        if (mirror.TilePosition.Y < playerTilePosition.Y && playerTilePosition.Y <= mirrorRange)
                        {
                            // Skip any mirrors that are within range of an already active mirror
                            if (IsTileWithinActiveMirror(mirrorRange))
                            {
                                continue;
                            }

                            mirror.IsEnabled = true;
                            mirror.ActiveIndex = DynamicReflections.activeMirrorPositions.Count;

                            var playerDistanceFromBase = mirror.WorldPosition.Y - playerWorldPosition.Y;
                            var adjustedPosition = new Vector2(playerWorldPosition.X, mirror.WorldPosition.Y + playerDistanceFromBase + 64f);
                            mirror.PlayerReflectionPosition = adjustedPosition;

                            DynamicReflections.shouldDrawMirrorReflection = true;
                            DynamicReflections.activeMirrorPositions.Add(mirror.TilePosition);
                        }
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

        private void OnDayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            SetWaterReflectionSettings();
            DetectMirrorsForActiveLocation();
        }

        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // Load any owned content packs
            LoadContentPacks();

            // Set our default configuration file
            modConfig = Helper.ReadConfig<ModConfig>();

            // Hook into GMCM, if applicable
            if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu") && apiManager.HookIntoGenericModConfigMenu(Helper))
            {
                var configApi = apiManager.GetGenericModConfigMenuApi();
                configApi.Register(ModManifest, () => modConfig = new ModConfig(), delegate { Helper.WriteConfig(modConfig); SetWaterReflectionSettings(); LoadRenderers(); });

                // Register the standard settings
                configApi.RegisterLabel(ModManifest, $"General Settings", String.Empty);
                configApi.AddBoolOption(ModManifest, () => modConfig.AreMirrorReflectionsEnabled, value => modConfig.AreMirrorReflectionsEnabled = value, () => "Enable Mirror Reflections");
                configApi.AddBoolOption(ModManifest, () => modConfig.AreWaterReflectionsEnabled, value => modConfig.AreWaterReflectionsEnabled = value, () => "Enable Water Reflections");

                configApi.RegisterLabel(ModManifest, $"Water Specific Settings", String.Empty);
                configApi.AddNumberOption(ModManifest, () => (int)modConfig.WaterReflectionSettings.ReflectionDirection, value => modConfig.WaterReflectionSettings.ReflectionDirection = (Direction)value, () => "Reflection Direction", tooltip: () => "0 for North reflections, 2 for South reflections.", min: 0, max: 2, interval: 2);
                configApi.AddNumberOption(ModManifest, () => modConfig.WaterReflectionSettings.ReflectionOverlay.R, value => modConfig.WaterReflectionSettings.ReflectionOverlay = new Color(value, modConfig.WaterReflectionSettings.ReflectionOverlay.G, modConfig.WaterReflectionSettings.ReflectionOverlay.B, modConfig.WaterReflectionSettings.ReflectionOverlay.A), () => "Reflection Overlay Color (R)", min: 0, max: 255, interval: 1);
                configApi.AddNumberOption(ModManifest, () => modConfig.WaterReflectionSettings.ReflectionOverlay.G, value => modConfig.WaterReflectionSettings.ReflectionOverlay = new Color(modConfig.WaterReflectionSettings.ReflectionOverlay.R, value, modConfig.WaterReflectionSettings.ReflectionOverlay.B, modConfig.WaterReflectionSettings.ReflectionOverlay.A), () => "Reflection Overlay Color (G)", min: 0, max: 255, interval: 1);
                configApi.AddNumberOption(ModManifest, () => modConfig.WaterReflectionSettings.ReflectionOverlay.B, value => modConfig.WaterReflectionSettings.ReflectionOverlay = new Color(modConfig.WaterReflectionSettings.ReflectionOverlay.R, modConfig.WaterReflectionSettings.ReflectionOverlay.G, value, modConfig.WaterReflectionSettings.ReflectionOverlay.A), () => "Reflection Overlay Color (B)", min: 0, max: 255, interval: 1);
                configApi.AddNumberOption(ModManifest, () => modConfig.WaterReflectionSettings.ReflectionOverlay.A, value => modConfig.WaterReflectionSettings.ReflectionOverlay = new Color(modConfig.WaterReflectionSettings.ReflectionOverlay.R, modConfig.WaterReflectionSettings.ReflectionOverlay.G, modConfig.WaterReflectionSettings.ReflectionOverlay.B, value), () => "Reflection Overlay Color (A)", min: 0, max: 255, interval: 1);
                configApi.AddNumberOption(ModManifest, () => modConfig.WaterReflectionSettings.ReflectionOffset.X, value => modConfig.WaterReflectionSettings.ReflectionOffset = new Vector2(value, modConfig.WaterReflectionSettings.ReflectionOffset.Y), () => "Reflection X Offset", interval: 0.1f);
                configApi.AddNumberOption(ModManifest, () => modConfig.WaterReflectionSettings.ReflectionOffset.Y, value => modConfig.WaterReflectionSettings.ReflectionOffset = new Vector2(modConfig.WaterReflectionSettings.ReflectionOffset.X, value), () => "Reflection Y Offset", interval: 0.1f);
                configApi.AddBoolOption(ModManifest, () => modConfig.WaterReflectionSettings.IsReflectionWavy, value => modConfig.WaterReflectionSettings.IsReflectionWavy = value, () => "Enable Wavy Reflections");
                configApi.AddNumberOption(ModManifest, () => modConfig.WaterReflectionSettings.WaveSpeed, value => modConfig.WaterReflectionSettings.WaveSpeed = value, () => "Reflection Wave Speed");
                configApi.AddNumberOption(ModManifest, () => modConfig.WaterReflectionSettings.WaveAmplitude, value => modConfig.WaterReflectionSettings.WaveAmplitude = value, () => "Reflection Wave Amplitude");
                configApi.AddNumberOption(ModManifest, () => modConfig.WaterReflectionSettings.WaveFrequency, value => modConfig.WaterReflectionSettings.WaveFrequency = value, () => "Reflection Wave Frequency");
            }

            // Load in our shaders
            // Compile via the command: mgfxc wavy.fx wavy.mgfx
            // Unused: opacityEffect = new Effect(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(modHelper.DirectoryPath, "Framework", "Assets", "opacity.mgfx")));
            mirrorReflectionEffect = new Effect(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(modHelper.DirectoryPath, "Framework", "Assets", "mask.mgfx")));

            waterReflectionEffect = new Effect(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(modHelper.DirectoryPath, "Framework", "Assets", "wavy.mgfx")));
            waterReflectionEffect.CurrentTechnique = waterReflectionEffect.Techniques["Wavy"];

            // Create the RenderTarget2D and RasterizerState for use by the water reflection
            LoadRenderers();
        }

        private void LoadContentPacks(bool silent = false)
        {
            // Clear the existing cache of custom buildings
            mirrorsManager.Reset();

            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    Monitor.Log($"Loading data from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", silent ? LogLevel.Trace : LogLevel.Debug);

                    // Load mirrors
                    if (!File.Exists(Path.Combine(contentPack.DirectoryPath, "mirrors.json")))
                    {
                        Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a mirrors.json!", LogLevel.Warn);
                        continue;
                    }

                    var mirrorModels = contentPack.ReadJsonFile<List<ContentPackModel>>("mirrors.json");
                    if (mirrorModels is null || mirrorModels.Count == 0)
                    {
                        Monitor.Log($"Content pack {contentPack.Manifest.Name} has an empty or invalid mirrors.json!", LogLevel.Warn);
                        continue;
                    }

                    // Verify that the mask texture exists under the content pack's Masks folder
                    foreach (var mirrorModel in mirrorModels)
                    {
                        if (String.IsNullOrEmpty(mirrorModel.MaskTexture) || !File.Exists(Path.Combine(contentPack.DirectoryPath, "Masks", mirrorModel.MaskTexture)))
                        {
                            Monitor.Log($"The mirror for {mirrorModel.FurnitureId} within content pack {contentPack.Manifest.Name} is missing its mask texture!", LogLevel.Warn);
                            continue;
                        }

                        mirrorModel.Mask = contentPack.ModContent.Load<Texture2D>(Path.Combine("Masks", mirrorModel.MaskTexture));
                    }

                    mirrorsManager.Add(mirrorModels);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to load the content pack {contentPack.Manifest.UniqueID}: {ex}", LogLevel.Warn);
                }
            }
        }

        private void SetWaterReflectionSettings()
        {
            if (currentWaterSettings is null)
            {
                currentWaterSettings = new WaterSettings();
            }
            currentWaterSettings.Reset(modConfig.WaterReflectionSettings);

            if (Context.IsWorldReady is false || Game1.currentLocation is null || Game1.currentLocation.Map is null)
            {
                return;
            }
            var map = Game1.currentLocation.Map;

            // Set the map specific water settings
            if (map.Properties.ContainsKey(WaterSettings.MapProperty_IsEnabled))
            {
                currentWaterSettings.IsEnabled = map.Properties[WaterSettings.MapProperty_IsEnabled].ToString().Equals("T", StringComparison.OrdinalIgnoreCase);
            }

            if (map.Properties.ContainsKey(WaterSettings.MapProperty_ReflectionDirection))
            {
                if (Enum.TryParse<Direction>(map.Properties[WaterSettings.MapProperty_ReflectionDirection], out var direction))
                {
                    currentWaterSettings.ReflectionDirection = direction;
                }
            }

            if (map.Properties.ContainsKey(WaterSettings.MapProperty_ReflectionOverlay))
            {
                try
                {
                    if (JsonSerializer.Deserialize<Color>(map.Properties[WaterSettings.MapProperty_ReflectionOverlay]) is Color overlay)
                    {
                        currentWaterSettings.ReflectionOverlay = overlay;
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to get WaterSettings.MapProperty_ReflectionOverlay from the map {map.Id}!", LogLevel.Warn);
                    Monitor.Log($"Failed to get WaterSettings.MapProperty_ReflectionOverlay from the map {map.Id}: {ex}", LogLevel.Trace);
                }
            }

            if (map.Properties.ContainsKey(WaterSettings.MapProperty_ReflectionOffset))
            {
                try
                {
                    if (JsonSerializer.Deserialize<Vector2>(map.Properties[WaterSettings.MapProperty_ReflectionOffset]) is Vector2 offset)
                    {
                        currentWaterSettings.ReflectionOffset = offset;
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to get WaterSettings.MapProperty_ReflectionOffset from the map {map.Id}!", LogLevel.Warn);
                    Monitor.Log($"Failed to get WaterSettings.MapProperty_ReflectionOffset from the map {map.Id}: {ex}", LogLevel.Trace);
                }
            }

            if (map.Properties.ContainsKey(WaterSettings.MapProperty_IsReflectionWavy))
            {
                currentWaterSettings.IsReflectionWavy = map.Properties[WaterSettings.MapProperty_IsReflectionWavy].ToString().Equals("T", StringComparison.OrdinalIgnoreCase);
            }

            if (map.Properties.ContainsKey(WaterSettings.MapProperty_WaveSpeed))
            {
                if (float.TryParse(map.Properties[WaterSettings.MapProperty_WaveSpeed], out var waveSpeed))
                {
                    currentWaterSettings.WaveSpeed = waveSpeed;
                }
            }

            if (map.Properties.ContainsKey(WaterSettings.MapProperty_WaveAmplitude))
            {
                if (float.TryParse(map.Properties[WaterSettings.MapProperty_WaveAmplitude], out var waveAmplitude))
                {
                    currentWaterSettings.WaveAmplitude = waveAmplitude;
                }
            }

            if (map.Properties.ContainsKey(WaterSettings.MapProperty_WaveFrequency))
            {
                if (float.TryParse(map.Properties[WaterSettings.MapProperty_WaveFrequency], out var waveFrequency))
                {
                    currentWaterSettings.WaveFrequency = waveFrequency;
                }
            }
        }

        private void DetectMirrorsForActiveLocation()
        {
            if (Context.IsWorldReady is false || Game1.currentLocation is null)
            {
                return;
            }
            var currentLocation = Game1.currentLocation;

            // Clear the old base points out
            DynamicReflections.mirrors.Clear();

            // Check current map for tiles with IsMirrorBase
            var map = currentLocation.Map;
            if (map is not null && (map.GetLayer("Mirrors") is var mirrorLayer && mirrorLayer is not null))
            {
                for (int x = 0; x < mirrorLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < mirrorLayer.LayerHeight; y++)
                    {
                        if (IsMirrorBaseTile(currentLocation, x, y, true) is false)
                        {
                            continue;
                        }

                        var point = new Point(x, y);
                        if (DynamicReflections.mirrors.ContainsKey(point) is false)
                        {
                            var settings = new MirrorSettings()
                            {
                                Dimensions = new Rectangle(0, 0, GetMirrorWidth(currentLocation, x, y), GetMirrorHeight(currentLocation, x, y) - 1),
                                ReflectionOffset = GetMirrorOffset(currentLocation, x, y),
                                ReflectionOverlay = GetMirrorOverlay(currentLocation, x, y),
                                ReflectionScale = GetMirrorScale(currentLocation, x, y)
                            };

                            DynamicReflections.mirrors[point] = new Mirror()
                            {
                                TilePosition = point,
                                Settings = settings
                            };
                        }
                    }
                }
            }

            // Find all mirror furniture
            foreach (var furniture in currentLocation.furniture)
            {
                if (DynamicReflections.mirrorsManager.GetSettings(furniture.Name) is MirrorSettings baseSettings && baseSettings is not null)
                {
                    var point = new Point((int)furniture.TileLocation.X, (int)furniture.TileLocation.Y);
                    var settings = new MirrorSettings()
                    {
                        Dimensions = new Rectangle(baseSettings.Dimensions.X, baseSettings.Dimensions.Y, baseSettings.Dimensions.Width, baseSettings.Dimensions.Height),
                        ReflectionOffset = baseSettings.ReflectionOffset,
                        ReflectionOverlay = baseSettings.ReflectionOverlay,
                        ReflectionScale = baseSettings.ReflectionScale
                    };

                    DynamicReflections.mirrors.Add(point, new Mirror()
                    {
                        FurnitureLink = furniture,
                        TilePosition = point,
                        Settings = settings
                    });
                }
            }
        }

        private void LoadRenderers()
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

            if (mirrorsLayerRenderTarget is not null)
            {
                mirrorsLayerRenderTarget.Dispose();
            }

            mirrorsLayerRenderTarget = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            if (mirrorsFurnitureRenderTarget is not null)
            {
                mirrorsFurnitureRenderTarget.Dispose();
            }

            mirrorsFurnitureRenderTarget = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            if (maskedPlayerMirrorReflectionRenders is not null)
            {
                foreach (var mirrorPlayerRender in maskedPlayerMirrorReflectionRenders)
                {
                    if (mirrorPlayerRender is not null)
                    {
                        mirrorPlayerRender.Dispose();
                    }
                }
            }
            maskedPlayerMirrorReflectionRenders = new RenderTarget2D[3];
            for (int i = 0; i < 3; i++)
            {
                maskedPlayerMirrorReflectionRenders[i] = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);
            }

            if (composedPlayerMirrorReflectionRenders is not null)
            {
                foreach (var mirrorPlayerRender in composedPlayerMirrorReflectionRenders)
                {
                    if (mirrorPlayerRender is not null)
                    {
                        mirrorPlayerRender.Dispose();
                    }
                }
            }
            composedPlayerMirrorReflectionRenders = new RenderTarget2D[3];
            for (int i = 0; i < 3; i++)
            {
                composedPlayerMirrorReflectionRenders[i] = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);
            }

            if (inBetweenRenderTarget is not null)
            {
                inBetweenRenderTarget.Dispose();
            }

            inBetweenRenderTarget = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            if (rasterizer is not null)
            {
                rasterizer.Dispose();
            }

            rasterizer = new RasterizerState();
            rasterizer.CullMode = CullMode.CullClockwiseFace;
        }

        private bool IsTileWithinActiveMirror(int tileY)
        {
            foreach (var activePosition in DynamicReflections.activeMirrorPositions)
            {
                if (tileY - 1 <= activePosition.Y && activePosition.Y <= tileY + 1)
                {
                    return true;
                }
            }

            return false;
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
                return 1;
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
                return 1;
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

        private Color GetMirrorOverlay(GameLocation location, int x, int y)
        {
            string mirrorOverlayProperty = location.doesTileHavePropertyNoNull(x, y, "MirrorOverlay", "Mirrors");
            if (String.IsNullOrEmpty(mirrorOverlayProperty) || mirrorOverlayProperty.Split(' ').Length < 3)
            {
                return Color.White;
            }
            var splitColorValues = mirrorOverlayProperty.Split(' ');

            for (int i = 0; i < 3; i++)
            {
                if (Int32.TryParse(splitColorValues[i], out int _) is false)
                {
                    return Color.White;
                }
            }

            return new Color(Int32.Parse(splitColorValues[0]), Int32.Parse(splitColorValues[1]), Int32.Parse(splitColorValues[2]), Int32.Parse(splitColorValues[3]));
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
