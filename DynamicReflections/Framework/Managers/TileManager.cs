using DynamicReflections.Framework.Models;
using DynamicReflections.Framework.Models.MapTilePresets;
using DynamicReflections.Framework.Models.MapTilePresets.Vanilla;
using DynamicReflections.Framework.Models.Reflections;
using DynamicReflections.Framework.Models.Settings;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Managers
{
    internal class TileManager
    {
        private List<ReflectableMapObject> _refectableMapObjects;

        private List<MapTilePresetTemplate> _mapTilePresets = new List<MapTilePresetTemplate>()
        {
            new TownMapTilePreset()
        };

        public TileManager()
        {
            Reset();
        }

        public void Reset()
        {
            _refectableMapObjects = new List<ReflectableMapObject>();
        }

        public void LoadMapPreset(GameLocation location, bool resetCache = true)
        {
            // Reset the current cache
            if (resetCache)
            {
                Reset();
            }

            if (location is null)
            {
                return;
            }

            var firstPresetMatch = _mapTilePresets.FirstOrDefault(p => p.MapName.Equals(location.Name, StringComparison.OrdinalIgnoreCase));
            if (firstPresetMatch is not null)
            {
                foreach (var reflectableMapObject in firstPresetMatch.MapObjects)
                {
                    AddMapObject(reflectableMapObject);
                }
            }
        }

        public void AddMapObject(ReflectableMapObject mapObject)
        {
            _refectableMapObjects.Add(mapObject);
            _refectableMapObjects = _refectableMapObjects.OrderBy(r => r.Tile).ToList();
        }

        public List<ReflectableMapObject> GetReflectableMapObjectsForCurrentLocation()
        {
            return _refectableMapObjects;
        }
    }
}
