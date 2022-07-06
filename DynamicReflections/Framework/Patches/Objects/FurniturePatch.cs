using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace DynamicReflections.Framework.Patches.Objects
{
    internal class FurniturePatch : PatchTemplate
    {
        private readonly Type _type = typeof(Furniture);
        internal FurniturePatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            if (DynamicReflections.modHelper.ModRegistry.IsLoaded("spacechase0.DynamicGameAssets"))
            {
                try
                {
                    if (Type.GetType("DynamicGameAssets.Game.CustomBasicFurniture, DynamicGameAssets") is Type dgaFurnitureType && dgaFurnitureType != null)
                    {
                        harmony.Patch(AccessTools.Method(dgaFurnitureType, nameof(Furniture.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
                        harmony.Patch(AccessTools.Method(dgaFurnitureType, nameof(Furniture.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), postfix: new HarmonyMethod(GetType(), nameof(DrawPostfix)));
                    }
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Failed to patch Dynamic Game Assets in {this.GetType().Name}: DR will not be able to apply furniture reflections!", LogLevel.Warn);
                    _monitor.Log($"Patch for DGA failed in {this.GetType().Name}: {ex}", LogLevel.Trace);
                }
            }
        }

        private static bool DrawPrefix(Furniture __instance, NetInt ___sourceIndexOffset, NetVector2 ___drawPosition, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (__instance.Name == "PeacefulEnd.DGA.FashionableMirrors/Leaning Mirror")
            {
                if (DynamicReflections.isFilteringMirror is true && DynamicReflections.isDrawingMirrorReflection is false)
                {
                    var sourceRect = new Rectangle(4, 4, 8, 36);
                    var dummyTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                    dummyTexture.SetData(new Color[] { Color.White });

                    spriteBatch.Draw(dummyTexture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), sourceRect, Color.White * alpha, 0f, new Vector2(-sourceRect.X, -sourceRect.Y), 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)__instance.furniture_type == 12) ? (2E-09f + __instance.tileLocation.Y / 100000f) : ((float)(__instance.boundingBox.Value.Bottom - (((int)__instance.furniture_type == 6 || (int)__instance.furniture_type == 17 || (int)__instance.furniture_type == 13) ? 48 : 8)) / 10000f));

                    return false;
                }
            }
            return true;
        }

        private static void DrawPostfix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (__instance.Name == "PeacefulEnd.DGA.FashionableMirrors/Leaning Mirror")
            {
                var layerOffset = ((int)__instance.furniture_type == 12) ? (2E-09f + __instance.tileLocation.Y / 100000f) : ((float)(__instance.boundingBox.Value.Bottom - (((int)__instance.furniture_type == 6 || (int)__instance.furniture_type == 17 || (int)__instance.furniture_type == 13) ? 48 : 8)) / 10000f);
                spriteBatch.Draw(DynamicReflections.maskedPlayerMirrorReflectionRenders.First(), Vector2.Zero, DynamicReflections.maskedPlayerMirrorReflectionRenders.First().Bounds, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerOffset + 0.001f);
            }
        }
    }
}
