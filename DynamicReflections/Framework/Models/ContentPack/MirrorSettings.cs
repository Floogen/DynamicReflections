using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.ContentPack
{
    internal class MirrorSettings
    {
        public Rectangle Dimensions { get; set; }
        public float ReflectionScale { get; set; } // TODO: Implement this property
        public Color ReflectionOverlay { get; set; }
        public Vector2 ReflectionOffset { get; set; }
    }
}
