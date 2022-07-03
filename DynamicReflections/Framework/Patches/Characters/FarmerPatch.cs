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

namespace DynamicReflections.Framework.Patches.Buildings
{
    internal class FarmerPatch : PatchTemplate
    {

        private readonly Type _type = typeof(Farmer);

        internal FarmerPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_type, nameof(Farmer.warpFarmer), new[] { typeof(Warp), typeof(int) }), prefix: new HarmonyMethod(GetType(), nameof(WarpFarmerPrefix)));
            harmony.Patch(AccessTools.Method(_type, nameof(Farmer.GetBoundingBox), null), postfix: new HarmonyMethod(GetType(), nameof(GetBoundingBoxPostfix)));
        }

        private static bool WarpFarmerPrefix(Farmer __instance, Warp w, int warp_collide_direction)
        {
            if (__instance.isFakeEventActor)
            {
                return false;
            }

            return true;
        }

        private static void GetBoundingBoxPostfix(Farmer __instance, ref Rectangle __result)
        {
            if (__instance.isFakeEventActor)
            {
                __result = Rectangle.Empty;
            }
        }
    }
}
