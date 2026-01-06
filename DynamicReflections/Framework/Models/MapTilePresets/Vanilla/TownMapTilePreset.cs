using DynamicReflections.Framework.Models.Reflections;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.MapTilePresets.Vanilla
{
    internal class TownMapTilePreset : MapTilePresetTemplate
    {
        public override string MapName { get; } = "Town";
        public override List<ReflectableMapObject> MapObjects { get; } = new List<ReflectableMapObject>()
        {
            new ReflectableMapObject("Town_SouthRiver_Tree_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 43, y: 102),
                new ReflectableMapTile(layerName: "Buildings", x: 44, y: 102),
                new ReflectableMapTile(layerName: "Buildings", x: 43, y: 101),
                new ReflectableMapTile(layerName: "Buildings", x: 44, y: 101),

                new ReflectableMapTile(layerName: "Front", x: 43, y: 100),
                new ReflectableMapTile(layerName: "Front", x: 44, y: 100),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 42, y: 99),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 43, y: 99),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 44, y: 99),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 45, y: 99),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 42, y: 98),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 43, y: 98),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 44, y: 98),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 45, y: 98),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 42, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 43, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 44, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 45, y: 97),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 42, y: 96),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 43, y: 96),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 44, y: 96),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 45, y: 96)
            }),
            new ReflectableMapObject("Town_EastRiver_Bridge_Under_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Back", x: 75, y: 95) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 76, y: 95) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 77, y: 95) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 78, y: 95) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 79, y: 95) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 80, y: 95) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 81, y: 95) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 82, y: 95) { Offset = new Vector2(0f, 2.2f) }
            }),
            new ReflectableMapObject("Town_EastRiver_Bridge_Rail_1", new List<ReflectableMapTile>()
            {
                /*
                new ReflectableMapTile(layerName: "Front", x: 75, y: 95) { Offset = new Vector2(0f, 2.9f) },
                new ReflectableMapTile(layerName: "Front", x: 76, y: 95) { Offset = new Vector2(0f, 2.9f) },
                new ReflectableMapTile(layerName: "Front", x: 77, y: 95) { Offset = new Vector2(0f, 2.9f) },
                new ReflectableMapTile(layerName: "Front", x: 78, y: 95) { Offset = new Vector2(0f, 2.9f) },
                new ReflectableMapTile(layerName: "Front", x: 79, y: 95) { Offset = new Vector2(0f, 2.9f) },
                new ReflectableMapTile(layerName: "Front", x: 80, y: 95) { Offset = new Vector2(0f, 2.9f) },
                new ReflectableMapTile(layerName: "Front", x: 81, y: 95) { Offset = new Vector2(0f, 2.9f) },
                new ReflectableMapTile(layerName: "Front", x: 82, y: 95) { Offset = new Vector2(0f, 2.9f) }
                */
                
                new ReflectableMapTile(layerName: "Buildings", x: 75, y: 96) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 76, y: 96) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 77, y: 96) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 78, y: 96) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 79, y: 96) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 80, y: 96) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 81, y: 96) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 82, y: 96) { Offset = new Vector2(0f, 1.6f) },

            }),
            new ReflectableMapObject("Town_EastRiver_Bridge_Under_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Back", x: 70, y: 54) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 71, y: 54) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 72, y: 54) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 73, y: 54) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 74, y: 54) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 75, y: 54) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 76, y: 54) { Offset = new Vector2(0f, 2.2f) },
                new ReflectableMapTile(layerName: "Back", x: 77, y: 54) { Offset = new Vector2(0f, 2.2f) }
            }),
            new ReflectableMapObject("Town_EastRiver_Bridge_Rail_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 70, y: 55) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 71, y: 55) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 72, y: 55) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 73, y: 55) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 74, y: 55) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 75, y: 55) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 76, y: 55) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 77, y: 55) { Offset = new Vector2(0f, 1.6f) },

            }),
            new ReflectableMapObject("Town_EastRiver_Wood_Plank", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 92, y: 13) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 93, y: 13) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 94, y: 13) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 95, y: 13) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 96, y: 13) { Offset = new Vector2(0f, 1.6f) }
            }),
        };
    }
}
