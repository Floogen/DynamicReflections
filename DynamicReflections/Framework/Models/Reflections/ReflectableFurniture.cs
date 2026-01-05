using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace DynamicReflections.Framework.Models.Reflections
{
    public class ReflectableFurniture : ReflectableObject
    {
        public Furniture Furniture;

        public ReflectableFurniture(Furniture furniture)
        {
            Furniture = furniture;
            Tile = furniture.TileLocation;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Furniture.isDrawingLocationFurniture = true;
            Furniture.draw(spriteBatch, -1, -1);
            Furniture.isDrawingLocationFurniture = false;
        }

        public override bool IsOnScreen()
        {
            // Allow for three tile (3 * 64) spacing for trees and bushes
            return Utility.isOnScreen(Tile * 64, 3 * 64);
        }

        public override bool IsEnabled()
        {
            return DynamicReflections.modConfig.AreFurnitureReflectionsEnabled;
        }
    }
}
