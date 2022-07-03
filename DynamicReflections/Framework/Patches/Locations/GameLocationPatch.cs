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
    internal class GameLocationPatch : PatchTemplate
    {

        private readonly Type _type = typeof(GameLocation);

        internal GameLocationPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_type, nameof(GameLocation.UpdateWhenCurrentLocation), new[] { typeof(GameTime) }), postfix: new HarmonyMethod(GetType(), nameof(UpdateWhenCurrentLocationPostfix)));
        }

        private static void UpdateWhenCurrentLocationPostfix(GameLocation __instance, GameTime time)
        {

        }
    }
}
