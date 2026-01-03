using DynamicReflections.Framework.Patches.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace DynamicReflections.Framework.Utilities
{
    public static class SpriteBatchToolkit
    {
        // General helpers
        private static bool _hasCache = false;
        private static RenderTarget2D _cachedRenderer;
        private static SpriteSortMode _cachedSpriteSortMode;
        private static BlendState _cachedBlendState;
        private static SamplerState _cachedSamplerState;
        private static DepthStencilState _cachedDepthStencilState;
        private static RasterizerState _cachedRasterizerState;
        private static Effect _cachedSpriteEffect;
        private static Matrix? _cachedMatrix;

        public static void CacheSpriteBatchSettings(SpriteBatch spriteBatch, bool endSpriteBatch = false)
        {
            var reflection = DynamicReflections.modHelper.Reflection;

            _cachedSpriteSortMode = reflection.GetField<SpriteSortMode>(spriteBatch, "_sortMode").GetValue();
            _cachedBlendState = reflection.GetField<BlendState>(spriteBatch, "_blendState").GetValue();
            _cachedSamplerState = reflection.GetField<SamplerState>(spriteBatch, "_samplerState").GetValue();
            _cachedDepthStencilState = reflection.GetField<DepthStencilState>(spriteBatch, "_depthStencilState").GetValue();
            _cachedRasterizerState = reflection.GetField<RasterizerState>(spriteBatch, "_rasterizerState").GetValue();
            _cachedSpriteEffect = reflection.GetField<Effect>(spriteBatch, "_effect").GetValue();
            _cachedMatrix = reflection.GetField<SpriteEffect>(spriteBatch, "_spriteEffect").GetValue().TransformMatrix;

            _hasCache = true;
            if (endSpriteBatch is true)
            {
                spriteBatch.End();
            }
        }

        public static bool ResumeCachedSpriteBatch(SpriteBatch spriteBatch)
        {
            if (_hasCache is false)
            {
                return false;
            }
            _hasCache = false;

            spriteBatch.Begin(_cachedSpriteSortMode, _cachedBlendState, _cachedSamplerState, _cachedDepthStencilState, _cachedRasterizerState, _cachedSpriteEffect, _cachedMatrix);
            return true;
        }

        public static void StartRendering(RenderTarget2D renderTarget2D)
        {
            var currentRenderer = Game1.graphics.GraphicsDevice.GetRenderTargets();
            if (currentRenderer is not null && currentRenderer.Length > 0 && currentRenderer[0].RenderTarget is not null)
            {
                _cachedRenderer = currentRenderer[0].RenderTarget as RenderTarget2D;
            }

            Game1.graphics.GraphicsDevice.SetRenderTarget(renderTarget2D);
        }

        public static void StopRendering()
        {
            Game1.graphics.GraphicsDevice.SetRenderTarget(_cachedRenderer);
            _cachedRenderer = null;
        }

        // LayerPatch helper methods
        // A note on the Render and Draw prefixed methods: These methods assume SpriteBatch has not been started via SpriteBatch.Begin
        internal static void DrawMirrorReflection(Texture2D mask)
        {
            DynamicReflections.mirrorReflectionEffect.Parameters["Mask"].SetValue(mask);
            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, effect: DynamicReflections.mirrorReflectionEffect);

            int index = 0;
            foreach (var mirrorPosition in DynamicReflections.activeMirrorPositions)
            {
                var mirror = DynamicReflections.mirrors[mirrorPosition];
                if (mirror.FurnitureLink is null)
                {
                    Game1.spriteBatch.Draw(DynamicReflections.composedPlayerMirrorReflectionRenders[index], Vector2.Zero, Color.White);
                }

                index++;
            }

            Game1.spriteBatch.End();
        }

        internal static void RenderMirrorsLayer()
        {
            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.mirrorsLayerRenderTarget);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            if (Game1.currentLocation is not null && Game1.currentLocation.Map is not null)
            {
                if (Game1.currentLocation.Map.GetLayer("Mirrors") is var mirrorsLayer && mirrorsLayer is not null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

                    // Draw the "Mirrors" layer
                    LayerPatch.DrawNormalReversePatch(mirrorsLayer, Game1.mapDisplayDevice, Game1.viewport, Location.Origin, 4);
                    Game1.spriteBatch.End();
                }
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderMirrorsFurniture()
        {
            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.mirrorsFurnitureRenderTarget);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            if (Game1.currentLocation is not null && Game1.currentLocation.furniture is not null)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
                foreach (var mirror in DynamicReflections.mirrors.Values.ToList())
                {
                    if (mirror.IsEnabled is false || mirror.FurnitureLink is null)
                    {
                        continue;
                    }

                    foreach (var furniture in Game1.currentLocation.furniture)
                    {
                        if (mirror.FurnitureLink != furniture)
                        {
                            continue;
                        }

                        DynamicReflections.isFilteringMirror = true;
                        furniture.draw(Game1.spriteBatch, (int)furniture.TileLocation.X, (int)furniture.TileLocation.Y);
                        DynamicReflections.isFilteringMirror = false;
                        break;
                    }
                }
                Game1.spriteBatch.End();
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderMirrorReflectionPlayerSprite()
        {
            var oldPosition = Game1.player.Position;
            var oldDirection = Game1.player.FacingDirection;
            var oldSprite = Game1.player.FarmerSprite;

            // Cache modData for Fashion Sense
            Dictionary<string, string> modDataCache = new Dictionary<string, string>();
            foreach (var dataKey in Game1.player.modData.Keys)
            {
                modDataCache[dataKey] = Game1.player.modData[dataKey];
            }

            // Draw the raw and flattened player sprites
            int index = 0;
            foreach (var mirrorPosition in DynamicReflections.activeMirrorPositions)
            {
                var rawReflectionRender = DynamicReflections.inBetweenRenderTarget;

                // Set the render target
                SpriteBatchToolkit.StartRendering(rawReflectionRender);

                // Draw the scene
                Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

                var mirror = DynamicReflections.mirrors[mirrorPosition];
                var offsetPosition = mirror.PlayerReflectionPosition;
                offsetPosition += mirror.Settings.ReflectionOffset * 16;

                // Compute translation to draw the player as if their Position were offsetPosition
                var playerScreen = Game1.GlobalToLocal(Game1.viewport, Game1.player.Position);
                var targetScreen = Game1.GlobalToLocal(Game1.viewport, offsetPosition);
                var delta = targetScreen - playerScreen;

                Game1.spriteBatch.Begin(
                    SpriteSortMode.FrontToBack,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    depthStencilState: null,
                    rasterizerState: null,
                    effect: null,
                    transformMatrix: Matrix.CreateTranslation(delta.X, delta.Y, 0f)
                );

                Game1.player.FacingDirection = DynamicReflections.GetReflectedDirection(oldDirection, true);
                Game1.player.FarmerSprite = oldDirection == 0 ? DynamicReflections.mirrorReflectionSprite : oldSprite;
                Game1.player.modData["FashionSense.Animation.FacingDirection"] = Game1.player.FacingDirection.ToString();

                Game1.player.draw(Game1.spriteBatch);

                Game1.spriteBatch.End();

                SpriteBatchToolkit.StopRendering();

                // Now use the rawReflectionRender to flip and apply other effects to them
                var composedReflectionRender = DynamicReflections.composedPlayerMirrorReflectionRenders[index];

                // Set the render target
                SpriteBatchToolkit.StartRendering(composedReflectionRender);

                // Draw the scene
                Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);

                Game1.player.FacingDirection = DynamicReflections.GetReflectedDirection(oldDirection, true);

                // Should flip the sprite on the X-axis (if facing front or back)
                var flipEffect = Game1.player.FacingDirection is (0 or 2)
                    ? SpriteEffects.FlipHorizontally
                    : SpriteEffects.None;

                // Use the mirror's reflection position as the flip center (like before, but without changing Position)
                float reflectCenterX = Game1.GlobalToLocal(Game1.viewport, offsetPosition).X;
                var flipOffset = Game1.player.FacingDirection is (0 or 2)
                    ? (Game1.viewport.Width * Game1.options.zoomLevel - reflectCenterX * 2f) - 64f
                    : 0f;

                // TODO: Implement these for Mirror.ReflectionScale
                var scale = new Vector2(1f, 1f);
                var scaleOffset = Vector2.Zero;

                Game1.spriteBatch.Draw(
                    rawReflectionRender,
                    new Vector2(-flipOffset, 0f),
                    rawReflectionRender.Bounds,
                    mirror.Settings.ReflectionOverlay,
                    0f,
                    scaleOffset,
                    scale,
                    flipEffect,
                    1f
                );

                Game1.spriteBatch.End();

                // Drop the render target
                SpriteBatchToolkit.StopRendering();

                // Now draw the individual furniture mask
                var furnitureMaskRender = DynamicReflections.mirrorsFurnitureRenderTarget;

                // Set the render target
                SpriteBatchToolkit.StartRendering(furnitureMaskRender);

                // Draw the scene
                Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

                if (mirror.IsEnabled is true && mirror.FurnitureLink is not null && Game1.currentLocation is not null && Game1.currentLocation.furniture is not null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

                    foreach (var furniture in Game1.currentLocation.furniture)
                    {
                        if (mirror.FurnitureLink != furniture)
                        {
                            continue;
                        }

                        DynamicReflections.isFilteringMirror = true;
                        furniture.draw(Game1.spriteBatch, (int)furniture.TileLocation.X, (int)furniture.TileLocation.Y);
                        DynamicReflections.isFilteringMirror = false;
                        break;
                    }
                    Game1.spriteBatch.End();
                }

                // Drop the render target
                SpriteBatchToolkit.StopRendering();

                // Now draw the masked version, for use by the furniture
                var maskedReflectionRender = DynamicReflections.maskedPlayerMirrorReflectionRenders[index];

                // Set the render target
                SpriteBatchToolkit.StartRendering(maskedReflectionRender);

                // Draw the scene
                Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

                DynamicReflections.mirrorReflectionEffect.Parameters["Mask"].SetValue(DynamicReflections.mirrorsFurnitureRenderTarget);
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, effect: DynamicReflections.mirrorReflectionEffect);

                Game1.spriteBatch.Draw(composedReflectionRender, Vector2.Zero, Color.White);

                Game1.spriteBatch.End();

                // Drop the render target
                SpriteBatchToolkit.StopRendering();

                index++;
            }

            // Restore player state
            Game1.player.Position = oldPosition;
            Game1.player.FacingDirection = oldDirection;
            Game1.player.FarmerSprite = oldSprite;

            // Restore modData for Fashion Sense
            foreach (var dataKey in modDataCache.Keys)
            {
                Game1.player.modData[dataKey] = modDataCache[dataKey];
            }

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderWaterReflectionNightSky()
        {
            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.nightSkyRenderTarget);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            if (Game1.currentLocation is not null && Game1.currentLocation.Map is not null)
            {
                if (LayerToolkit.GetLowestBackgroundLayer(Game1.currentLocation) is var backLayer && backLayer is not null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

                    DynamicReflections.isFilteringSky = true;
                    LayerPatch.DrawNormalReversePatch(backLayer, Game1.mapDisplayDevice, Game1.viewport, Location.Origin, 4);
                    DynamicReflections.isFilteringSky = false;

                    Game1.spriteBatch.End();

                    if (DynamicReflections.isFilteringWater is true)
                    {
                        SpriteBatchToolkit.DrawRenderedCharacters(isWavy: DynamicReflections.currentWaterSettings.IsReflectionWavy);
                    }

                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);

                    DynamicReflections.isFilteringStar = true;
                    LayerPatch.DrawNormalReversePatch(backLayer, Game1.mapDisplayDevice, Game1.viewport, Location.Origin, 4);
                    DynamicReflections.isFilteringStar = false;

                    foreach (var skyEffect in DynamicReflections.skyManager.skyEffectSprites.ToList())
                    {
                        skyEffect.draw(Game1.spriteBatch);
                    }
                    Game1.spriteBatch.End();
                }
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void DrawNightSky()
        {
            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            Game1.spriteBatch.Draw(DynamicReflections.nightSkyRenderTarget, Vector2.Zero, Color.White);
            Game1.spriteBatch.End();
        }

        internal static void RenderWaterReflectionPlayerSprite()
        {
            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.playerWaterReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            // Draw buildings before player
            if (DynamicReflections.modConfig.ArePlayerBuildingReflectionsEnabled)
            {
                RenderWaterReflectionPlayerBuildings(16, afterPlayer: false);
            }

            // Draw player reflection (if near water tile)
            if (DynamicReflections.shouldDrawWaterReflection)
            {
                DrawReflectionViaMatrix();
            }

            // Draw buildings after player
            if (DynamicReflections.modConfig.ArePlayerBuildingReflectionsEnabled)
            {
                RenderWaterReflectionPlayerBuildings(16, beforePlayer: false);
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderWaterReflectionNPCs()
        {
            if (Game1.currentLocation is null || Game1.currentLocation.characters is null)
            {
                return;
            }

            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.npcWaterReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (var npc in DynamicReflections.GetActiveNPCs(Game1.currentLocation))
            {
                if (DynamicReflections.npcToWaterReflectionPosition.ContainsKey(npc) is false)
                {
                    continue;
                }

                if (DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionDirection == Models.Settings.Direction.South)
                {
                    var scale = Matrix.CreateScale(1, -1, 1);
                    var position = Matrix.CreateTranslation(0, Game1.GlobalToLocal(Game1.viewport, DynamicReflections.npcToWaterReflectionPosition[npc]).Y * 2, 0);

                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);
                }
                else
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                }

                npc.draw(Game1.spriteBatch);

                Game1.spriteBatch.End();
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderPuddleReflectionNPCs()
        {
            if (Game1.currentLocation is null || Game1.currentLocation.characters is null)
            {
                return;
            }

            // If puddle reflections aren't configured / active, don't waste work.
            if (DynamicReflections.currentPuddleSettings is null)
            {
                return;
            }

            var config = DynamicReflections.modConfig;

            // If both NPC and companion reflections are disabled, skip entirely.
            bool npcReflectionsEnabled = config?.AreNPCReflectionsEnabled ?? true;
            bool companionReflectionsEnabled = config?.AreCompanionReflectionsEnabled ?? true;
            if (!npcReflectionsEnabled && !companionReflectionsEnabled)
            {
                return;
            }

            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.npcPuddleReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            int npcCount = 0;
            int companionCount = 0;

            foreach (var npc in DynamicReflections.GetActiveNPCs(Game1.currentLocation))
            {
                bool isCompanion = DynamicReflections.IsCustomCompanion(npc);

                // Respect global toggles and performance caps
                if (isCompanion)
                {
                    if (!companionReflectionsEnabled)
                    {
                        continue;
                    }

                    int maxCompanions = config?.PerformanceSettings?.MaxCompanionReflections ?? int.MaxValue;
                    if (companionCount >= maxCompanions)
                    {
                        continue;
                    }
                }
                else
                {
                    if (!npcReflectionsEnabled)
                    {
                        continue;
                    }

                    int maxNpcs = config?.PerformanceSettings?.MaxNpcReflections ?? int.MaxValue;
                    if (npcCount >= maxNpcs)
                    {
                        continue;
                    }
                }

                var offset = isCompanion
                    ? DynamicReflections.currentPuddleSettings.CompanionReflectionOffset
                    : DynamicReflections.currentPuddleSettings.NPCReflectionOffset;

                // Get the NPC's on-screen position
                var npcScreenPos = Game1.GlobalToLocal(Game1.viewport, npc.Position);

                // Mirror pivot: NPC's Y on screen + puddle offset
                float pivotY = npcScreenPos.Y + (offset.Y * 64f);

                // Flip vertically around that pivot
                var scale = Matrix.CreateScale(1, -1, 1);
                var position = Matrix.CreateTranslation(0, pivotY * 2f, 0);

                Game1.spriteBatch.Begin(
                    SpriteSortMode.FrontToBack,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    DynamicReflections.rasterizer,
                    transformMatrix: scale * position
                );

                npc.draw(Game1.spriteBatch);
                Game1.spriteBatch.End();

                if (isCompanion)
                {
                    companionCount++;
                }
                else
                {
                    npcCount++;
                }
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderWaterReflectionTerrain()
        {
            if (Game1.currentLocation is null || Game1.currentLocation.largeTerrainFeatures is null)
            {
                return;
            }

            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.terrainWaterReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (TerrainFeature terrainFeature in DynamicReflections.GetWaterReflectionTerrainFeatures(Game1.currentLocation))
            {
                if (terrainFeature is not Tree && terrainFeature is not Bush)
                {
                    continue;
                }

                int yOffset = 48;
                if (terrainFeature is Tree)
                {
                    yOffset = 72;
                }

                if (DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionDirection == Models.Settings.Direction.South)
                {
                    var scale = Matrix.CreateScale(1, -1, 1);
                    var position = Matrix.CreateTranslation(0, (Game1.GlobalToLocal(Game1.viewport, terrainFeature.Tile * 64).Y + yOffset) * 2, 0);

                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);
                }
                else
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                }

                terrainFeature.draw(Game1.spriteBatch);

                Game1.spriteBatch.End();
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderPuddleReflectionTerrain()
        {
            if (Game1.currentLocation is null || Game1.currentLocation.largeTerrainFeatures is null)
            {
                return;
            }

            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.terrainPuddleReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (TerrainFeature terrainFeature in DynamicReflections.GetPuddleReflectionTerrainFeatures(Game1.currentLocation))
            {
                if (terrainFeature is not Tree && terrainFeature is not Bush)
                {
                    continue;
                }

                int yOffset = 16;
                if (terrainFeature is Tree)
                {
                    yOffset = 8;
                }

                if (DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionDirection == Models.Settings.Direction.South)
                {
                    var scale = Matrix.CreateScale(1, -1, 1);
                    var position = Matrix.CreateTranslation(0, (Game1.GlobalToLocal(Game1.viewport, terrainFeature.Tile * 64).Y + yOffset) * 2, 0);

                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);
                }
                else
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                }

                terrainFeature.draw(Game1.spriteBatch);

                Game1.spriteBatch.End();
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderWaterReflectionGrass()
        {
            if (Game1.currentLocation is null || Game1.currentLocation.terrainFeatures is null)
            {
                return;
            }

            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.grassWaterReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (var terrainFeature in DynamicReflections.GetWaterReflectionTerrainFeatures(Game1.currentLocation))
            {
                if (terrainFeature is not Grass)
                {
                    continue;
                }

                if (DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionDirection == Models.Settings.Direction.South)
                {
                    var scale = Matrix.CreateScale(1, -1, 1);
                    var position = Matrix.CreateTranslation(0, (Game1.GlobalToLocal(Game1.viewport, terrainFeature.Tile * 64).Y + 72) * 2, 0);

                    // Using SpriteSortMode.BackToFront for Grass to properly draw any "flowers" that should be on top
                    Game1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);
                }
                else
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                }

                terrainFeature.draw(Game1.spriteBatch);

                Game1.spriteBatch.End();
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderPuddleReflectionGrass()
        {
            if (Game1.currentLocation is null || Game1.currentLocation.terrainFeatures is null)
            {
                return;
            }

            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.grassPuddleReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (var terrainFeature in DynamicReflections.GetPuddleReflectionTerrainFeatures(Game1.currentLocation))
            {
                if (terrainFeature is not Grass)
                {
                    continue;
                }

                if (DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionDirection == Models.Settings.Direction.South)
                {
                    var scale = Matrix.CreateScale(1, -1, 1);
                    var position = Matrix.CreateTranslation(0, (Game1.GlobalToLocal(Game1.viewport, terrainFeature.Tile * 64).Y + 48) * 2, 0);

                    // Using SpriteSortMode.BackToFront for Grass to properly draw any "flowers" that should be on top
                    Game1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);
                }
                else
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                }

                terrainFeature.draw(Game1.spriteBatch);

                Game1.spriteBatch.End();
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderWaterReflectionPlayerBuildings(int yOffsetBase, bool beforePlayer = true, bool afterPlayer = true)
        {
            if (Game1.currentLocation is null || Game1.currentLocation.buildings is null)
            {
                return;
            }

            foreach (var building in Game1.currentLocation.buildings)
            {
                if (building is null || building is GreenhouseBuilding)
                {
                    continue;
                }
                else if (beforePlayer is false && building.tileY.Value < Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (afterPlayer is false && building.tileY.Value > Game1.player.Tile.Y)
                {
                    continue;
                }

                int yOffset = (building.tilesHigh.Value * 64) - yOffsetBase;
                if (DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionDirection == Models.Settings.Direction.South)
                {
                    var scale = Matrix.CreateScale(1, -1, 1);
                    var position = Matrix.CreateTranslation(0, (Game1.GlobalToLocal(Game1.viewport, new Vector2(building.tileX.Value, building.tileY.Value) * 64).Y + yOffset) * 2, 0);

                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);
                }
                else
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                }

                // Ignore building's transparency when doing reflecitons
                var cachedBuildingAlpha = building.alpha;
                building.alpha = 1f;

                building.draw(Game1.spriteBatch);

                building.alpha = cachedBuildingAlpha;

                Game1.spriteBatch.End();
            }
        }

        internal static void DrawPuddleReflection(Texture2D mask)
        {
            DynamicReflections.mirrorReflectionEffect.Parameters["Mask"].SetValue(mask);
            Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, effect: DynamicReflections.mirrorReflectionEffect);

            if (DynamicReflections.shouldDrawNightSky)
            {
                Game1.spriteBatch.Draw(DynamicReflections.nightSkyRenderTarget, Vector2.Zero, Color.White);
            }

            if (DynamicReflections.shouldDrawWaterReflection is true || DynamicReflections.modConfig.AreTerrainReflectionsEnabled is true || DynamicReflections.modConfig.AreGrassReflectionsEnabled is true)
            {
                if (DynamicReflections.modConfig.AreTerrainReflectionsEnabled)
                {
                    // Draw terrain (tree / bushes)
                    Game1.spriteBatch.Draw(DynamicReflections.terrainPuddleReflectionRender, Vector2.Zero, DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionOverlay);
                }

                // Draw the player
                Game1.spriteBatch.Draw(DynamicReflections.playerPuddleReflectionRender, Vector2.Zero, DynamicReflections.currentPuddleSettings.ReflectionOverlay);

                if (DynamicReflections.modConfig.AreGrassReflectionsEnabled)
                {
                    // Draw grass
                    Game1.spriteBatch.Draw(DynamicReflections.grassPuddleReflectionRender, Vector2.Zero, DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionOverlay);
                }
            }

            Game1.spriteBatch.Draw(DynamicReflections.npcPuddleReflectionRender, Vector2.Zero, DynamicReflections.currentPuddleSettings.ReflectionOverlay);

            Game1.spriteBatch.End();
        }

        internal static void RenderPuddles()
        {
            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.puddlesRenderTarget);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            if (Game1.currentLocation is not null && Game1.currentLocation.Map is not null)
            {
                if (LayerToolkit.GetLowestBackgroundLayer(Game1.currentLocation) is var backLayer && backLayer is not null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

                    // Draw the "Back" layer with just the puddles
                    LayerPatch.DrawNormalReversePatch(backLayer, Game1.mapDisplayDevice, Game1.viewport, Location.Origin, 4);

                    Game1.spriteBatch.End();
                }
            }

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderPuddleReflectionPlayerSprite()
        {
            // Set the render target
            SpriteBatchToolkit.StartRendering(DynamicReflections.playerPuddleReflectionRender);

            // Draw the scene
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            // Draw buildings before player
            if (DynamicReflections.modConfig.ArePlayerBuildingReflectionsEnabled)
            {
                RenderWaterReflectionPlayerBuildings(1326, afterPlayer: false);
            }

            // Draw player reflection
            DrawPlayerPuddleReflection();

            // Draw buildings after player
            if (DynamicReflections.modConfig.ArePlayerBuildingReflectionsEnabled)
            {
                RenderWaterReflectionPlayerBuildings(32, beforePlayer: false);
            }

            // Draw puddle ripples on top, unchanged
            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (var rippleSprite in DynamicReflections.puddleManager.puddleRippleSprites.ToList())
            {
                rippleSprite.draw(Game1.spriteBatch);
            }
            Game1.spriteBatch.End();

            // Drop the render target
            SpriteBatchToolkit.StopRendering();
            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void DrawReflectionViaMatrix()
        {
            // Cache what we’re going to touch so we can restore it
            var oldDirection = Game1.player.FacingDirection;
            var oldSprite = Game1.player.FarmerSprite;

            var currentWaterSettings = DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation);

            // Always draw the *real* player, just flip the screen with a matrix.
            if (currentWaterSettings.ReflectionDirection == Models.Settings.Direction.South)
            {
                // Flip vertically around the water line in screen space.
                var scale = Matrix.CreateScale(1f, -1f, 1f);

                // Pivot at the water reflection line (already computed in world space, convert to screen).
                float pivotY = Game1.GlobalToLocal(Game1.viewport, DynamicReflections.waterReflectionPosition.Value).Y;
                var position = Matrix.CreateTranslation(0f, pivotY * 2f, 0f);

                Game1.spriteBatch.Begin(
                    SpriteSortMode.FrontToBack,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    DynamicReflections.rasterizer,
                    transformMatrix: scale * position
                );
            }
            else
            {
                // Non-south directions keep using the original mirror-style logic.
                Game1.spriteBatch.Begin(
                    SpriteSortMode.FrontToBack,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp
                );

                Game1.player.FacingDirection = DynamicReflections.GetReflectedDirection(oldDirection, true);
                Game1.player.FarmerSprite = oldDirection == 0
                    ? DynamicReflections.mirrorReflectionSprite
                    : oldSprite;

                Game1.player.modData["FashionSense.Animation.FacingDirection"] =
                    Game1.player.FacingDirection.ToString();
            }

            // IMPORTANT: No longer touch Game1.player.Position here.
            Game1.player.draw(Game1.spriteBatch);

            // Restore what changed
            Game1.player.FacingDirection = oldDirection;
            Game1.player.FarmerSprite = oldSprite;

            Game1.spriteBatch.End();
        }


        internal static void DrawPlayerPuddleReflection()
        {
            var oldDirection = Game1.player.FacingDirection;
            var oldSprite = Game1.player.FarmerSprite;

            // Original world position
            var oldPosition = Game1.player.Position;

            // Where the reflection was previously drawn (world space)
            var worldOffset = DynamicReflections.currentPuddleSettings.ReflectionOffset * 64f;
            var targetWorld = oldPosition - worldOffset;

            // Convert both positions to screen space to build an equivalent translation
            var playerScreen = Game1.GlobalToLocal(Game1.viewport, oldPosition);
            var targetScreen = Game1.GlobalToLocal(Game1.viewport, targetWorld);
            var delta = targetScreen - playerScreen;

            // Same vertical flip & pivot as before (across the player's original local Y)
            var scale = Matrix.CreateScale(1f, -1f, 1f);
            var pivot = Matrix.CreateTranslation(0f, playerScreen.Y * 2f, 0f);

            // Apply the offset as a pre-translation, then the original reflection matrix
            var preTranslation = Matrix.CreateTranslation(delta.X, delta.Y, 0f);
            var transform = preTranslation * scale * pivot;

            Game1.spriteBatch.Begin(
                SpriteSortMode.FrontToBack,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                depthStencilState: null,
                rasterizerState: DynamicReflections.rasterizer,
                effect: null,
                transformMatrix: transform
            );

            // Draw the player at their real position; transform handles reflection+offset
            Game1.player.draw(Game1.spriteBatch);

            Game1.player.FacingDirection = oldDirection;
            Game1.player.FarmerSprite = oldSprite;

            Game1.spriteBatch.End();
        }

        internal static void DrawRenderedCharacters(bool isWavy = false)
        {

            if (DynamicReflections.modConfig.AreWaterReflectionsEnabled || DynamicReflections.modConfig.AreTerrainReflectionsEnabled is true || DynamicReflections.modConfig.AreGrassReflectionsEnabled is true)
            {
                DynamicReflections.waterReflectionEffect.Parameters["ColorOverlay"].SetValue(DynamicReflections.modConfig.WaterReflectionSettings.ReflectionOverlay.ToVector4());
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, effect: isWavy ? DynamicReflections.waterReflectionEffect : null);

                if (DynamicReflections.modConfig.ArePlayerBuildingReflectionsEnabled)
                {
                    // Draw player buildings
                    //Game1.spriteBatch.Draw(DynamicReflections.buildingWaterReflectionRender, Vector2.Zero, DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionOverlay);
                }

                if (DynamicReflections.modConfig.AreTerrainReflectionsEnabled)
                {
                    // Draw terrain (tree / bushes)
                    Game1.spriteBatch.Draw(DynamicReflections.terrainWaterReflectionRender, Vector2.Zero, DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionOverlay);
                }

                if (DynamicReflections.modConfig.AreWaterReflectionsEnabled)
                {
                    // Draw the player
                    Game1.spriteBatch.Draw(DynamicReflections.playerWaterReflectionRender, Vector2.Zero, DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionOverlay);
                }

                if (DynamicReflections.modConfig.AreGrassReflectionsEnabled)
                {
                    // Draw grass
                    Game1.spriteBatch.Draw(DynamicReflections.grassWaterReflectionRender, Vector2.Zero, DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionOverlay);
                }

                Game1.spriteBatch.End();
            }

            if (DynamicReflections.modConfig.AreNPCReflectionsEnabled is true)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, effect: isWavy ? DynamicReflections.waterReflectionEffect : null);
                Game1.spriteBatch.Draw(DynamicReflections.npcWaterReflectionRender, Vector2.Zero, DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionOverlay);
                Game1.spriteBatch.End();
            }
        }

        internal static void HandleBackgroundDraw()
        {
            if (Game1.background is not null)
            {
                Game1.background.draw(Game1.spriteBatch);
            }

            if (Game1.currentLocation is not null)
            {
                Game1.currentLocation.drawBackground(Game1.spriteBatch);
            }
        }
    }
}
