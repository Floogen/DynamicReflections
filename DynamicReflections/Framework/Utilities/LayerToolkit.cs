using DynamicReflections.Framework.Patches.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Layers;

namespace DynamicReflections.Framework.Utilities
{
    public static class LayerToolkit
    {
        internal static Layer GetLowestBackgroundLayer(GameLocation location)
        {
            if (location is null || location.backgroundLayers.Count == 0)
            {
                return null;
            }

            // GameLocation.backgroundLayers should be automatically sorted via StardewValley.GameLocation.SortLayers
            return location.backgroundLayers[0].Key;
        }

        internal static Layer GetHighestBackgroundLayer(GameLocation location)
        {
            if (location is null || location.backgroundLayers.Count == 0)
            {
                return null;
            }

            // GameLocation.backgroundLayers should be automatically sorted via StardewValley.GameLocation.SortLayers
            return location.backgroundLayers[location.backgroundLayers.Count - 1].Key;
        }
    }
}
