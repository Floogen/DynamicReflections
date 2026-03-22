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
using xTile.Tiles;

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

        internal static bool HasMultipleBackgroundLayers(GameLocation location)
        {
            var lowest = GetLowestBackgroundLayer(location);
            var highest = GetHighestBackgroundLayer(location);

            return lowest is not null && highest is not null && !lowest.Equals(highest);
        }

        internal static bool IsTopmostVisibleBackgroundWater(GameLocation location, int x, int y)
        {
            if (location is null || x < 0 || y < 0 || location.backgroundLayers is null || location.backgroundLayers.Count == 0)
            {
                return false;
            }

            for (int index = location.backgroundLayers.Count - 1; index >= 0; index--)
            {
                var layer = location.backgroundLayers[index].Key;
                if (layer is null || x >= layer.LayerWidth || y >= layer.LayerHeight)
                {
                    continue;
                }

                Tile tile = layer.Tiles[x, y];
                if (tile is null)
                {
                    continue;
                }

                return tile.Properties.TryGetValue("Water", out _) || tile.TileIndexProperties.TryGetValue("Water", out _);
            }

            return false;
        }
    }
}
