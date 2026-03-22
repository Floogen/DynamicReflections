using Microsoft.Xna.Framework;
using System.Collections.Generic;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace DynamicReflections.Framework.Utilities
{
    public static class LayerToolkit
    {
        private static GameLocation _cachedMetadataLocation;
        private static xTile.Map _cachedMetadataMap;
        private static Layer _cachedLowestBackgroundLayer;
        private static Layer _cachedHighestBackgroundLayer;
        private static bool _cachedHasMultipleBackgroundLayers;
        private static int _cachedMaxLayerWidth;
        private static int _cachedMaxLayerHeight;

        private static GameLocation _cachedViewportLocation;
        private static xTile.Map _cachedViewportMap;
        private static int _cachedViewportStartX;
        private static int _cachedViewportStartY;
        private static int _cachedViewportEndX = -1;
        private static int _cachedViewportEndY = -1;
        private static readonly HashSet<Point> _cachedTopmostVisibleBackgroundWaterTiles = new HashSet<Point>();

        internal static void InvalidateCaches()
        {
            _cachedMetadataLocation = null;
            _cachedMetadataMap = null;
            _cachedLowestBackgroundLayer = null;
            _cachedHighestBackgroundLayer = null;
            _cachedHasMultipleBackgroundLayers = false;
            _cachedMaxLayerWidth = 0;
            _cachedMaxLayerHeight = 0;

            _cachedViewportLocation = null;
            _cachedViewportMap = null;
            _cachedViewportStartX = 0;
            _cachedViewportStartY = 0;
            _cachedViewportEndX = -1;
            _cachedViewportEndY = -1;
            _cachedTopmostVisibleBackgroundWaterTiles.Clear();
        }

        internal static Layer GetLowestBackgroundLayer(GameLocation location)
        {
            EnsureBackgroundLayerMetadata(location);
            return _cachedLowestBackgroundLayer;
        }

        internal static Layer GetHighestBackgroundLayer(GameLocation location)
        {
            EnsureBackgroundLayerMetadata(location);
            return _cachedHighestBackgroundLayer;
        }

        internal static bool HasMultipleBackgroundLayers(GameLocation location)
        {
            EnsureBackgroundLayerMetadata(location);
            return _cachedHasMultipleBackgroundLayers;
        }

        internal static bool HasAnyTopmostVisibleBackgroundWater(GameLocation location)
        {
            EnsureTopmostVisibleBackgroundWaterCache(location);
            return _cachedTopmostVisibleBackgroundWaterTiles.Count > 0;
        }

        internal static bool IsTopmostVisibleBackgroundWater(GameLocation location, int x, int y)
        {
            EnsureTopmostVisibleBackgroundWaterCache(location);

            if (ReferenceEquals(location, _cachedViewportLocation) && x >= _cachedViewportStartX && x <= _cachedViewportEndX && y >= _cachedViewportStartY && y <= _cachedViewportEndY)
            {
                return _cachedTopmostVisibleBackgroundWaterTiles.Contains(new Point(x, y));
            }

            return IsTopmostVisibleBackgroundWaterCore(location, x, y);
        }

        private static void EnsureBackgroundLayerMetadata(GameLocation location)
        {
            xTile.Map map = location?.Map;
            if (ReferenceEquals(location, _cachedMetadataLocation) && ReferenceEquals(map, _cachedMetadataMap))
            {
                return;
            }

            _cachedMetadataLocation = location;
            _cachedMetadataMap = map;
            _cachedLowestBackgroundLayer = null;
            _cachedHighestBackgroundLayer = null;
            _cachedHasMultipleBackgroundLayers = false;
            _cachedMaxLayerWidth = 0;
            _cachedMaxLayerHeight = 0;

            if (location is null || map is null || location.backgroundLayers is null || location.backgroundLayers.Count == 0)
            {
                return;
            }

            // GameLocation.backgroundLayers should already be sorted by GameLocation.SortLayers.
            _cachedLowestBackgroundLayer = location.backgroundLayers[0].Key;
            _cachedHighestBackgroundLayer = location.backgroundLayers[location.backgroundLayers.Count - 1].Key;
            _cachedHasMultipleBackgroundLayers = _cachedLowestBackgroundLayer is not null
                && _cachedHighestBackgroundLayer is not null
                && _cachedLowestBackgroundLayer.Equals(_cachedHighestBackgroundLayer) is false;

            for (int i = 0; i < map.Layers.Count; i++)
            {
                Layer layer = map.Layers[i];
                if (layer.LayerWidth > _cachedMaxLayerWidth)
                {
                    _cachedMaxLayerWidth = layer.LayerWidth;
                }

                if (layer.LayerHeight > _cachedMaxLayerHeight)
                {
                    _cachedMaxLayerHeight = layer.LayerHeight;
                }
            }
        }

        private static void EnsureTopmostVisibleBackgroundWaterCache(GameLocation location)
        {
            EnsureBackgroundLayerMetadata(location);

            int startX;
            int startY;
            int endX;
            int endY;
            GetBufferedViewportTileBounds(location, out startX, out startY, out endX, out endY);

            if (ReferenceEquals(location, _cachedViewportLocation)
                && ReferenceEquals(location?.Map, _cachedViewportMap)
                && startX == _cachedViewportStartX
                && startY == _cachedViewportStartY
                && endX == _cachedViewportEndX
                && endY == _cachedViewportEndY)
            {
                return;
            }

            _cachedViewportLocation = location;
            _cachedViewportMap = location?.Map;
            _cachedViewportStartX = startX;
            _cachedViewportStartY = startY;
            _cachedViewportEndX = endX;
            _cachedViewportEndY = endY;
            _cachedTopmostVisibleBackgroundWaterTiles.Clear();

            if (location is null || endX < startX || endY < startY)
            {
                return;
            }

            // Cache the final, authoritative visible water surface once per buffered viewport.
            // The deferred Back* path can then reuse it without re-scanning every background layer during drawWater().
            for (int tileX = startX; tileX <= endX; tileX++)
            {
                for (int tileY = startY; tileY <= endY; tileY++)
                {
                    if (location.isWaterTile(tileX, tileY) && IsTopmostVisibleBackgroundWaterCore(location, tileX, tileY))
                    {
                        _cachedTopmostVisibleBackgroundWaterTiles.Add(new Point(tileX, tileY));
                    }
                }
            }
        }

        private static void GetBufferedViewportTileBounds(GameLocation location, out int startX, out int startY, out int endX, out int endY)
        {
            EnsureBackgroundLayerMetadata(location);

            if (location is null || _cachedMaxLayerWidth <= 0 || _cachedMaxLayerHeight <= 0)
            {
                startX = 0;
                startY = 0;
                endX = -1;
                endY = -1;
                return;
            }

            startX = System.Math.Max(0, (Game1.viewport.X / Game1.tileSize) - 1);
            startY = System.Math.Max(0, (Game1.viewport.Y / Game1.tileSize) - 1);
            endX = System.Math.Min(_cachedMaxLayerWidth - 1, ((Game1.viewport.X + Game1.viewport.Width) / Game1.tileSize) + 1);
            endY = System.Math.Min(_cachedMaxLayerHeight - 1, ((Game1.viewport.Y + Game1.viewport.Height) / Game1.tileSize) + 1);
        }

        private static bool IsTopmostVisibleBackgroundWaterCore(GameLocation location, int x, int y)
        {
            if (location is null || x < 0 || y < 0 || location.backgroundLayers is null || location.backgroundLayers.Count == 0)
            {
                return false;
            }

            for (int index = location.backgroundLayers.Count - 1; index >= 0; index--)
            {
                Layer layer = location.backgroundLayers[index].Key;
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
