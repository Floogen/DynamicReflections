using DynamicReflections.Framework.Models.Reflections;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.MapTilePresets.Vanilla
{
    internal class IslandWestMapTilePreset : MapTilePresetTemplate
    {
        public override string MapName { get; } = "IslandWest";
        public override List<ReflectableMapObject> MapObjects { get; } = new List<ReflectableMapObject>()
        {
            new ReflectableMapObject("IslandWest_North_Wood_Plank_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 64, y: 51) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 65, y: 51) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 66, y: 51) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 67, y: 51) { Offset = new Vector2(0f, 1.65f) },
            }),
            new ReflectableMapObject("IslandWest_North_Wood_Plank_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 58, y: 60) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 59, y: 60) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 60, y: 60) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 61, y: 60) { Offset = new Vector2(0f, 1.65f) },
            }),
            new ReflectableMapObject("IslandWest_North_Wood_Plank_3", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 52, y: 80) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 53, y: 80) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 54, y: 80) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 55, y: 80) { Offset = new Vector2(0f, 1.65f) },
            }),
        };
    }
}
