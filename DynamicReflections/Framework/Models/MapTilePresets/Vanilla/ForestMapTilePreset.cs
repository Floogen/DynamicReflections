using DynamicReflections.Framework.Models.Reflections;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.MapTilePresets.Vanilla
{
    internal class ForestMapTilePreset : MapTilePresetTemplate
    {
        public override string MapName { get; } = "Forest";
        public override List<ReflectableMapObject> MapObjects { get; } = new List<ReflectableMapObject>()
        {
            new ReflectableMapObject("Forest_North_Wood_Plank_Short", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 62, y: 70) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 63, y: 70) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 64, y: 70) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 65, y: 70) { Offset = new Vector2(0f, 1.65f) }
            }),
            new ReflectableMapObject("Forest_North_Wood_Plank_Long", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 77, y: 49) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 78, y: 49) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 79, y: 49) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 80, y: 49) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 81, y: 49) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 82, y: 49) { Offset = new Vector2(0f, 1.65f) },
            }),
        };
    }
}
