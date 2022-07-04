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

        private static void DrawMirrorReflection(bool resumeSpriteBatch = false)
        {
            Game1.spriteBatch.End();

            DynamicReflections.mirrorReflectionEffect.Parameters["Mask"].SetValue(DynamicReflections.mirrorsRenderTarget);
            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, effect: DynamicReflections.mirrorReflectionEffect);

            Game1.spriteBatch.Draw(DynamicReflections.playerMirrorReflectionRender, Vector2.Zero, Color.White);

            Game1.spriteBatch.End();

            if (resumeSpriteBatch)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        internal static void DrawMirrorsLayer(bool useCachedRender = false)
        {
            if (useCachedRender)
            {
                Game1.spriteBatch.Draw(DynamicReflections.mirrorsRenderTarget, Vector2.Zero, Color.White);
            }
            else
            {
                if (Game1.currentLocation is null || Game1.currentLocation.Map is null)
                {
                    return;
                }

                if (Game1.currentLocation.Map.GetLayer("Mirrors") is var mirrorsLayer && mirrorsLayer is null)
                {
                    return;
                }
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

                // Draw the "Mirrors" layer
                LayerPatch.DrawReversePatch(mirrorsLayer, Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
            }
        }

        private static void RenderMirrors()
        {
            // Set the render target
            Game1.graphics.GraphicsDevice.SetRenderTarget(DynamicReflections.mirrorsRenderTarget);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            DrawMirrorsLayer();

            // Drop the render target
            Game1.graphics.GraphicsDevice.SetRenderTarget(null);

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        private static void RenderMirrorReflectionPlayerSprite(bool resumeSpriteBatch = false)
        {
            Game1.spriteBatch.End();

            // Set the render target
            Game1.graphics.GraphicsDevice.SetRenderTarget(DynamicReflections.playerMirrorReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            var oldPosition = Game1.player.Position;
            var oldDirection = Game1.player.FacingDirection;
            var oldSprite = Game1.player.FarmerSprite;

            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (var mirrorPosition in DynamicReflections.activeMirrorPositions)
            {
                var mirror = DynamicReflections.mapMirrors[mirrorPosition];

                Game1.player.Position = mirror.PlayerReflectionPosition;
                Game1.player.FacingDirection = DynamicReflections.GetReflectedDirection(oldDirection, true);
                Game1.player.FarmerSprite = oldDirection == 0 ? DynamicReflections.mirrorReflectionSprite : oldSprite;

                Game1.player.draw(Game1.spriteBatch);
            }
            Game1.spriteBatch.End();

            Game1.player.Position = oldPosition;
            Game1.player.FacingDirection = oldDirection;
            Game1.player.FarmerSprite = oldSprite;

            // Drop the render target
            Game1.graphics.GraphicsDevice.SetRenderTarget(null);

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);

            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        }

        private static void DrawReflectionViaMatrix(bool resumeSpriteBatch = false)
        {
            Game1.spriteBatch.End();

            var scale = Matrix.CreateScale(1, -1, 1);
            var position = Matrix.CreateTranslation(0, Game1.GlobalToLocal(Game1.viewport, DynamicReflections.waterReflectionPosition.Value).Y * 2, 0);
            //_monitor.LogOnce($"{Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight} | {Game1.GlobalToLocal(Game1.viewport, ModEntry.farmerReflection.Position).Y}", LogLevel.Debug);

            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);

            var oldPosition = Game1.player.Position;
            Game1.player.Position = DynamicReflections.waterReflectionPosition.Value;
            Game1.player.draw(Game1.spriteBatch);
            Game1.player.Position = oldPosition;

            Game1.spriteBatch.End();

            if (resumeSpriteBatch)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        private static void RenderWaterReflectionPlayerSprite(bool resumeSpriteBatch = false)
        {
            // Set the render target
            Game1.graphics.GraphicsDevice.SetRenderTarget(DynamicReflections.playerWaterReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            DrawReflectionViaMatrix();

            // Drop the render target
            Game1.graphics.GraphicsDevice.SetRenderTarget(null);

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);

            if (resumeSpriteBatch)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        private static void DrawRenderedPlayer(bool isWavy = false, bool resumeSpriteBatch = false)
        {
            Game1.spriteBatch.End();

            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, effect: isWavy ? DynamicReflections.waterReflectionEffect : null);
            //ModEntry.monitor.LogOnce($"[{ModEntry.renderTarget.Bounds}] {Game1.viewport.Width / 2} | {Game1.viewport.Height / 2}", LogLevel.Debug);
            Game1.spriteBatch.Draw(DynamicReflections.playerWaterReflectionRender, Vector2.Zero, Color.White);

            Game1.spriteBatch.End();

            if (resumeSpriteBatch)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        private static bool DrawPrefix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            DynamicReflections.isDrawingWaterReflection = false;
            DynamicReflections.isDrawingMirrorReflection = false;

            if (__instance.Id.Equals("Back", StringComparison.OrdinalIgnoreCase) is true)
            {
                // Pre-render the Mirrors layer
                RenderMirrors();

                // Pre-render the mirror reflections
                RenderMirrorReflectionPlayerSprite();

                // Handle preliminary water reflection logic
                if (DynamicReflections.shouldDrawWaterReflection is true)
                {
                    DynamicReflections.isFilteringWater = true;

                    if (DynamicReflections.isWavyReflection)
                    {
                        RenderWaterReflectionPlayerSprite(resumeSpriteBatch: true);
                    }
                }

                // Draw the filtered layer, if needed
                if (DynamicReflections.isFilteringWater is false)
                {
                    return true;
                }
                LayerPatch.DrawReversePatch(__instance, displayDevice, mapViewport, displayOffset, wrapAround, pixelZoom);

                // Draw the water reflection
                if (DynamicReflections.isFilteringWater is true)
                {
                    if (DynamicReflections.isWavyReflection)
                    {
                        DrawRenderedPlayer(isWavy: true, resumeSpriteBatch: true);
                    }
                    else
                    {
                        DrawReflectionViaMatrix(resumeSpriteBatch: true);
                    }

                    DynamicReflections.isFilteringWater = false;
                    DynamicReflections.isDrawingWaterReflection = true;
                }
            }
            else if (__instance.Id.Equals("Buildings", StringComparison.OrdinalIgnoreCase) is true)
            {
                DrawMirrorsLayer(useCachedRender: true);

                // Skip drawing the player's reflection if not needed
                if (DynamicReflections.shouldDrawMirrorReflection is true)
                {
                    DynamicReflections.isDrawingMirrorReflection = true;
                    DrawMirrorReflection(resumeSpriteBatch: true);
                }
            }

            return true;
        }

        private static void DrawReversePatch(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            new NotImplementedException("It's a stub!");
        }
    }
}
