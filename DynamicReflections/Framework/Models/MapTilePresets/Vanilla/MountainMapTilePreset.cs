using DynamicReflections.Framework.Models.Reflections;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.MapTilePresets.Vanilla
{
    internal class MountainMapTilePreset : MapTilePresetTemplate
    {
        public override string MapName { get; } = "Mountain";
        public override List<ReflectableMapObject> MapObjects { get; } = new List<ReflectableMapObject>()
        {
            new ReflectableMapObject("Mountain_North_Wood_Plank_Short", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 46, y: 7) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 47, y: 7) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 48, y: 7) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 49, y: 7) { Offset = new Vector2(0f, 1.6f) },
            }),
            new ReflectableMapObject("Mountain_North_Wood_Plank_Long", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 61, y: 21) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 62, y: 21) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 63, y: 21) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 64, y: 21) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 65, y: 21) { Offset = new Vector2(0f, 1.65f) }
            }),
        };
    }
}
