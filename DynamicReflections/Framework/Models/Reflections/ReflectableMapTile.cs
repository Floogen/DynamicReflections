using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace DynamicReflections.Framework.Models.Reflections
{
    public class ReflectableMapTile : ReflectableObject
    {
        public string LayerName { get; }
        public Tile MapTile { get; internal set; }

        public Vector2 Offset { get; set; } = new Vector2(0f, 1f);
        public Color ReflectionColor { get; set; } = Color.White;

        private int _xTile;
        private int _yTile;

        public ReflectableMapTile(string layerName, int x, int y)
        {
            LayerName = layerName;
            Tile = new Vector2(x, y);

            _xTile = x;
            _yTile = y;
        }

        public bool SetMapTile(GameLocation location)
        {
            if (location is not null && location.Map is not null)
            {
                var layer = location.Map.GetLayer(LayerName);
                var mapTile = layer.Tiles[_xTile, _yTile];
                if (mapTile is not null)
                {
                    MapTile = mapTile;
                    return true;
                }
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (MapTile is null && SetMapTile(Game1.currentLocation) is false)
            {
                return;
            }

            var texture = DynamicReflections.tileManager.GetTileSheetTexture(MapTile.TileSheet);
            if (texture is null)
            {
                return;
            }

            var sourceRectangle = MapTile.TileSheet.GetTileImageBounds(MapTile.TileIndex);
            var parsedSourceRectangle = new Rectangle(sourceRectangle.X, sourceRectangle.Y, sourceRectangle.Width, sourceRectangle.Height);

            Game1.spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, (Tile - Offset) * 64), parsedSourceRectangle, ReflectionColor, 0f, Vector2.Zero, Layer.zoom, SpriteEffects.None, 0.9f);
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
