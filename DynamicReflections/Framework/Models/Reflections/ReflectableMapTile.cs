using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using xTile.Tiles;

namespace DynamicReflections.Framework.Models.Reflections
{
    public class ReflectableMapTile : ReflectableObject
    {
        public string LayerName { get; }
        public Tile MapTile { get; }

        public ReflectableMapTile(string layerName, Vector2 tileLocation)
        {
            LayerName = layerName;
            Tile = tileLocation;
        }

        public ReflectableMapTile(string layerName, int x, int y) : this(layerName, new Vector2(x, y))
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // TODO
        }

        public override bool IsOnScreen()
        {
            return Utility.isOnScreen(Tile * 64, 3 * 64);
        }

        public override bool IsEnabled()
        {
            return true;
        }
    }
}
