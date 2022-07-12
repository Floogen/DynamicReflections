using DynamicReflections.Framework.Models;
using DynamicReflections.Framework.Models.Settings;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Managers
{
    internal class AssetManager
    {
        internal Texture2D PuddlesTileSheetTexture { get; }

        public AssetManager(IModHelper helper)
        {
            // Load in the puddles tilesheet
            PuddlesTileSheetTexture = helper.ModContent.Load<Texture2D>(Path.Combine(helper.DirectoryPath, "Framework", "Assets", "Textures", "puddles_sheet.png"));
        }
    }
}
