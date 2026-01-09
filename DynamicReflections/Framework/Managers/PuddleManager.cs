using DynamicReflections.Framework.Extensions;
using DynamicReflections.Framework.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Layers;
using xTile.Tiles;

namespace DynamicReflections.Framework.Managers
{
    internal class PuddleManager
    {
        private const int PUDDLES_POOL = 16;
        internal const int DEFAULT_PUDDLE_INDEX = -1;

        internal Dictionary<Farmer, double> puddleRippleCooldowns = new Dictionary<Farmer, double>();
        internal List<TemporaryAnimatedSprite> puddleRippleSprites = new List<TemporaryAnimatedSprite>();
        internal Dictionary<GameLocation, PuddleTile[,]> locationToPuddleTiles = new Dictionary<GameLocation, PuddleTile[,]>();
        private List<Point> _puddlePoints = new List<Point>();

        public void Reset()
        {
            locationToPuddleTiles = new Dictionary<GameLocation, PuddleTile[,]>();
        }

        public void Generate(GameLocation location, int percentOfDiggableTiles = 10, bool force = false)
        {
            puddleRippleSprites = new List<TemporaryAnimatedSprite>();
            if (location is null || location.Map is null)
            {
                return;
            }

            if (locationToPuddleTiles is null)
            {
                Reset();
            }
            else if (force is false && locationToPuddleTiles.ContainsKey(location) is true && locationToPuddleTiles[location] is not null)
            {
                return;
            }

            if (Game1.IsMasterGame)
            {
                GenerateByPercentage(location, percentOfDiggableTiles);
            }
            else
            {
                DynamicReflections.messageManager.RequestPuddleLocationTiles(location);
            }
        }

