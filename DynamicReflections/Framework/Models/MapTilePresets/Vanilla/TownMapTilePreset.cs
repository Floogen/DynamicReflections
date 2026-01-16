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
            new ReflectableMapObject("Town_SouthRiver_SpringTree_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 25, y: 97),
                new ReflectableMapTile(layerName: "Buildings", x: 26, y: 97),
                new ReflectableMapTile(layerName: "Buildings", x: 25, y: 96),
                new ReflectableMapTile(layerName: "Buildings", x: 26, y: 96),

                new ReflectableMapTile(layerName: "Front", x: 25, y: 95),
                new ReflectableMapTile(layerName: "Front", x: 26, y: 95),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 23, y: 94),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 24, y: 94),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 25, y: 94),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 26, y: 94),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 27, y: 94),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 28, y: 94),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 23, y: 93),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 24, y: 93),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 25, y: 93),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 26, y: 93),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 27, y: 93),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 28, y: 93),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 23, y: 92),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 24, y: 92),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 25, y: 92),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 26, y: 92),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 27, y: 92),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 28, y: 92),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 24, y: 91),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 25, y: 91),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 26, y: 91),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 27, y: 91)
            }),
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
            new ReflectableMapObject("Town_EastRiver_StaticBush_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 58, y: 99),
                new ReflectableMapTile(layerName: "Buildings", x: 59, y: 99),
                new ReflectableMapTile(layerName: "Buildings", x: 60, y: 99),

                new ReflectableMapTile(layerName: "Front", x: 58, y: 98),
                new ReflectableMapTile(layerName: "Front", x: 59, y: 98),
                new ReflectableMapTile(layerName: "Front", x: 60, y: 98),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 58, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 59, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 60, y: 97)
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
            new ReflectableMapObject("Town_SouthRiver_SpringTree_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 88, y: 100),
                new ReflectableMapTile(layerName: "Buildings", x: 89, y: 100),
                new ReflectableMapTile(layerName: "Buildings", x: 88, y: 99),
                new ReflectableMapTile(layerName: "Buildings", x: 89, y: 99),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 88, y: 98),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 89, y: 98),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 86, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 87, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 88, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 89, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 90, y: 97),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 91, y: 97),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 86, y: 96),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 87, y: 96),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 88, y: 96),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 89, y: 96),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 90, y: 96),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 91, y: 96),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 86, y: 95),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 87, y: 95),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 88, y: 95),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 89, y: 95),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 90, y: 95),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 91, y: 95),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 87, y: 94),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 88, y: 94),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 89, y: 94),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 90, y: 94)
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
                new ReflectableMapTile(layerName: "Buildings", x: 96, y: 13) { Offset = new Vector2(0f, 1.6f) },
                new ReflectableMapTile(layerName: "Buildings", x: 97, y: 13) { Offset = new Vector2(0f, 1.6f) }
            }),
        };
    }
}
