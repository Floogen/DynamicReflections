using DynamicReflections.Framework.Utilities;
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

namespace DynamicReflections.Framework.Patches.Tiles
{
    internal class LayerPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Layer);

        internal LayerPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Layer.Draw), new[] { typeof(IDisplayDevice), typeof(xTile.Dimensions.Rectangle), typeof(xTile.Dimensions.Location), typeof(bool), typeof(int) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));

            harmony.CreateReversePatcher(AccessTools.Method(_object, nameof(Layer.Draw), new[] { typeof(IDisplayDevice), typeof(xTile.Dimensions.Rectangle), typeof(xTile.Dimensions.Location), typeof(bool), typeof(int) }), new HarmonyMethod(GetType(), nameof(DrawReversePatch))).Patch();
        }

        // Start of the actual patch
        private static bool DrawPrefix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            DynamicReflections.isDrawingWaterReflection = false;
            DynamicReflections.isDrawingMirrorReflection = false;

            if (__instance.Id.Equals("Back", StringComparison.OrdinalIgnoreCase) is true)
            {
                SpriteBatchToolkit.CacheSpriteBatchSettings(Game1.spriteBatch, endSpriteBatch: true);

                // Pre-render the Mirrors layer (this should always be done, regardless of DynamicReflections.shouldDrawMirrorReflection)
                SpriteBatchToolkit.RenderMirrors();
                if (DynamicReflections.shouldDrawMirrorReflection is true)
                {
                    // Pre-render the mirror reflections
                    DynamicReflections.isFilteringMirror = true;
                    SpriteBatchToolkit.RenderMirrorReflectionPlayerSprite();
                    DynamicReflections.isFilteringMirror = false;
                }

                // Handle preliminary water reflection logic
                if (DynamicReflections.shouldDrawWaterReflection is true)
                {
                    DynamicReflections.isFilteringWater = true;

                    if (DynamicReflections.isWavyReflection)
                    {
                        SpriteBatchToolkit.RenderWaterReflectionPlayerSprite();
                    }
                }

                // Resume previous SpriteBatch
                SpriteBatchToolkit.ResumeCachedSpriteBatch(Game1.spriteBatch);

                // Draw the filtered layer, if needed
                if (DynamicReflections.isFilteringWater is false)
                {
                    return true;
                }
                LayerPatch.DrawReversePatch(__instance, displayDevice, mapViewport, displayOffset, wrapAround, pixelZoom);

                // Draw the water reflection
                if (DynamicReflections.isFilteringWater is true)
                {
                    SpriteBatchToolkit.CacheSpriteBatchSettings(Game1.spriteBatch, endSpriteBatch: true);

                    if (DynamicReflections.isWavyReflection)
                    {
                        SpriteBatchToolkit.DrawRenderedPlayer(isWavy: true);
                    }
                    else
                    {
                        SpriteBatchToolkit.DrawReflectionViaMatrix();
                    }

                    DynamicReflections.isFilteringWater = false;
                    DynamicReflections.isDrawingWaterReflection = true;

                    // Resume previous SpriteBatch
                    SpriteBatchToolkit.ResumeCachedSpriteBatch(Game1.spriteBatch);
                }
            }
            else if (__instance.Id.Equals("Buildings", StringComparison.OrdinalIgnoreCase) is true)
            {
                // Draw the cached Mirrors layer
                Game1.spriteBatch.Draw(DynamicReflections.mirrorsRenderTarget, Vector2.Zero, Color.White);

                // Skip drawing the player's reflection if not needed
                if (DynamicReflections.shouldDrawMirrorReflection is true)
                {
                    SpriteBatchToolkit.CacheSpriteBatchSettings(Game1.spriteBatch, endSpriteBatch: true);

                    DynamicReflections.isDrawingMirrorReflection = true;
                    SpriteBatchToolkit.DrawMirrorReflection();

                    // Resume previous SpriteBatch
                    SpriteBatchToolkit.ResumeCachedSpriteBatch(Game1.spriteBatch);
                }
            }

            return true;
        }

        internal static void DrawReversePatch(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            new NotImplementedException("It's a stub!");
        }
    }
}
