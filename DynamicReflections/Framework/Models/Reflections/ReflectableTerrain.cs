using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DynamicReflections.Framework.Models.Reflections
{
    public class ReflectableTerrain : ReflectableObject
    {
        public TerrainFeature Terrain;

        public ReflectableTerrain(TerrainFeature terrain)
        {
            Terrain = terrain;
            Tile = terrain.Tile;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Terrain is Tree tree)
            {
                float alphaCache = tree.alpha;
                tree.alpha = 1f;

                Terrain.draw(spriteBatch);
                tree.alpha = alphaCache;
            }
            else
            {
                Terrain.draw(spriteBatch);
            }
        }

        public override bool IsOnScreen()
        {
            // Allow for three tile (3 * 64) spacing for trees and bushes
            return Utility.isOnScreen(Tile * 64, 3 * 64);
        }

        public override bool IsEnabled()
        {
            if (Terrain is Grass)
            {
                return DynamicReflections.modConfig.AreGrassReflectionsEnabled;
            }

            return DynamicReflections.modConfig.AreTerrainReflectionsEnabled;
        }
    }
}
