using DynamicReflections.Framework.Models.Reflections;
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
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

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

        // Pre-compiled open delegates to read SpriteBatch private state without SMAPI reflection overhead
        private static readonly Func<SpriteBatch, SpriteSortMode> _getSortMode;
        private static readonly Func<SpriteBatch, BlendState> _getBlendState;
        private static readonly Func<SpriteBatch, SamplerState> _getSamplerState;
        private static readonly Func<SpriteBatch, DepthStencilState> _getDepthStencilState;
        private static readonly Func<SpriteBatch, RasterizerState> _getRasterizerState;
        private static readonly Func<SpriteBatch, Effect> _getEffect;
        private static readonly Func<SpriteBatch, Matrix?> _getMatrix;
        private static readonly bool _hasCompiledDelegates;

        static SpriteBatchToolkit()
        {
            // Build dynamic getters on startup so we can snapshot SpriteBatch state each frame
            // without paying reflection string lookup or value-type boxing costs.
            try
            {
                Type sbType = typeof(SpriteBatch);
                Type seType = sbType.Assembly.GetType("Microsoft.Xna.Framework.Graphics.SpriteEffect");

                FieldInfo fSort = sbType.GetField("_sortMode", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fBlend = sbType.GetField("_blendState", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fSampler = sbType.GetField("_samplerState", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fDepth = sbType.GetField("_depthStencilState", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fRaster = sbType.GetField("_rasterizerState", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fEffect = sbType.GetField("_effect", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fSpriteEffect = sbType.GetField("_spriteEffect", BindingFlags.Instance | BindingFlags.NonPublic);
                PropertyInfo propMatrix = seType?.GetProperty("TransformMatrix", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (fSort != null && fBlend != null && fSampler != null && fDepth != null && fRaster != null && fEffect != null && fSpriteEffect != null && propMatrix != null)
                {
                    _getSortMode = CreateGetter<SpriteSortMode>(fSort);
                    _getBlendState = CreateGetter<BlendState>(fBlend);
                    _getSamplerState = CreateGetter<SamplerState>(fSampler);
                    _getDepthStencilState = CreateGetter<DepthStencilState>(fDepth);
                    _getRasterizerState = CreateGetter<RasterizerState>(fRaster);
                    _getEffect = CreateGetter<Effect>(fEffect);
                    _getMatrix = CreateMatrixGetter(fSpriteEffect, propMatrix);
                    _hasCompiledDelegates = true;
                }
            }
            catch (Exception ex)
            {
                _hasCompiledDelegates = false;
                DynamicReflections.monitor?.Log($"Failed to initialize compiled delegates for SpriteBatch: {ex.Message}", LogLevel.Warn);
            }
        }

        private static Func<SpriteBatch, T> CreateGetter<T>(FieldInfo field)
        {
            // Emits: ldarg.0, ldfld <field>, ret
            var dm = new DynamicMethod($"DR_Get_{field.Name}", typeof(T), new[] { typeof(SpriteBatch) }, typeof(SpriteBatchToolkit), true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ret);
            return (Func<SpriteBatch, T>)dm.CreateDelegate(typeof(Func<SpriteBatch, T>));
        }

        private static Func<SpriteBatch, Matrix?> CreateMatrixGetter(FieldInfo fSpriteEffect, PropertyInfo propTransformMatrix)
        {
            // Null-safe getter: reads _spriteEffect.TransformMatrix, or null if _spriteEffect is null
            var dm = new DynamicMethod("DR_Get_TransformMatrix", typeof(Matrix?), new[] { typeof(SpriteBatch) }, typeof(SpriteBatchToolkit), true);
            var il = dm.GetILGenerator();
            var nullLabel = il.DefineLabel();
            var retLabel = il.DefineLabel();
            var loc = il.DeclareLocal(typeof(Matrix?));

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fSpriteEffect);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Brfalse_S, nullLabel);

            il.Emit(OpCodes.Callvirt, propTransformMatrix.GetGetMethod(true)!);
            il.Emit(OpCodes.Stloc, loc);
            il.Emit(OpCodes.Br_S, retLabel);

            il.MarkLabel(nullLabel);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ldloca_S, loc);
            il.Emit(OpCodes.Initobj, typeof(Matrix?));

            il.MarkLabel(retLabel);
            il.Emit(OpCodes.Ldloc, loc);
            il.Emit(OpCodes.Ret);

            return (Func<SpriteBatch, Matrix?>)dm.CreateDelegate(typeof(Func<SpriteBatch, Matrix?>));
        }

        public static void CacheSpriteBatchSettings(SpriteBatch spriteBatch, bool endSpriteBatch = false)
        {
            if (spriteBatch is null)
            {
                return;
            }

            // Fast path: read internal state directly via compiled delegates when enabled
            bool useCompiled = _hasCompiledDelegates && (DynamicReflections.modConfig?.PerformanceSettings?.EnableFastSettingsCache != false);
            if (useCompiled)
            {
                try
                {
                    _cachedSpriteSortMode = _getSortMode(spriteBatch);
                    _cachedBlendState = _getBlendState(spriteBatch);
                    _cachedSamplerState = _getSamplerState(spriteBatch);
                    _cachedDepthStencilState = _getDepthStencilState(spriteBatch);
                    _cachedRasterizerState = _getRasterizerState(spriteBatch);
                    _cachedSpriteEffect = _getEffect(spriteBatch);
                    _cachedMatrix = _getMatrix(spriteBatch);

                    _hasCache = true;
                    if (endSpriteBatch is true)
                    {
                        spriteBatch.End();
                    }
                    return;
                }
                catch
                {
                    // Fall back safely to SMAPI reflection below if anything fails
                }
            }

            // Fallback path: standard SMAPI reflection
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
            // Skip the render target pass if no mirrors are active or the map has no Mirrors layer
            if (DynamicReflections.modConfig?.PerformanceSettings?.EnableRenderTargetCulling != false)
            {
                if (DynamicReflections.activeMirrorPositions == null || DynamicReflections.activeMirrorPositions.Count == 0)
                {
                    return;
                }

                if (Game1.currentLocation?.Map?.GetLayer("Mirrors") is null)
                {
                    return;
                }
            }

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

            if (oldDirection == 0 || oldDirection == 2)
            {
                Game1.player.FarmerSprite = DynamicReflections.mirrorReflectionSprite;
            }

            Game1.player.FacingDirection = DynamicReflections.GetReflectedDirection(oldDirection, true);
            Game1.player.modData["FashionSense.Animation.FacingDirection"] = Game1.player.FacingDirection.ToString();

            foreach (var mirrorPosition in DynamicReflections.activeMirrorPositions)
            {
                var rawReflectionRender = DynamicReflections.inBetweenRenderTarget;

                // Set the render targets
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

                float mirrorRange = (mirror.FurnitureLink != null ? (int)Math.Ceiling(mirror.Settings.Dimensions.Height / 16f) : mirror.Settings.Dimensions.Height);
                var playerDistanceFromBase = Math.Abs((mirror.WorldPosition.Y - Game1.player.Position.Y) + 64f) / 64f / mirrorRange;
                Game1.spriteBatch.Draw(
                    rawReflectionRender,
                    new Vector2(-flipOffset, 0f),
                    rawReflectionRender.Bounds,
                    Color.Lerp(new Color(25, 25, 25, 25), mirror.Settings.ReflectionOverlay, 1f - playerDistanceFromBase),
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
            Game1.player.FarmerSprite = oldSprite;
            Game1.player.Position = oldPosition;
            Game1.player.FacingDirection = oldDirection;
            Game1.player.modData["FashionSense.Animation.FacingDirection"] = oldDirection.ToString();

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

            // Draw terrain before player
            RenderWaterReflectionTerrain(afterPlayer: false);

            // Draw map tiles before player
            RenderLayersMapReflections(afterPlayer: false);

            // Draw npcs before player
            RenderWaterReflectionNPCs(afterPlayer: false);

            // Draw farmhands before player
            DrawFarmhandWaterReflections(afterPlayer: false);

            // Draw player reflection (if near water tile)
            if (DynamicReflections.shouldDrawWaterReflection)
            {
                DrawPlayerWaterReflection(Game1.player);
            }

            // Draw farmhands after player
            DrawFarmhandWaterReflections(beforePlayer: false);

            // Draw terrain after player
            RenderWaterReflectionTerrain(beforePlayer: false);

            // Draw map tiles after player
            RenderLayersMapReflections(beforePlayer: false);

            // Draw npcs after player
            RenderWaterReflectionNPCs(beforePlayer: false);

            // Drop the render target
            SpriteBatchToolkit.StopRendering();

            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void RenderWaterReflectionNPCs(bool beforePlayer = true, bool afterPlayer = true)
        {
            if (Game1.currentLocation is null || Game1.currentLocation.characters is null || DynamicReflections.modConfig.AreNPCReflectionsEnabled is false)
            {
                return;
            }

            foreach (var npc in DynamicReflections.GetActiveNPCs(Game1.currentLocation))
            {
                if (DynamicReflections.npcToWaterReflectionPosition.ContainsKey(npc) is false)
                {
                    continue;
                }
                else if (beforePlayer is false && npc.Tile.Y <= Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (afterPlayer is false && npc.Tile.Y > Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (Utility.isOnScreen(npc.Position, 64 * 3) is false)
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
        }

        internal static void RenderPuddleReflectionNPCs(bool beforePlayer = true, bool afterPlayer = true)
        {
            if (Game1.currentLocation is null || Game1.currentLocation.characters is null || DynamicReflections.modConfig.AreNPCReflectionsEnabled is false)
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

                if (beforePlayer is false && npc.Tile.Y <= Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (afterPlayer is false && npc.Tile.Y > Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (Utility.isOnScreen(npc.Position, 64 * 3) is false)
                {
                    continue;
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
        }

        internal static void RenderWaterReflectionTerrain(bool beforePlayer = true, bool afterPlayer = true)
        {
            if (Game1.currentLocation is null)
            {
                return;
            }

            foreach (ReflectableObject reflectableObject in DynamicReflections.GetWaterReflectionTerrainFeatures(Game1.currentLocation))
            {
                if (reflectableObject.IsEnabled() is false)
                {
                    continue;
                }
                else if (beforePlayer is false && reflectableObject.Tile.Y <= Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (afterPlayer is false && reflectableObject.Tile.Y > Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (reflectableObject.IsOnScreen() is false)
                {
                    continue;
                }

                int yOffset = 0;
                var spriteSortMode = SpriteSortMode.FrontToBack;
                if (reflectableObject is ReflectableTerrain reflectableTerrain)
                {
                    if (reflectableTerrain.Terrain is not Tree && reflectableTerrain.Terrain is not Bush && reflectableTerrain.Terrain is not Grass)
                    {
                        continue;
                    }

                    yOffset = 48;
                    if (reflectableTerrain.Terrain is Tree)
                    {
                        yOffset = 72;
                    }
                    else if (reflectableTerrain.Terrain is Grass)
                    {
                        yOffset = 96;
                        spriteSortMode = SpriteSortMode.BackToFront;
                    }
                }
                else if (reflectableObject is ReflectableBuilding reflectableBuilding)
                {
                    yOffset = (reflectableBuilding.Building.tilesHigh.Value * 64) - 20;
                }
                else if (reflectableObject is ReflectableFurniture reflectableFurniture)
                {
                    yOffset = reflectableFurniture.Furniture.getTilesHigh() * 64;
                }

                if (DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionDirection == Models.Settings.Direction.South)
                {
                    var scale = Matrix.CreateScale(1, -1, 1);
                    var position = Matrix.CreateTranslation(0, (Game1.GlobalToLocal(Game1.viewport, reflectableObject.Tile * 64).Y + yOffset) * 2, 0);

                    Game1.spriteBatch.Begin(spriteSortMode, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);
                }
                else
                {
                    Game1.spriteBatch.Begin(spriteSortMode, BlendState.AlphaBlend, SamplerState.PointClamp);
                }

                reflectableObject.Draw(Game1.spriteBatch);

                Game1.spriteBatch.End();
            }
        }

        internal static void RenderPuddleReflectionTerrain(bool beforePlayer = true, bool afterPlayer = true)
        {
            if (Game1.currentLocation is null)
            {
                return;
            }

            foreach (ReflectableObject reflectableObject in DynamicReflections.GetPuddleReflectionTerrainFeatures(Game1.currentLocation))
            {
                if (reflectableObject.IsEnabled() is false)
                {
                    continue;
                }
                else if (beforePlayer is false && reflectableObject.Tile.Y <= Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (afterPlayer is false && reflectableObject.Tile.Y > Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (reflectableObject.IsOnScreen() is false)
                {
                    continue;
                }

                int yOffset = 0;
                var spriteSortMode = SpriteSortMode.FrontToBack;
                if (reflectableObject is ReflectableTerrain reflectableTerrain)
                {
                    if (reflectableTerrain.Terrain is not Tree && reflectableTerrain.Terrain is not Bush && reflectableTerrain.Terrain is not Grass)
                    {
                        continue;
                    }

                    yOffset = 16;
                    if (reflectableTerrain.Terrain is Tree)
                    {
                        yOffset = 8;
                    }
                    else if (reflectableTerrain.Terrain is Grass)
                    {
                        yOffset = 48;
                        spriteSortMode = SpriteSortMode.BackToFront;
                    }
                }
                else if (reflectableObject is ReflectableBuilding reflectableBuilding)
                {
                    yOffset = (reflectableBuilding.Building.tilesHigh.Value * 64) - 32;
                }
                else if (reflectableObject is ReflectableFurniture reflectableFurniture)
                {
                    yOffset = reflectableFurniture.Furniture.getTilesHigh() * 16;
                }

                if (DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionDirection == Models.Settings.Direction.South)
                {
                    var scale = Matrix.CreateScale(1, -1, 1);
                    var position = Matrix.CreateTranslation(0, (Game1.GlobalToLocal(Game1.viewport, reflectableObject.Tile * 64).Y + yOffset) * 2, 0);

                    Game1.spriteBatch.Begin(spriteSortMode, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);
                }
                else
                {
                    Game1.spriteBatch.Begin(spriteSortMode, BlendState.AlphaBlend, SamplerState.PointClamp);
                }

                reflectableObject.Draw(Game1.spriteBatch);

                Game1.spriteBatch.End();
            }
        }

        internal static void RenderLayersMapReflections(bool beforePlayer = true, bool afterPlayer = true)
        {
            if (Game1.currentLocation is null || Game1.currentLocation.map is null)
            {
                return;
            }

            RenderMapLayerReflections(Game1.currentLocation.backgroundLayers, beforePlayer, afterPlayer);
            RenderMapLayerReflections(Game1.currentLocation.buildingLayers, beforePlayer, afterPlayer);
            RenderMapLayerReflections(Game1.currentLocation.frontLayers, beforePlayer, afterPlayer);

            if (afterPlayer)
            {
                RenderMapLayerReflections(Game1.currentLocation.alwaysFrontLayers);
            }
        }

        internal static void RenderMapLayerReflections(List<KeyValuePair<Layer, int>> layers, bool beforePlayer = true, bool afterPlayer = true)
        {
            foreach (var layerPair in layers)
            {
                var layer = layerPair.Key;
                if (layer is null)
                {
                    continue;
                }

                foreach (var reflectableMapObject in DynamicReflections.tileManager.GetReflectableMapObjectsForCurrentLocation())
                {
                    if (reflectableMapObject.HasAnyTileWithLayer(layer.Id) is false || reflectableMapObject.IsEnabled() is false)
                    {
                        continue;
                    }
                    else if (beforePlayer is false && reflectableMapObject.Tile.Y < Game1.player.Tile.Y)
                    {
                        continue;
                    }
                    else if (afterPlayer is false && reflectableMapObject.Tile.Y > Game1.player.Tile.Y)
                    {
                        continue;
                    }
                    else if (reflectableMapObject.IsOnScreen() is false)
                    {
                        continue;
                    }

                    var scale = Matrix.CreateScale(1, -1, 1);
                    var position = Matrix.CreateTranslation(0, Game1.GlobalToLocal(Game1.viewport, reflectableMapObject.Tile * 64).Y * 2f, 0);
                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, rasterizerState: DynamicReflections.rasterizer, transformMatrix: scale * position);

                    DynamicReflections.isFilteringMap = true;
                    reflectableMapObject.DrawByLayer(Game1.spriteBatch, layer.Id);
                    DynamicReflections.isFilteringMap = false;

                    Game1.spriteBatch.End();
                }
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

            if (DynamicReflections.modConfig.ArePuddleReflectionsEnabled is true)
            {
                // Draw the player
                Game1.spriteBatch.Draw(DynamicReflections.playerPuddleReflectionRender, Vector2.Zero, DynamicReflections.currentPuddleSettings.ReflectionOverlay);
            }

            Game1.spriteBatch.End();
        }

        internal static void RenderPuddles()
        {
            // Skip puddle rendering if indoors or if it hasn't rained today/yesterday
            if (DynamicReflections.modConfig?.PerformanceSettings?.EnableRenderTargetCulling != false)
            {
                if (Game1.currentLocation == null || !Game1.currentLocation.IsOutdoors || (!Game1.isRaining && !Game1.IsRainingHere(Game1.currentLocation) && (Game1.player?.modData.ContainsKey(ModDataKeys.DID_RAIN_YESTERDAY) != true || Game1.player.modData[ModDataKeys.DID_RAIN_YESTERDAY] != "True")))
                {
                    return;
                }
            }

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

            // Draw terrain before player
            RenderPuddleReflectionTerrain(afterPlayer: false);

            // Draw map tiles before player
            RenderLayersMapReflections(afterPlayer: false);

            // Draw npcs before player
            RenderPuddleReflectionNPCs(afterPlayer: false);

            // Draw farmhands before player
            DrawFarmhandPuddleReflections(afterPlayer: false);

            // Draw player reflection
            DrawPlayerPuddleReflection(Game1.player);

            // Draw farmhands after player
            DrawFarmhandPuddleReflections(beforePlayer: false);

            // Draw terrain after player
            RenderPuddleReflectionTerrain(beforePlayer: false);

            // Draw map tiles after player
            RenderLayersMapReflections(beforePlayer: false);

            // Draw npcs after player
            RenderPuddleReflectionNPCs(beforePlayer: false);

            // Draw puddle ripples on top, unchanged
            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (var rippleSprite in DynamicReflections.puddleManager.puddleRippleSprites.ToList())
            {
                if (Utility.isOnScreen(rippleSprite.Position, 3 * 64))
                {
                    rippleSprite.draw(Game1.spriteBatch);
                }
            }
            Game1.spriteBatch.End();

            // Drop the render target
            SpriteBatchToolkit.StopRendering();
            Game1.graphics.GraphicsDevice.Clear(Game1.bgColor);
        }

        internal static void DrawFarmhandWaterReflections(bool beforePlayer = true, bool afterPlayer = true)
        {
            foreach (var farmhand in Game1.getOnlineFarmers())
            {
                if (farmhand == Game1.player)
                {
                    continue;
                }
                else if (beforePlayer is false && farmhand.Tile.Y <= Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (afterPlayer is false && farmhand.Tile.Y > Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (Utility.isOnScreen(farmhand.Position, 64 * 3) is false)
                {
                    continue;
                }

                DrawPlayerWaterReflection(farmhand);
            }
        }

        internal static void DrawPlayerWaterReflection(Farmer farmer)
        {
            // Cache what we’re going to touch so we can restore it
            var oldDirection = farmer.FacingDirection;
            var oldSprite = farmer.FarmerSprite;

            var currentWaterSettings = DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation);

            // Always draw the *real* player, just flip the screen with a matrix.
            if (currentWaterSettings.ReflectionDirection == Models.Settings.Direction.South)
            {
                // Flip vertically around the water line in screen space.
                var scale = Matrix.CreateScale(1f, -1f, 1f);

                // Pivot at the water reflection line (already computed in world space, convert to screen).
                float yOffset = farmer.IsSitting() ? 16f : 0f;
                float pivotY = Game1.GlobalToLocal(Game1.viewport, farmer.Position + currentWaterSettings.PlayerReflectionOffset * 64).Y;
                var position = Matrix.CreateTranslation(0f, (pivotY + yOffset) * 2f, 0f);

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

                farmer.FacingDirection = DynamicReflections.GetReflectedDirection(oldDirection, true);
                farmer.FarmerSprite = oldDirection == 0
                    ? DynamicReflections.mirrorReflectionSprite
                    : oldSprite;

                farmer.modData["FashionSense.Animation.FacingDirection"] = farmer.FacingDirection.ToString();
            }

            // IMPORTANT: No longer touch farmer.Position here.
            farmer.draw(Game1.spriteBatch);

            // Restore what changed
            farmer.FacingDirection = oldDirection;
            farmer.FarmerSprite = oldSprite;

            Game1.spriteBatch.End();
        }

        internal static void DrawFarmhandPuddleReflections(bool beforePlayer = true, bool afterPlayer = true)
        {
            foreach (var farmhand in Game1.getOnlineFarmers())
            {
                if (farmhand == Game1.player)
                {
                    continue;
                }
                else if (beforePlayer is false && farmhand.Tile.Y <= Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (afterPlayer is false && farmhand.Tile.Y > Game1.player.Tile.Y)
                {
                    continue;
                }
                else if (Utility.isOnScreen(farmhand.Position, 64 * 3) is false)
                {
                    continue;
                }

                DrawPlayerPuddleReflection(farmhand);
            }
        }

        internal static void DrawPlayerPuddleReflection(Farmer farmer)
        {
            var oldDirection = farmer.FacingDirection;
            var oldSprite = farmer.FarmerSprite;

            // Original world position
            var oldPosition = farmer.Position;

            // Where the reflection was previously drawn (world space)
            var worldOffset = DynamicReflections.currentPuddleSettings.ReflectionOffset * 64f;
            var targetWorld = oldPosition - worldOffset;

            // Convert both positions to screen space to build an equivalent translation
            var playerScreen = Game1.GlobalToLocal(Game1.viewport, oldPosition);
            var targetScreen = Game1.GlobalToLocal(Game1.viewport, targetWorld);
            var delta = targetScreen - playerScreen;

            // Same vertical flip & pivot as before (across the player's original local Y)
            float yOffset = farmer.IsSitting() ? 32f : 0f;
            var scale = Matrix.CreateScale(1f, -1f, 1f);
            var pivot = Matrix.CreateTranslation(0f, (playerScreen.Y + yOffset) * 2f, 0f);

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
            farmer.draw(Game1.spriteBatch);

            farmer.FacingDirection = oldDirection;
            farmer.FarmerSprite = oldSprite;

            Game1.spriteBatch.End();
        }

        internal static void DrawRenderedCharacters(bool isWavy = false)
        {

            if (DynamicReflections.modConfig.AreWaterReflectionsEnabled)
            {
                DynamicReflections.waterReflectionEffect.Parameters["ColorOverlay"].SetValue(DynamicReflections.modConfig.WaterReflectionSettings.ReflectionOverlay.ToVector4());
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, effect: isWavy ? DynamicReflections.waterReflectionEffect : null);

                // Draw the player
                Game1.spriteBatch.Draw(DynamicReflections.playerWaterReflectionRender, Vector2.Zero, DynamicReflections.modConfig.GetCurrentWaterSettings(Game1.currentLocation).ReflectionOverlay);

                Game1.spriteBatch.End();
            }

            if (DynamicReflections.modConfig.AreNPCReflectionsEnabled is true)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, effect: isWavy ? DynamicReflections.waterReflectionEffect : null);
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
