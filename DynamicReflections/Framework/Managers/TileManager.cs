using DynamicReflections.Framework.Models;
using DynamicReflections.Framework.Models.MapTilePresets;
using DynamicReflections.Framework.Models.MapTilePresets.Vanilla;
using DynamicReflections.Framework.Models.Reflections;
using DynamicReflections.Framework.Models.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace DynamicReflections.Framework.Managers
{
    internal class TileManager
    {
        private List<ReflectableMapObject> _refectableMapObjects;
        private Dictionary<TileSheet, Texture2D> _internalTileSheetTextures;

        private List<MapTilePresetTemplate> _mapTilePresets = new List<MapTilePresetTemplate>();

        public TileManager()
        {
            Reset();
            BuildInternalMapPresets();
        }

        public void Reset()
        {
            _refectableMapObjects = new List<ReflectableMapObject>();
        }

        public void BuildInternalMapPresets()
        {
            _mapTilePresets = new List<MapTilePresetTemplate>()
            {
                new TownMapTilePreset(),
                new BeachMapTilePreset(),
                new BeachNightMarketMapTilePreset(),
                new MountainMapTilePreset(),
                new ForestMapTilePreset(),
                new IslandNorthMapTilePreset(),
                new IslandWestMapTilePreset()
            };
        }

        public void LoadMapPreset(GameLocation location, bool resetCache = true)
        {
            // Reset the current cache
            if (resetCache)
            {
                Reset();
            }

            // Grab the tilesheets via XnaDisplayDevice
            _internalTileSheetTextures = DynamicReflections.modHelper.Reflection.GetField<Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice, "m_tileSheetTextures").GetValue();

            if (location is not null)
            {
                // Find any ReflectableMapTile that exist on the current map
                if (location.Map is not null && location.Map.Layers is not null)
                {
                    GetReflectableMapTilesFromLayers(location.backgroundLayers);
                    GetReflectableMapTilesFromLayers(location.buildingLayers);
                    GetReflectableMapTilesFromLayers(location.frontLayers);
                    GetReflectableMapTilesFromLayers(location.alwaysFrontLayers);
                }

                // Load in any presets
                var firstPresetMatch = _mapTilePresets.FirstOrDefault(p => p.MapName.Equals(location.Name, StringComparison.OrdinalIgnoreCase));
                if (firstPresetMatch is not null && DynamicReflections.modConfig.ShouldUsePresetMapReflections is true)
                {
                    if (firstPresetMatch.SkipWithModIds.Count == 0 || firstPresetMatch.SkipWithModIds.Any(id => DynamicReflections.modHelper.ModRegistry.IsLoaded(id)) is false)
                    {
                        if (firstPresetMatch.RequiredModIds.Count == 0 || firstPresetMatch.RequiredModIds.Any(id => DynamicReflections.modHelper.ModRegistry.IsLoaded(id)) is true)
                        {
                            foreach (var reflectableMapObject in firstPresetMatch.MapObjects)
                            {
                                // Skip any preset MapObjects if the tile currently already has a reflective property (via Content Patcher, etc.)
                                bool shouldUsePreset = true;
                                foreach (var mapTile in reflectableMapObject.GetTiles())
                                {
                                    if (_refectableMapObjects.Any(m => m.HasTileOnAnyLayer((int)mapTile.Tile.X, (int)mapTile.Tile.Y)))
                                    {
                                        shouldUsePreset = false;
                                        DynamicReflections.monitor.Log($"Skipping preset reflectable map object in {firstPresetMatch.MapName} at tile {reflectableMapObject.Tile} due to overlapping TileProperty at {mapTile.Tile}!", StardewModdingAPI.LogLevel.Trace);
                                        break;
                                    }
                                }
                                
                                if (shouldUsePreset is true)
                                {
                                    AddMapObject(reflectableMapObject);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetReflectableMapTilesFromLayers(List<KeyValuePair<Layer, int>> layers)
        {
            List<ReflectableMapTile> reflectableMapTiles = new List<ReflectableMapTile>();
            foreach (var layerPair in layers)
            {
                var layer = layerPair.Key;
                if (layer is null)
                {
                    continue;
                }

                for (int x = 0; x < layer.LayerWidth; x++)
                {
                    for (int y = 0; y < layer.LayerHeight; y++)
                    {
                        var reflectableMapTile = GetReflectableMapTile(layer, x, y);
                        if (reflectableMapTile is not null)
                        {
                            reflectableMapTiles.Add(reflectableMapTile);
                        }
                    }
                }
            }

            // Generate the ReflectableMapObjects based on the ReflectableMapTile.ParentId
            List<ReflectableMapObject> reflectableMapObjects = new List<ReflectableMapObject>();
            foreach (var reflectableMapTile in reflectableMapTiles)
            {
                var reflectableMapObject = reflectableMapObjects.FirstOrDefault(m => m.Id.Equals(reflectableMapTile.ObjectId, StringComparison.OrdinalIgnoreCase));
                if (reflectableMapObject is null)
                {
                    reflectableMapObject = new ReflectableMapObject(reflectableMapTile.ObjectId);
                    reflectableMapObjects.Add(reflectableMapObject);
                }

                reflectableMapObject.AddTile(reflectableMapTile);
            }

            // Add in the generated ReflectableMapObjects
            foreach (var reflectableMapObject in reflectableMapObjects)
            {
                AddMapObject(reflectableMapObject);
            }
        }

        private ReflectableMapTile GetReflectableMapTile(Layer layer, int x, int y)
        {
            var tile = layer.Tiles[x, y];
            if (tile is null)
            {
                return null;
            }

            if (tile.Properties.TryGetValue("DR_Object_Id", out string objectId) && string.IsNullOrEmpty(objectId) is false)
            {
                var reflectableMapTile = new ReflectableMapTile(layer.Id, x, y)
                {
                    ObjectId = objectId
                };

                // Get optional property values
                if (tile.Properties.TryGetValue("DR_Offset", out string offset) && string.IsNullOrEmpty(offset) is false && offset.Contains(" ") is true)
                {
                    var xOffsetText = offset.Split(" ")[0];
                    var yOffsetText = offset.Split(" ")[1];
                    if (float.TryParse(xOffsetText, out float xOffsetValue) is true && float.TryParse(yOffsetText, out float yOffsetValue) is true)
                    {
                        reflectableMapTile.Offset = new Vector2(xOffsetValue, yOffsetValue);
                    }
                }

                if (tile.Properties.TryGetValue("DR_Color", out string color) && string.IsNullOrEmpty(color) is false && color.Split(' ').Length >= 4 is true)
                {
                    var splitColorValues = color.Split(' ');

                    bool isValidColor = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (int.TryParse(splitColorValues[i], out int _) is false)
                        {
                            isValidColor = false;
                            break;
                        }
                    }

                    if ( (isValidColor))
                    {
                        {
                            reflectableMapTile.ReflectionColor = new Color(int.Parse(splitColorValues[0]), int.Parse(splitColorValues[1]), int.Parse(splitColorValues[2]), int.Parse(splitColorValues[3]));
                        }
                    }
                }

                return reflectableMapTile;
            }

            return null;
        }

        public void AddMapObject(ReflectableMapObject mapObject)
        {
            _refectableMapObjects.Add(mapObject);
        }

        public Texture2D GetTileSheetTexture(TileSheet tileSheet)
        {
            if (_internalTileSheetTextures.TryGetValue(tileSheet, out var texture2D))
            {
                return texture2D;
            }

            return null;
        }

        public List<ReflectableMapObject> GetReflectableMapObjectsForCurrentLocation()
        {
            return _refectableMapObjects;
        }
    }
}
