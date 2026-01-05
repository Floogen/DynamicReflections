using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;

namespace DynamicReflections.Framework.Models.Reflections
{
    public class ReflectableBuilding : ReflectableObject
    {
        public Building Building;

        public ReflectableBuilding(Building building)
        {
            Building = building;
            Tile = new Vector2(building.tileX.Value, building.tileY.Value);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Building.draw(spriteBatch);
        }

        public override bool IsOnScreen()
        {
            return Utility.isOnScreen(Tile * 64, Building.tilesWide.Value * 64);
        }
    }
}
