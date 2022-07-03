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
    internal class ToolPatch : PatchTemplate
    {

        private readonly Type _type = typeof(Tool);

        internal ToolPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_type, nameof(Tool.doesShowTileLocationMarker), null), postfix: new HarmonyMethod(GetType(), nameof(DoesShowTileLocationMarkerPostfix)));
        }

        private static void DoesShowTileLocationMarkerPostfix(Tool __instance, ref bool __result)
        {
            if (DynamicReflections.isFilteringMirror || DynamicReflections.isFilteringWater)
            {
                __result = false;
            }
        }
    }
}
