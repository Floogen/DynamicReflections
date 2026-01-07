using DynamicReflections.Framework.Models.Reflections;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.MapTilePresets.Vanilla
{
    internal class IslandNorthMapTilePreset : MapTilePresetTemplate
    {
        public override string MapName { get; } = "IslandNorth";
        public override List<ReflectableMapObject> MapObjects { get; } = new List<ReflectableMapObject>()
        {
            new ReflectableMapObject("IslandNorth_North_Wood_Plank_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 12, y: 49) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 13, y: 49) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 14, y: 49) { Offset = new Vector2(0f, 1.65f) },
            }),
            new ReflectableMapObject("IslandNorth_North_Wood_Plank_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 12, y: 63) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 13, y: 63) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 14, y: 63) { Offset = new Vector2(0f, 1.65f) },
            }),
        };
    }
}
