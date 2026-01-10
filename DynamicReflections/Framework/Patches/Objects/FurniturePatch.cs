using DynamicReflections.Framework.Models;
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
            try
            {
                harmony.Patch(AccessTools.Method(_type, nameof(Furniture.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
                harmony.Patch(AccessTools.Method(_type, nameof(Furniture.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), postfix: new HarmonyMethod(GetType(), nameof(DrawPostfix)));
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to patch furniture in {this.GetType().Name}: DR will not be able to apply furniture reflections!", LogLevel.Warn);
                _monitor.Log($"Patch for DR furniture failed in {this.GetType().Name}: {ex}", LogLevel.Trace);
            }
        }

        [HarmonyBefore(new string[] { "PeacefulEnd.AlternativeTextures" })]
        private static bool DrawPrefix(Furniture __instance, NetInt ___sourceIndexOffset, NetVector2 ___drawPosition, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (DynamicReflections.isFilteringMirror is true && DynamicReflections.isDrawingMirrorReflection is false)
            {
                foreach (var mirror in DynamicReflections.mirrors.Values.ToList())
                {
                    if (mirror.IsEnabled is false || mirror.FurnitureLink != __instance || DynamicReflections.mirrorsManager.GetMask(__instance.ItemId) is null)
                    {
                        continue;
                    }

                    // Attempt to get the animation frame
                    Rectangle? sourceRectangle = null;
                    try
                    {
                        sourceRectangle = mirror.Settings.Dimensions;

                        // Verify we actually got the source rectangle for the DR texture
                        if (sourceRectangle is null)
                        {
                            _monitor.LogOnce($"Failed to get texture source rectangle from the DR item {__instance.ItemId}", LogLevel.Warn);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _monitor.LogOnce($"Failed to get texture source rectangle from the DR item {__instance.ItemId}", LogLevel.Warn);
                        _monitor.LogOnce($"Failed to get texture source rectangle from the DR item {__instance.ItemId}: {ex}", LogLevel.Trace);
                        return false;
                    }

                    spriteBatch.Draw(DynamicReflections.mirrorsManager.GetMask(__instance.ItemId), Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), sourceRectangle, Color.White * alpha, 0f, new Vector2(-mirror.Settings.Dimensions.X, -mirror.Settings.Dimensions.Y), 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.furniture_type.Value == 12) ? (2E-09f + __instance.tileLocation.Y / 100000f) : ((float)(__instance.boundingBox.Value.Bottom - ((__instance.furniture_type.Value == 6 || __instance.furniture_type.Value == 17 || __instance.furniture_type.Value == 13) ? 48 : 8)) / 10000f));

                    return false;
                }
            }
            return true;
        }

        private static void DrawPostfix(Furniture __instance, NetVector2 ___drawPosition, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            foreach (var mirror in DynamicReflections.mirrors.Values.ToList())
            {
                if (mirror.IsEnabled is false || mirror.FurnitureLink != __instance)
                {
                    continue;
                }

                var layerOffset = (__instance.furniture_type.Value == 12) ? (2E-09f + __instance.tileLocation.Y / 100000f) : ((float)(__instance.boundingBox.Value.Bottom - ((__instance.furniture_type.Value == 6 || __instance.furniture_type.Value == 17 || __instance.furniture_type.Value == 13) ? 48 : 8)) / 10000f);
                spriteBatch.Draw(DynamicReflections.maskedPlayerMirrorReflectionRenders[mirror.ActiveIndex], Vector2.Zero, DynamicReflections.maskedPlayerMirrorReflectionRenders[mirror.ActiveIndex].Bounds, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerOffset + 0.001f);

                break;
            }
        }
    }
}
