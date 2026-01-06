using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;

namespace DynamicReflections.Framework.Models.Reflections
{
    public class ReflectableMapObject : ReflectableObject
    {
        public readonly string Id;

        private List<ReflectableMapTile> _mapTiles = new List<ReflectableMapTile>();
        private HashSet<string> _layers = new HashSet<string>();

        public ReflectableMapObject(string id)
        {
            Id = id;
        }

        public ReflectableMapObject(string id, List<ReflectableMapTile> mapTiles) : this(id)
        {
            _mapTiles = new List<ReflectableMapTile>();
            foreach (var mapTile in mapTiles)
            {
                AddTile(mapTile);
            }
        }

        public ReflectableMapObject(string id, ReflectableMapTile mapTile) : this(id)
        {
            _mapTiles = new List<ReflectableMapTile>();

            AddTile(mapTile);
        }

        public void AddTile(ReflectableMapTile mapTile)
        {
            // If the newly added mapTile is lower than the current ReflectableMapObject.Tile, then use mapTile.Tile as the root Tile
            if (mapTile.Tile.Y > Tile.Y)
            {
                Tile = mapTile.Tile;
            }

            _mapTiles.Add(mapTile);
            _mapTiles = _mapTiles.OrderBy(m => m.Tile.Y).ToList();

            _layers.Add(mapTile.LayerName.ToLower());
        }

        public bool HasTileWithLayer(string layerName)
        {
            return _layers.Contains(layerName.ToLower());
        }

        public void DrawByLayer(SpriteBatch spriteBatch, string layerName)
        {
            foreach (var mapTile in _mapTiles.Where(m => m.LayerName.Equals(layerName, System.StringComparison.OrdinalIgnoreCase)))
            {
                mapTile.Draw(spriteBatch);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var mapTile in _mapTiles)
            {
                mapTile.Draw(spriteBatch);
            }
        }

        public override bool IsOnScreen()
        {
            // Allow for three tile (3 * 64) spacing for trees and bushes
            foreach (var mapTile in _mapTiles)
            {
                if (mapTile.IsOnScreen() is false)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool IsEnabled()
        {
            // TODO: Change this
            return DynamicReflections.modConfig.AreTerrainReflectionsEnabled;
        }
    }
}
