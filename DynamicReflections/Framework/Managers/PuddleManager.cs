using StardewValley;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Layers;
using xTile.Tiles;

namespace DynamicReflections.Framework.Managers
{
    internal class PuddleManager
    {
        private const int PUDDLES_POOL = 10;
        internal const int DEFAULT_PUDDLE_INDEX = -1;

        internal List<TemporaryAnimatedSprite> puddleRippleSprites = new List<TemporaryAnimatedSprite>();
        private Dictionary<GameLocation, bool[,]> _locationToPuddleTiles;

        public void Reset()
        {
            _locationToPuddleTiles = new Dictionary<GameLocation, bool[,]>();
        }

        public void Generate(GameLocation location)
        {
            puddleRippleSprites = new List<TemporaryAnimatedSprite>();
            if (location is null || location.Map is null)
            {
                return;
            }

            if (_locationToPuddleTiles is null)
            {
                Reset();
            }
            else if (_locationToPuddleTiles.ContainsKey(location) is true && _locationToPuddleTiles[location] is not null)
            {
                return;
            }

            GenerateByPercentage(location);
        }

        private void GenerateByPercentage(GameLocation location, int percentOfDiggableTiles = 10)
        {
            var random = new Random((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed * 500 + Game1.ticks + DateTime.Now.Ticks));
            if (location.Map.GetLayer("Back") is var backLayer && backLayer is not null)
            {
                _locationToPuddleTiles[location] = new bool[backLayer.LayerWidth, backLayer.LayerHeight];

                List<Point> diggableTiles = new List<Point>();
                for (int x = 0; x < backLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < backLayer.LayerHeight; y++)
                    {
                        if (backLayer.Tiles[x, y] is not null)
                        {
                            backLayer.Tiles[x, y].Properties["PuddleIndex"] = DEFAULT_PUDDLE_INDEX;

                            if (backLayer.Tiles[x, y].TileIndexProperties.TryGetValue("Diggable", out _))
                            {
                                diggableTiles.Add(new Point(x, y));
                            }
                        }
                    }
                }

                for (int i = 0; i < diggableTiles.Count / percentOfDiggableTiles; i++)
                {
                    var tilePosition = GetRandomTile(random, diggableTiles);
                    if (String.IsNullOrEmpty(location.doesTileHaveProperty(tilePosition.X, tilePosition.Y, "Diggable", "Back")) is false && location.isTileHoeDirt(new Microsoft.Xna.Framework.Vector2(tilePosition.X, tilePosition.Y)) is false && location.isTileLocationTotallyClearAndPlaceable(tilePosition.X, tilePosition.Y) && DoesPointHavePreviousNeighbor(location, new Point(tilePosition.X, tilePosition.Y)) is false)
                    {
                        backLayer.Tiles[(int)tilePosition.X, (int)tilePosition.Y].Properties["PuddleIndex"] = random.Next(DEFAULT_PUDDLE_INDEX, PUDDLES_POOL);
                        backLayer.Tiles[(int)tilePosition.X, (int)tilePosition.Y].Properties["PuddleEffect"] = random.Next(0, 4);
                        backLayer.Tiles[(int)tilePosition.X, (int)tilePosition.Y].Properties["PuddleRotation"] = Microsoft.Xna.Framework.MathHelper.ToRadians(90 * random.Next(0, 4));
                    }
                }
            }
        }

        private Point GetRandomTile(Random random, List<Point> tilePoints)
        {
            return tilePoints[random.Next(tilePoints.Count)];
        }

        private void GeneratePerTile(GameLocation location)
        {
            var random = new Random((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed * 500 + Game1.ticks + DateTime.Now.Ticks));
            if (location.Map.GetLayer("Back") is var backLayer && backLayer is not null)
            {
                _locationToPuddleTiles[location] = new bool[backLayer.LayerWidth, backLayer.LayerHeight];

                for (int x = 0; x < backLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < backLayer.LayerHeight; y++)
                    {
                        var point = new Point(x, y);
                        if (backLayer.Tiles[x, y] is not null)
                        {
                            var puddleIndex = DEFAULT_PUDDLE_INDEX;
                            if (backLayer.Tiles[x, y].TileIndexProperties.TryGetValue("Diggable", out _) && DoesPointHavePreviousNeighbor(location, point) is false && location.isTileLocationTotallyClearAndPlaceable(x, y))
                            {
                                puddleIndex = random.NextDouble() < 0.95 ? DEFAULT_PUDDLE_INDEX : random.Next(DEFAULT_PUDDLE_INDEX, PUDDLES_POOL);
                                _locationToPuddleTiles[location][x, y] = puddleIndex != DEFAULT_PUDDLE_INDEX;
                            }

                            backLayer.Tiles[x, y].Properties["PuddleIndex"] = puddleIndex;
                        }
                    }
                }
            }
        }

        public bool DoesPointHavePreviousNeighbor(GameLocation location, Point point)
        {
            var offsetPoint = new Point(point.X - 1, point.Y);
            if (GetPuddleTileIndex(location, offsetPoint.X, offsetPoint.Y) is true)
            {
                return true;
            }

            offsetPoint = new Point(point.X + 1, point.Y);
            if (GetPuddleTileIndex(location, offsetPoint.X, offsetPoint.Y) is true)
            {
                return true;
            }

            offsetPoint = new Point(point.X, point.Y - 1);
            if (GetPuddleTileIndex(location, offsetPoint.X, offsetPoint.Y) is true)
            {
                return true;
            }

            offsetPoint = new Point(point.X, point.Y + 1);
            if (GetPuddleTileIndex(location, offsetPoint.X, offsetPoint.Y) is true)
            {
                return true;
            }

            return false;
        }

        public bool GetPuddleTileIndex(GameLocation location, int x, int y)
        {
            if (_locationToPuddleTiles[location] is null || x < 0 || y < 0 || _locationToPuddleTiles[location].GetLength(0) <= x || _locationToPuddleTiles[location].GetLength(1) <= y)
            {
                return false;
            }

            return _locationToPuddleTiles[location][x, y];
        }
    }
}
