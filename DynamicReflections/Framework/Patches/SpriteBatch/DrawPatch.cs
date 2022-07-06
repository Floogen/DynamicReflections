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
using Object = StardewValley.Object;

namespace DynamicReflections.Framework.Patches.SMAPI
{
    internal class DrawPatch : PatchTemplate
    {
        private readonly Type _type = typeof(SpriteBatch);
        internal DrawPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_type, nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
        }

        private static bool DrawPrefix(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, ref SpriteEffects effects, float layerDepth)
        {
            if (DynamicReflections.shouldOverrideHorizontalFlip is true && effects != SpriteEffects.FlipVertically)
            {
                effects |= SpriteEffects.FlipHorizontally;
            }

            return true;
        }
    }
}