        private void GenerateByPercentage(GameLocation location, int percentOfDiggableTiles = 10)
        {
            var random = new Random((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed * 500 + Game1.ticks + DateTime.Now.Ticks));
            if (location.Map.GetLayer("Back") is var backLayer && backLayer is not null)
            {
                locationToPuddleTiles[location] = new PuddleTile[backLayer.LayerWidth, backLayer.LayerHeight];

                List<Point> diggableTiles = new List<Point>();
                for (int x = 0; x < backLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < backLayer.LayerHeight; y++)
                    {
                        if (backLayer.Tiles[x, y] is not null)
                        {
                            backLayer.Tiles[x, y].Properties["PuddleIndex"] = DEFAULT_PUDDLE_INDEX;
                            backLayer.Tiles[x, y].Properties["BigPuddleIndex"] = DEFAULT_PUDDLE_INDEX;

                            if (location.isTileLocationTotallyClearAndPlaceable(x, y) is false)
                            {
                                continue;
                            }

                            if (String.IsNullOrEmpty(location.doesTileHaveProperty(x, y, "Diggable", "Back")) is false && location.isTileHoeDirt(new Microsoft.Xna.Framework.Vector2(x, y)) is false)
                            {
                                diggableTiles.Add(new Point(x, y));
                            }
                            else
                            {
                                string stepType = location.doesTileHaveProperty(x, y, "Type", "Buildings");
                                if (stepType == null || stepType.Length < 1)
                                {
                                    stepType = location.doesTileHaveProperty(x, y, "Type", "Back");
                                }

                                if (stepType == "Dirt" || stepType == "Stone")
                                {
                                    diggableTiles.Add(new Point(x, y));
                                }
                            }
                        }
                    }
                }

                if (DynamicReflections.currentPuddleSettings.ShouldGeneratePuddles is false || percentOfDiggableTiles == 0)
                {
                    return;
                }

                for (int i = 0; i < diggableTiles.Count / percentOfDiggableTiles; i++)
                {
                    var tilePosition = GetRandomTile(random, diggableTiles);
                    if (IsBigPuddleTile(backLayer, tilePosition))
                    {
                        continue;
                    }

                    var puddleIndex = random.Next(DEFAULT_PUDDLE_INDEX, PUDDLES_POOL);
                    backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleIndex"] = puddleIndex;
                    backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleEffect"] = random.Next(0, 4);
                    backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleRotation"] = MathHelper.ToRadians(90 * random.Next(0, 4));

                    bool shouldAttemptBigPuddle = random.NextDouble() < DynamicReflections.currentPuddleSettings.BigPuddleChance / 100f;
                    if (shouldAttemptBigPuddle)
                    {
                        bool canMakeBigPuddle = true;
                        if (diggableTiles.FirstOrDefault(d => d.X == tilePosition.X + 1 && d.Y == tilePosition.Y) is var xOffsetPoint && (xOffsetPoint == default(Point) || IsBigPuddleTile(backLayer, xOffsetPoint)))
                        {
                            canMakeBigPuddle = false;
                        }
                        if (diggableTiles.FirstOrDefault(d => d.X == tilePosition.X + 1 && d.Y == tilePosition.Y + 1) is var yOffsetPoint && (yOffsetPoint == default(Point) || IsBigPuddleTile(backLayer, yOffsetPoint)))
                        {
                            canMakeBigPuddle = false;
                        }
                        if (diggableTiles.FirstOrDefault(d => d.X == tilePosition.X && d.Y == tilePosition.Y + 1) is var xyOffsetPoint && (xyOffsetPoint == default(Point) || IsBigPuddleTile(backLayer, xyOffsetPoint)))
                        {
                            canMakeBigPuddle = false;
                        }

                        if (canMakeBigPuddle)
                        {
                            var puddleEffect = random.Next(0, 4);
                            var puddleRotation = 0;
                            var adjustedPuddleIndex = random.Next(0, PUDDLES_POOL);
                            if (adjustedPuddleIndex % 2 != 0)
                            {
                                adjustedPuddleIndex -= 1;
                            }

                            backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleIndex"] = puddleEffect is (0 or 1) ? adjustedPuddleIndex : adjustedPuddleIndex + 1;
                            backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["BigPuddleIndex"] = puddleEffect is (0 or 2) ? 1 : 2;
                            backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleEffect"] = puddleEffect;
                            backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleRotation"] = puddleRotation;
                            locationToPuddleTiles[location][tilePosition.X, tilePosition.Y] = new PuddleTile() 
                            {
                                PuddleIndex = backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleIndex"],
                                BigPuddleIndex = backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["BigPuddleIndex"],
                                PuddleEffect = puddleEffect,
                                PuddleRotation = puddleRotation
                            };

                            backLayer.Tiles[tilePosition.X + 1, tilePosition.Y].Properties["PuddleIndex"] = puddleEffect is (0 or 1) ? adjustedPuddleIndex : adjustedPuddleIndex + 1;
                            backLayer.Tiles[tilePosition.X + 1, tilePosition.Y].Properties["BigPuddleIndex"] = puddleEffect is (0 or 2) ? 2 : 1;
                            backLayer.Tiles[tilePosition.X + 1, tilePosition.Y].Properties["PuddleEffect"] = puddleEffect;
                            backLayer.Tiles[tilePosition.X + 1, tilePosition.Y].Properties["PuddleRotation"] = puddleRotation;
                            locationToPuddleTiles[location][tilePosition.X + 1, tilePosition.Y] = new PuddleTile()
                            {
                                PuddleIndex = backLayer.Tiles[tilePosition.X + 1, tilePosition.Y].Properties["PuddleIndex"],
                                BigPuddleIndex = backLayer.Tiles[tilePosition.X + 1, tilePosition.Y].Properties["BigPuddleIndex"],
                                PuddleEffect = puddleEffect,
                                PuddleRotation = puddleRotation
                            };

                            backLayer.Tiles[tilePosition.X, tilePosition.Y + 1].Properties["PuddleIndex"] = puddleEffect is (0 or 1) ? adjustedPuddleIndex + 1 : adjustedPuddleIndex;
                            backLayer.Tiles[tilePosition.X, tilePosition.Y + 1].Properties["BigPuddleIndex"] = puddleEffect is (0 or 2) ? 1 : 2;
                            backLayer.Tiles[tilePosition.X, tilePosition.Y + 1].Properties["PuddleEffect"] = puddleEffect;
                            backLayer.Tiles[tilePosition.X, tilePosition.Y + 1].Properties["PuddleRotation"] = puddleRotation;
                            locationToPuddleTiles[location][tilePosition.X, tilePosition.Y + 1] = new PuddleTile()
                            {
                                PuddleIndex = backLayer.Tiles[tilePosition.X, tilePosition.Y + 1].Properties["PuddleIndex"],
                                BigPuddleIndex = backLayer.Tiles[tilePosition.X, tilePosition.Y + 1].Properties["BigPuddleIndex"],
                                PuddleEffect = puddleEffect,
                                PuddleRotation = puddleRotation
                            };

                            backLayer.Tiles[tilePosition.X + 1, tilePosition.Y + 1].Properties["PuddleIndex"] = puddleEffect is (0 or 1) ? adjustedPuddleIndex + 1 : adjustedPuddleIndex;
                            backLayer.Tiles[tilePosition.X + 1, tilePosition.Y + 1].Properties["BigPuddleIndex"] = puddleEffect is (0 or 2) ? 2 : 1;
                            backLayer.Tiles[tilePosition.X + 1, tilePosition.Y + 1].Properties["PuddleEffect"] = puddleEffect;
                            backLayer.Tiles[tilePosition.X + 1, tilePosition.Y + 1].Properties["PuddleRotation"] = puddleRotation;
                            locationToPuddleTiles[location][tilePosition.X + 1, tilePosition.Y + 1] = new PuddleTile()
                            {
                                PuddleIndex = backLayer.Tiles[tilePosition.X + 1, tilePosition.Y + 1].Properties["PuddleIndex"],
                                BigPuddleIndex = backLayer.Tiles[tilePosition.X + 1, tilePosition.Y + 1].Properties["BigPuddleIndex"],
                                PuddleEffect = puddleEffect,
                                PuddleRotation = puddleRotation
                            };
                        }
                        else
                        {
                            locationToPuddleTiles[location][tilePosition.X, tilePosition.Y] = new PuddleTile()
                            {
                                PuddleIndex = backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleIndex"],
                                PuddleEffect = backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleEffect"],
                                PuddleRotation = backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleRotation"]
                            };
                        }
                    }
                    else
                    {
                        locationToPuddleTiles[location][tilePosition.X, tilePosition.Y] = new PuddleTile()
                        {
                            PuddleIndex = backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleIndex"],
                            PuddleEffect = backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleEffect"],
                            PuddleRotation = backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["PuddleRotation"]
                        };
                    }
                }
            }
        }

