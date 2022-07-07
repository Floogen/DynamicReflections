using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.ContentPack
{
    internal class ContentPackModel
    {
        public string FurnitureId { get; set; }
        public string MaskTexture { get; set; }
        public MirrorSettings Mirror { get; set; }

        internal Texture2D Mask { get; set; }
    }
}
