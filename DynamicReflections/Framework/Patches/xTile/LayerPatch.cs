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
            //harmony.Patch(AccessTools.Method(_object, nameof(Layer.Draw), new[] { typeof(IDisplayDevice), typeof(xTile.Dimensions.Rectangle), typeof(xTile.Dimensions.Location), typeof(bool), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(DrawPostfix)));

            harmony.CreateReversePatcher(AccessTools.Method(_object, nameof(Layer.Draw), new[] { typeof(IDisplayDevice), typeof(xTile.Dimensions.Rectangle), typeof(xTile.Dimensions.Location), typeof(bool), typeof(int) }), new HarmonyMethod(GetType(), nameof(DrawReversePatch))).Patch();
        }

        private static void DrawMirrorReflection(bool resumeSpriteBatch = false)
        {
            Game1.spriteBatch.End();
            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);

            var oldPosition = Game1.player.Position;
            var oldDirection = Game1.player.FacingDirection;
            var oldSprite = Game1.player.FarmerSprite;

            foreach (var mirrorPosition in DynamicReflections.activeMirrorPositions)
            {
                var mirror = DynamicReflections.mapMirrors[mirrorPosition];

                Game1.player.Position = mirror.PlayerReflectionPosition;
                Game1.player.FacingDirection = DynamicReflections.GetReflectedDirection(oldDirection, true);
                Game1.player.FarmerSprite = oldDirection == 0 ? DynamicReflections.mirrorReflectionSprite : oldSprite;

                Game1.player.draw(Game1.spriteBatch);
            }

            Game1.player.Position = oldPosition;
            Game1.player.FacingDirection = oldDirection;
            Game1.player.FarmerSprite = oldSprite;

            Game1.spriteBatch.End();

            if (resumeSpriteBatch)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        private static void DrawReflectionViaMatrix(bool resumeSpriteBatch = false)
        {
            Game1.spriteBatch.End();

            var scale = Matrix.CreateScale(1, -1, 1);
            var position = Matrix.CreateTranslation(0, Game1.GlobalToLocal(Game1.viewport, DynamicReflections.waterReflectionPosition.Value).Y * 2, 0);
            //_monitor.LogOnce($"{Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight} | {Game1.GlobalToLocal(Game1.viewport, ModEntry.farmerReflection.Position).Y}", LogLevel.Debug);

            RasterizerState rasterizer = new RasterizerState();
            rasterizer.CullMode = CullMode.CullClockwiseFace;

            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: rasterizer, transformMatrix: scale * position);

            var oldPosition = Game1.player.Position;
            Game1.player.Position = DynamicReflections.waterReflectionPosition.Value;
            Game1.player.draw(Game1.spriteBatch);
            Game1.player.Position = oldPosition;

            Game1.spriteBatch.End();

            if (resumeSpriteBatch)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        private static void RenderFlattenedPlayerSprite(bool resumeSpriteBatch = false)
        {
            // Set the render target
            Game1.graphics.GraphicsDevice.SetRenderTarget(DynamicReflections.renderTarget);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            DrawReflectionViaMatrix();

            // Drop the render target
            Game1.graphics.GraphicsDevice.SetRenderTarget(null);

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);

            if (resumeSpriteBatch)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        private static void DrawRenderedPlayer(bool isWavy = false, bool resumeSpriteBatch = false)
        {
            Game1.spriteBatch.End();

            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, effect: isWavy ? DynamicReflections.effect : null);
            //ModEntry.monitor.LogOnce($"[{ModEntry.renderTarget.Bounds}] {Game1.viewport.Width / 2} | {Game1.viewport.Height / 2}", LogLevel.Debug);
            Game1.spriteBatch.Draw(DynamicReflections.renderTarget, Vector2.Zero, Color.White);

            Game1.spriteBatch.End();

            if (resumeSpriteBatch)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        private static bool DrawPrefix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            DynamicReflections.isDrawingWaterReflection = false;
            DynamicReflections.isDrawingMirrorReflection = false;

            if (__instance.Id.Equals("Back", StringComparison.OrdinalIgnoreCase) is false)
            {
                return true;
            }

            // Handle preliminary water reflection logic
            if (DynamicReflections.shouldDrawWaterReflection is true)
            {
                DynamicReflections.isFilteringWater = true;

                if (DynamicReflections.isWavyReflection)
                {
                    RenderFlattenedPlayerSprite(resumeSpriteBatch: true);
                }
            }

            // Handle preliminary mirror reflection logic
            if (DynamicReflections.shouldDrawMirrorReflection is true)
            {
                DynamicReflections.isFilteringMirror = true;
            }

            // Draw the filtered layer, if needed
            if (DynamicReflections.isFilteringWater is false && DynamicReflections.isFilteringMirror is false)
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

            // Draw the mirror reflection
            if (DynamicReflections.isFilteringMirror is true)
            {
                DynamicReflections.isFilteringMirror = false;
                DynamicReflections.isDrawingMirrorReflection = true;

                DrawMirrorReflection(true);
            }

            return true;
        }

        private static void DrawPostfix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            if (DynamicReflections.waterReflectionPosition is null || __instance.Id.Equals("Back", StringComparison.OrdinalIgnoreCase) is false)
            {
                return;
            }

            //Game1.spriteBatch.Draw(_texture, new Vector2(ModEntry.farmerReflection.getLocalPosition(Game1.viewport).X - 4f, ModEntry.farmerReflection.getLocalPosition(Game1.viewport).Y - 4f), Color.White);
        }

        private static void DrawReversePatch(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            new NotImplementedException("It's a stub!");
        }
    }
}