        private bool IsBigPuddleTile(Layer backLayer, Point tilePosition)
        {
            if (backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties.ContainsKey("BigPuddleIndex") && Int32.TryParse(backLayer.Tiles[tilePosition.X, tilePosition.Y].Properties["BigPuddleIndex"], out int value) && value != DEFAULT_PUDDLE_INDEX)
            {
                return true;
            }

            return false;
        }

        private Point GetRandomTile(Random random, List<Point> tilePoints)
        {
            return tilePoints[random.Next(tilePoints.Count)];
        }

        public bool DoesPointHaveNeighbor(GameLocation location, Point point)
        {
            var offsetPoint = new Point(point.X - 1, point.Y);
            if (IsTilePuddle(location, offsetPoint.X, offsetPoint.Y) is true)
            {
                return true;
            }

            offsetPoint = new Point(point.X + 1, point.Y);
            if (IsTilePuddle(location, offsetPoint.X, offsetPoint.Y) is true)
            {
                return true;
            }

            offsetPoint = new Point(point.X, point.Y - 1);
            if (IsTilePuddle(location, offsetPoint.X, offsetPoint.Y) is true)
            {
                return true;
            }

            offsetPoint = new Point(point.X, point.Y + 1);
            if (IsTilePuddle(location, offsetPoint.X, offsetPoint.Y) is true)
            {
                return true;
            }

            return false;
        }

