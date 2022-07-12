using DynamicReflections.Framework.Managers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace DynamicReflections.Framework.Patches.Tools
{
    internal class GameLocationPatch : PatchTemplate
    {
        private readonly Type _type = typeof(GameLocation);
        private static double _elapsedMilliseconds;
        private static int _cooldownCounter;
        private static List<Point> _puddlesOnCooldown = new List<Point>();

        internal GameLocationPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_type, nameof(GameLocation.UpdateWhenCurrentLocation), new[] { typeof(GameTime) }), postfix: new HarmonyMethod(GetType(), nameof(UpdateWhenCurrentLocationPostfix)));
        }

        private static void UpdateWhenCurrentLocationPostfix(GameLocation __instance, GameTime time)
        {
            foreach (var rippleSprite in DynamicReflections.puddleManager.puddleRippleSprites.ToList())
            {
                if (rippleSprite.update(time))
                {
                    DynamicReflections.puddleManager.puddleRippleSprites.Remove(rippleSprite);
                }
            }

            var playerTilePosition = Game1.player.getTileLocationPoint();
            if (__instance.lastTouchActionLocation.Equals(Vector2.Zero) && Int32.TryParse(__instance.doesTileHaveProperty(playerTilePosition.X, playerTilePosition.Y, "PuddleIndex", "Back"), out int puddleIndex) && puddleIndex != PuddleManager.DEFAULT_PUDDLE_INDEX)
            {
                float xOffset = Game1.player.FacingDirection == 3 ? 64f : 0f;
                float yOffset = Game1.player.FacingDirection == 0 ? 64f : 0f;
                switch (Game1.player.FacingDirection)
                {
                    case 0:
                    case 2:
                        xOffset += 20f;
                        break;
                    case 1:
                    case 3:
                        yOffset += 20f;
                        break;
                }

                TemporaryAnimatedSprite splashSprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), Game1.random.Next(50, 100), 9, 1, new Vector2(Game1.player.getStandingX() - xOffset, Game1.player.getStandingY() - yOffset), flicker: false, flipped: false, 0f, 0f, DynamicReflections.currentPuddleSettings.RippleColor, 1f, 0f, 0f, 0f);
                splashSprite.acceleration = new Vector2(Game1.player.xVelocity, Game1.player.yVelocity);
                DynamicReflections.puddleManager.puddleRippleSprites.Add(splashSprite);

                TemporaryAnimatedSprite dropletSprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(2 * 64, 18 * 64, 64, 64), Game1.random.Next(75, 125), 5, 1, new Vector2(playerTilePosition.X, playerTilePosition.Y - 0.5f) * 64f, flicker: false, flipped: false, 0f, 0f, new Color(141, 181, 216, 91), 1f, 0f, 0f, 0f);
                splashSprite.acceleration = new Vector2(Game1.player.xVelocity, Game1.player.yVelocity);
                __instance.temporarySprites.Add(dropletSprite);

                if (DynamicReflections.currentPuddleSettings.ShouldPlaySplashSound)
                {
                    __instance.playSound(Game1.random.NextDouble() > 0.5 ? "slosh" : "waterSlosh");
                }
                __instance.lastTouchActionLocation = new Vector2(playerTilePosition.X, playerTilePosition.Y);
            }

            _elapsedMilliseconds += time.ElapsedGameTime.TotalMilliseconds;
            if (_elapsedMilliseconds > DynamicReflections.currentPuddleSettings.MillisecondsBetweenRaindropSplashes)
            {
                _elapsedMilliseconds = 0;

                if (DynamicReflections.currentPuddleSettings.ShouldRainSplashPuddles && Game1.IsRainingHere(__instance))
                {
                    var puddles = DynamicReflections.puddleManager.GetPuddleTiles(__instance, limitToView: true);
                    if (puddles.Count > 0)
                    {
                        for (int i = 0; i < Game1.random.Next(1, 5); i++)
                        {
                            var puddleTile = puddles[Game1.random.Next(puddles.Count)];
                            if (_puddlesOnCooldown.Any(p => p.X == puddleTile.X && p.Y == puddleTile.Y) is false && Game1.random.NextDouble() > 0.5)
                            {
                                GenerateRipple(__instance, puddleTile);
                                _puddlesOnCooldown.Add(puddleTile);
                            }
                        }
                    }

                    // Iterate the cooldown counter
                    _cooldownCounter -= 1;
                    if (_cooldownCounter < 0)
                    {
                        _puddlesOnCooldown.Clear();
                        _cooldownCounter = 5;
                    }
                }
            }
        }

        private static void GenerateRipple(GameLocation location, Point puddleTile, bool playSound = false)
        {
            TemporaryAnimatedSprite splashSprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), Game1.random.Next(50, 100), 9, 1, new Vector2(puddleTile.X, puddleTile.Y) * 64f, flicker: false, flipped: false, 0f, 0f, DynamicReflections.currentPuddleSettings.RippleColor, 1f, 0f, 0f, 0f);
            DynamicReflections.puddleManager.puddleRippleSprites.Add(splashSprite);

            TemporaryAnimatedSprite dropletSprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(2 * 64, 18 * 64, 64, 64), Game1.random.Next(75, 125), 5, 1, new Vector2(puddleTile.X, puddleTile.Y - 0.5f) * 64f, flicker: false, flipped: false, 0f, 0f, new Color(141, 181, 216, 91), 1f, 0f, 0f, 0f);
            location.temporarySprites.Add(dropletSprite);

            if (playSound)
            {
                location.playSound(Game1.random.NextDouble() > 0.5 ? "slosh" : "waterSlosh");
            }
        }
    }
}
