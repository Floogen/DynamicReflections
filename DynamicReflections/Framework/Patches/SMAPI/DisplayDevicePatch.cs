using DynamicReflections.Framework.Managers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace DynamicReflections.Framework.Patches.SMAPI
{
    internal class DisplayDevicePatch : PatchTemplate
    {
        internal DisplayDevicePatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method("StardewModdingAPI.Framework.Rendering.SDisplayDevice:DrawTile", new[] { typeof(Tile), typeof(xTile.Dimensions.Location), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawTilePrefix)));
            harmony.Patch(AccessTools.Method("StardewModdingAPI.Framework.Rendering.SXnaDisplayDevice:DrawTile", new[] { typeof(Tile), typeof(xTile.Dimensions.Location), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawTilePrefix)));

            // Perform PyTK related patches
            if (DynamicReflections.modHelper.ModRegistry.IsLoaded("Platonymous.Toolkit"))
            {
                try
                {
                    if (Type.GetType("PyTK.Types.PyDisplayDevice, PyTK") is Type PyTK && PyTK != null)
                    {
                        harmony.Patch(AccessTools.Method(PyTK, "DrawTile", new[] { typeof(Tile), typeof(Location), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawTilePrefix)));
                    }
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Failed to patch PyTK in {this.GetType().Name}: DR may not properly display reflections!", LogLevel.Warn);
                    _monitor.Log($"Patch for PyTK failed in {this.GetType().Name}: {ex}", LogLevel.Trace);
                }
            }
        }

        private static bool DrawTilePrefix(IDisplayDevice __instance, SpriteBatch ___m_spriteBatchAlpha, Color ___m_modulationColour, ref Vector2 ___m_tilePosition, Tile? tile, Location location, float layerDepth)
        {
            if (tile is null || DynamicReflections.currentWaterSettings.IsEnabled is false)
            {
                return true;
            }

            // TODO: Implement puddles by selecting random Diggable tiles on warp, then drawing the mask before drawing the reflection
            // TODO: Move this component to postfix to draw the actual puddle / reflection?
            if (DynamicReflections.isFilteringPuddles is true)
            {
                if (tile.Properties.TryGetValue("PuddleIndex", out var index) && (int)index != PuddleManager.DEFAULT_PUDDLE_INDEX)
                {
                    ___m_tilePosition.X = location.X;
                    ___m_tilePosition.Y = location.Y;
                    Vector2 origin = new Vector2(8f, 8f);
                    ___m_tilePosition.X += origin.X * (float)Layer.zoom;
                    ___m_tilePosition.Y += origin.X * (float)Layer.zoom;
                    ___m_spriteBatchAlpha.Draw(DynamicReflections.assetManager.PuddlesTileSheetTexture, ___m_tilePosition, new Microsoft.Xna.Framework.Rectangle(0, index * 16, 16, 16), ___m_modulationColour, 0f, origin, Layer.zoom, SpriteEffects.None, layerDepth);
                }
                return false;
            }

            if (DynamicReflections.isDrawingWaterReflection is true && tile.TileIndexProperties.TryGetValue("Water", out _) is true)
            {
                return false;
            }
            else if (DynamicReflections.isFilteringWater is true && tile.TileIndexProperties.TryGetValue("Water", out _) is false)
            {
                return false;
            }

            return true;
        }
    }
}