        public bool IsTilePuddle(GameLocation location, int x, int y)
        {
            if (locationToPuddleTiles.ContainsKey(location) is false || locationToPuddleTiles[location] is null || x < 0 || y < 0 || locationToPuddleTiles[location].GetLength(0) <= x || locationToPuddleTiles[location].GetLength(1) <= y || locationToPuddleTiles[location][x, y] is null)
            {
                return false;
            }

            return locationToPuddleTiles[location][x, y].IsValid();
        }

        public List<Point> GetPuddleTiles(GameLocation location, bool limitToView = false)
        {
            _puddlePoints.Clear();

            if (locationToPuddleTiles.ContainsKey(location) is false)
            {
                return _puddlePoints;
            }

            int tileWidth = Game1.pixelZoom * 16;
            int tileHeight = Game1.pixelZoom * 16;
            int tileXMin = ((Game1.viewport.X >= 0) ? (Game1.viewport.X / tileWidth) : ((Game1.viewport.X - tileWidth + 1) / tileWidth));
            int tileYMin = ((Game1.viewport.Y >= 0) ? (Game1.viewport.Y / tileHeight) : ((Game1.viewport.Y - tileHeight + 1) / tileHeight));
            if (tileXMin < 0)
            {
                tileXMin = 0;
            }
            if (tileYMin < 0)
            {
                tileYMin = 0;
            }
            int tileColumns = 1 + (Game1.viewport.Size.Width - 1) / tileWidth;
            int tileRows = 1 + (Game1.viewport.Size.Height - 1) / tileHeight;
            int tileXMax = tileXMin + tileColumns;
            int tileYMax = tileYMin + tileRows;

            if (limitToView)
            {
                for (int x = tileXMin; x < locationToPuddleTiles[location].GetLength(0) && x < tileXMax; x++)
                {
                    for (int y = tileYMin; y < locationToPuddleTiles[location].GetLength(1) && y < tileYMax; y++)
                    {
                        if (locationToPuddleTiles[location][x, y] is not null && locationToPuddleTiles[location][x, y].IsValid())
                        {
                            _puddlePoints.Add(new Point(x, y));
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < locationToPuddleTiles[location].GetLength(0); x++)
                {
                    for (int y = 0; y < locationToPuddleTiles[location].GetLength(1); y++)
                    {
                        if (locationToPuddleTiles[location][x, y] is not null && locationToPuddleTiles[location][x, y].IsValid())
                        {
                            _puddlePoints.Add(new Point(x, y));
                        }
                    }
                }
            }

            return _puddlePoints;
        }

        public void Sync(GameLocation location, PuddleTile[,] puddleTiles)
        {
            if (location is null || location.Map is null || (location.Map.GetLayer("Back") is var backLayer && backLayer is null))
            {
                return;
            }

            locationToPuddleTiles[location] = puddleTiles;
            for (int x = 0; x < locationToPuddleTiles[location].GetLength(0); x++)
            {
                for (int y = 0; y < locationToPuddleTiles[location].GetLength(1); y++)
                {
                    if (x < puddleTiles.GetLength(0) && y < puddleTiles.GetLength(1) && locationToPuddleTiles[location][x, y] is not null)
                    {
                        backLayer.Tiles[x, y].Properties["PuddleIndex"] = puddleTiles[x, y].PuddleIndex;
                        backLayer.Tiles[x, y].Properties["BigPuddleIndex"] = puddleTiles[x, y].BigPuddleIndex;
                        backLayer.Tiles[x, y].Properties["PuddleEffect"] = puddleTiles[x, y].PuddleEffect;
                        backLayer.Tiles[x, y].Properties["PuddleRotation"] = puddleTiles[x, y].PuddleRotation;
                    }
                }
            }
        }
    }
}
