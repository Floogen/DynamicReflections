using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using xTile.Tiles;

namespace DynamicReflections.Framework.Models.Reflections
{
    public class ReflectableMapObject : ReflectableObject
    {
        public readonly string Id;
        private List<ReflectableMapTile> _mapTiles;

        public ReflectableMapObject(string id, List<ReflectableMapTile> mapTiles)
        {
            Id = id;

            _mapTiles = new List<ReflectableMapTile>();
            foreach (var mapTile in  mapTiles)
            {
                AddTile(mapTile);
            }
        }

        public ReflectableMapObject(string id, ReflectableMapTile mapTile)
        {
            Id = id;
            _mapTiles = new List<ReflectableMapTile>() { mapTile };

            Tile = mapTile.Tile;
        }

        public void AddTile(ReflectableMapTile mapTile)
        {
            // If the newly added mapTile is lower than the current ReflectableMapObject.Tile, then use mapTile.Tile as the root Tile
            if (mapTile.Tile.Y > Tile.Y)
            {
                Tile = mapTile.Tile;
            }

            _mapTiles.Add(mapTile);
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
