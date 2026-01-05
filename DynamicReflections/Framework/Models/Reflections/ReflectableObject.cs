using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicReflections.Framework.Models.Reflections
{
    public abstract class ReflectableObject
    {
        public Vector2 Tile { get; set; }

        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract bool IsOnScreen();
    }
}
