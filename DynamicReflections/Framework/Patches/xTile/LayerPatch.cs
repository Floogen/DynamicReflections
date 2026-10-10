using DynamicReflections.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Reflection;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;

namespace DynamicReflections.Framework.Patches.Tiles
{
    internal class LayerPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Layer);
        private static Color _waterColor;

        internal LayerPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            MethodInfo drawNormal = AccessTools.Method(_object, "DrawNormal", new[]
            {
                typeof(IDisplayDevice),
                typeof(xTile.Dimensions.Rectangle),
                typeof(xTile.Dimensions.Location),
                typeof(int),
                typeof(float)
            });

            harmony.Patch(drawNormal, prefix: new HarmonyMethod(GetType(), nameof(DrawNormalPrefix)));
            harmony.Patch(drawNormal, postfix: new HarmonyMethod(GetType(), nameof(DrawNormalPostfix)));
            harmony.CreateReversePatcher(drawNormal, new HarmonyMethod(GetType(), nameof(DrawNormalReversePatch))).Patch();

            PatchPyTkLayerDraw(harmony, "Platonymous.Toolkit", "PyTK.Extensions.PyMaps, PyTK", "PyTK");
            PatchPyTkLayerDraw(harmony, "Platonymous.TMXLoader", "TMXLoader.PyMaps, TMXLoader", "TMXLoader");
        }

        private void PatchPyTkLayerDraw(Harmony harmony, string modId, string typeName, string displayName)
        {
            if (!DynamicReflections.modHelper.ModRegistry.IsLoaded(modId))
            {
                return;
            }

            try
            {
                Type pyTkType = Type.GetType(typeName);
                if (pyTkType is null)
                {
                    return;
                }

                MethodInfo drawLayer = AccessTools.Method(pyTkType, "drawLayer", new[]
                {
                    typeof(Layer),
                    typeof(IDisplayDevice),
                    typeof(xTile.Dimensions.Rectangle),
                    typeof(int),
                    typeof(Location),
                    typeof(bool)
                });

                if (drawLayer is not null)
                {
                    harmony.Patch(drawLayer, prefix: new HarmonyMethod(GetType(), nameof(PyTKDrawLayerPrefix)));
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to patch {displayName} in {GetType().Name}: DR may not properly display reflections!", LogLevel.Warn);
                _monitor.Log($"Patch for {displayName} failed in {GetType().Name}: {ex}", LogLevel.Trace);
            }
        }

        private static bool DrawNormalPrefix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, int pixelZoom, float sort_offset = 0f)
        {
            if (__instance is null || String.IsNullOrEmpty(__instance.Id))
            {
                return true;
            }

            DynamicReflections.isDrawingPuddles = false;
            DynamicReflections.isDrawingWaterReflection = false;
            DynamicReflections.isDrawingMirrorReflection = false;

            var lowestBackgroundLayer = LayerToolkit.GetLowestBackgroundLayer(Game1.currentLocation);
            bool hasMultipleBackgroundLayers = LayerToolkit.HasMultipleBackgroundLayers(Game1.currentLocation);

            if (__instance.Equals(lowestBackgroundLayer) is true)
            {
                DynamicReflections.shouldDeferWaterReflectionPresentation = false;
                DynamicReflections.shouldDeferSkyReflectionPresentation = false;
                SpriteBatchToolkit.CacheSpriteBatchSettings(Game1.spriteBatch, endSpriteBatch: true);

                // Pre-render the Mirrors layer (this should always be done, regardless of DynamicReflections.shouldDrawMirrorReflection)
                SpriteBatchToolkit.RenderMirrorsLayer();
                if (DynamicReflections.shouldDrawMirrorReflection is true)
                {
                    // Pre-render the mirror reflections
                    DynamicReflections.isFilteringMirror = true;
                    SpriteBatchToolkit.RenderMirrorReflectionPlayerSprite();
                    DynamicReflections.isFilteringMirror = false;
                }

                // Handle preliminary water reflection logic
                if (DynamicReflections.modConfig.AreWaterReflectionsEnabled)
                {
                    DynamicReflections.isFilteringWater = true;
                    SpriteBatchToolkit.RenderWaterReflectionPlayerSprite();
                }

                _waterColor = Game1.currentLocation.waterColor.Value;
                if (DynamicReflections.modConfig.AreSkyReflectionsEnabled is true)
                {
                    if (DynamicReflections.shouldDrawNightSky)
                    {
                        DynamicReflections.isFilteringSky = true;
                        Game1.currentLocation.waterColor.Value = new Color(60, 240, 255) * DynamicReflections.waterAlpha;
                        SpriteBatchToolkit.RenderWaterReflectionNightSky();
                    }
                }

                // Handle preliminary puddles reflection and draw logic
                if (DynamicReflections.currentPuddleSettings.ShouldGeneratePuddles is true)
                {
                    DynamicReflections.isFilteringPuddles = true;
                    SpriteBatchToolkit.RenderPuddles();
                    DynamicReflections.isFilteringPuddles = false;
                    DynamicReflections.isDrawingPuddles = true;
                }
                if (DynamicReflections.shouldDrawPuddlesReflection is true)
                {
                    DynamicReflections.isFilteringPuddles = true;
                    SpriteBatchToolkit.RenderPuddleReflectionPlayerSprite();
                    DynamicReflections.isFilteringPuddles = false;
                }

                // Draw the filtered layer, if needed
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                SpriteBatchToolkit.HandleBackgroundDraw();
                Game1.spriteBatch.End();

                // Resume previous SpriteBatch
                SpriteBatchToolkit.ResumeCachedSpriteBatch(Game1.spriteBatch);
                if (DynamicReflections.isFilteringWater is false && DynamicReflections.shouldDrawNightSky is false)
                {
                    return true;
                }

                // Keep the original single-Back behavior intact.
                // Only multi-Back* maps defer the final water/sky presentation until the top of the background stack.
                if (hasMultipleBackgroundLayers is true)
                {
                    DynamicReflections.shouldDeferSkyReflectionPresentation = DynamicReflections.shouldDrawNightSky;
                    DynamicReflections.shouldDeferWaterReflectionPresentation = DynamicReflections.isFilteringWater;
                    DynamicReflections.isFilteringSky = false;
                    DynamicReflections.isFilteringWater = false;
                    return true;
                }

                // Handle Visible Fish Compatability
                LayerPatch.DrawNormalReversePatch(__instance, displayDevice, mapViewport, displayOffset, pixelZoom);
                DynamicReflections.shouldSkipWaterOverlay = true;
                Game1.currentLocation.drawWater(Game1.spriteBatch);
                DynamicReflections.shouldSkipWaterOverlay = false;

                // Draw the sky reflection
                if (DynamicReflections.shouldDrawNightSky is true)
                {
                    SpriteBatchToolkit.CacheSpriteBatchSettings(Game1.spriteBatch, endSpriteBatch: true);

                    SpriteBatchToolkit.DrawNightSky();

                    if (DynamicReflections.isFilteringWater is true)
                    {
                        DynamicReflections.isFilteringWater = false;
                        DynamicReflections.isDrawingWaterReflection = true;
                    }

                    // Resume previous SpriteBatch
                    SpriteBatchToolkit.ResumeCachedSpriteBatch(Game1.spriteBatch);
                }
                else if (DynamicReflections.isFilteringWater is true) // Draw the water reflection, if sky reflections aren't currently active
                {
                    SpriteBatchToolkit.CacheSpriteBatchSettings(Game1.spriteBatch, endSpriteBatch: true);

                    SpriteBatchToolkit.DrawRenderedCharacters(isWavy: DynamicReflections.currentWaterSettings.IsReflectionWavy);

                    DynamicReflections.isFilteringWater = false;
                    DynamicReflections.isDrawingWaterReflection = true;

                    // Resume previous SpriteBatch
                    SpriteBatchToolkit.ResumeCachedSpriteBatch(Game1.spriteBatch);
                }
            }
            else if (__instance.Id.Equals("Buildings", StringComparison.OrdinalIgnoreCase) is true)
            {
                Game1.currentLocation.waterColor.Value = _waterColor;

                // Draw the cached Mirrors layer
                Game1.spriteBatch.Draw(DynamicReflections.mirrorsLayerRenderTarget, Vector2.Zero, Color.White);

                // Skip drawing the player's reflection if not needed
                if (DynamicReflections.shouldDrawMirrorReflection is true)
                {
                    SpriteBatchToolkit.CacheSpriteBatchSettings(Game1.spriteBatch, endSpriteBatch: true);

                    //DynamicReflections.isDrawingMirrorReflection = true;
                    SpriteBatchToolkit.DrawMirrorReflection(DynamicReflections.mirrorsLayerRenderTarget);

                    // Resume previous SpriteBatch
                    SpriteBatchToolkit.ResumeCachedSpriteBatch(Game1.spriteBatch);
                }
            }

            return true;
        }

        private static void DrawNormalPostfix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, int pixelZoom, float sort_offset = 0f)
        {
            if (__instance is null || String.IsNullOrEmpty(__instance.Id))
            {
                return;
            }

            var lowestBackgroundLayer = LayerToolkit.GetLowestBackgroundLayer(Game1.currentLocation);
            var highestBackgroundLayer = LayerToolkit.GetHighestBackgroundLayer(Game1.currentLocation);
            bool hasMultipleBackgroundLayers = LayerToolkit.HasMultipleBackgroundLayers(Game1.currentLocation);

            if (__instance.Equals(lowestBackgroundLayer) is true)
            {
                if (DynamicReflections.isDrawingPuddles is true)
                {
                    SpriteBatchToolkit.CacheSpriteBatchSettings(Game1.spriteBatch, endSpriteBatch: true);

                    // Draw puddle reflection
                    SpriteBatchToolkit.DrawPuddleReflection(DynamicReflections.puddlesRenderTarget);

                    // Resume previous SpriteBatch
                    SpriteBatchToolkit.ResumeCachedSpriteBatch(Game1.spriteBatch);

                    // Draw the puddles ontop of the "Back" layer
                    DynamicReflections.isFilteringPuddles = true;
                    LayerPatch.DrawNormalReversePatch(__instance, displayDevice, mapViewport, displayOffset, pixelZoom);
                    DynamicReflections.isFilteringPuddles = false;

                }
            }

            // Present the already-rendered water/sky reflections after the highest background layer.
            // The explicit water mask keeps the original placement behavior on maps that use Back* layer stacks.
            if (hasMultipleBackgroundLayers is true && __instance.Equals(highestBackgroundLayer) is true && (DynamicReflections.shouldDeferWaterReflectionPresentation is true || DynamicReflections.shouldDeferSkyReflectionPresentation is true))
            {
                SpriteBatchToolkit.CacheSpriteBatchSettings(Game1.spriteBatch, endSpriteBatch: true);

                SpriteBatchToolkit.RenderTopmostBackgroundWaterMask();

                if (DynamicReflections.shouldDeferSkyReflectionPresentation is true)
                {
                    SpriteBatchToolkit.DrawMaskedNightSky();

                    if (DynamicReflections.shouldDeferWaterReflectionPresentation is true)
                    {
                        DynamicReflections.shouldDeferWaterReflectionPresentation = false;
                        DynamicReflections.isDrawingWaterReflection = true;
                    }

                    DynamicReflections.shouldDeferSkyReflectionPresentation = false;
                }
                else if (DynamicReflections.shouldDeferWaterReflectionPresentation is true)
                {
                    SpriteBatchToolkit.DrawMaskedRenderedCharacters(isWavy: DynamicReflections.currentWaterSettings.IsReflectionWavy);

                    DynamicReflections.shouldDeferWaterReflectionPresentation = false;
                    DynamicReflections.isDrawingWaterReflection = true;
                }

                SpriteBatchToolkit.ResumeCachedSpriteBatch(Game1.spriteBatch);
            }
        }

        internal static void DrawNormalReversePatch(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, int pixelZoom, float sort_offset = 0f)
        {
            new NotImplementedException("It's a stub!");
        }

        // PyTK related patches
        private static void PyTKDrawLayerPrefix(Layer __instance, xTile.Display.IDisplayDevice device, xTile.Dimensions.Rectangle viewport, int pixelZoom, Location offset, bool wrap = false)
        {
            DrawNormalPrefix(__instance, device, viewport, offset, pixelZoom);
        }
    }
}
