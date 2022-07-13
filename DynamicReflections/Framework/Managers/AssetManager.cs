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
            // Get the asset folder path
            var assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Load in the puddles tilesheet
            PuddlesTileSheetTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Textures", "puddles_sheet.png"));
        }
    }
}
