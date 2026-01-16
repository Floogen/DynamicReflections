using DynamicReflections.Framework.Models.Reflections;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.MapTilePresets.Vanilla
{
    internal class BeachNightMarketMapTilePreset : MapTilePresetTemplate
    {
        public override string MapName { get; } = "BeachNightMarket";
        public override List<ReflectableMapObject> MapObjects { get; } = new List<ReflectableMapObject>()
        {
            new ReflectableMapObject("LonlyStone", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Front", x: 4, y: 26),
                new ReflectableMapTile(layerName: "Front", x: 5, y: 26),
                new ReflectableMapTile(layerName: "Front", x: 6, y: 26),

                new ReflectableMapTile(layerName: "Front", x: 4, y: 25),
                new ReflectableMapTile(layerName: "Front", x: 5, y: 25),
                new ReflectableMapTile(layerName: "Front", x: 6, y: 25),

                new ReflectableMapTile(layerName: "Front", x: 4, y: 24),
                new ReflectableMapTile(layerName: "Front", x: 5, y: 24),
                new ReflectableMapTile(layerName: "Front", x: 6, y: 24)
            }),
            new ReflectableMapObject("BrokenBeachBridge", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Back", x: 57, y: 13) { Offset = new Vector2(0f, 1.2f) },
                new ReflectableMapTile(layerName: "Buildings", x: 58, y: 13) { Offset = new Vector2(0f, 1.2f) },
                new ReflectableMapTile(layerName: "Buildings", x: 61, y: 13) { Offset = new Vector2(0f, 1.2f) },
                new ReflectableMapTile(layerName: "Back", x: 62, y: 13) { Offset = new Vector2(0f, 1.2f) },

                //Needs a fixed bridge
                new ReflectableMapTile(layerName: "Buildings", x: 59, y: 13) { Offset = new Vector2(0f, 1.2f) },
                new ReflectableMapTile(layerName: "Buildings", x: 60, y: 13) { Offset = new Vector2(0f, 1.2f) }
            }),
            new ReflectableMapObject("Street_Lamp_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 44, y: 34) { Offset = new Vector2(0f, 2.5f) },
                new ReflectableMapTile(layerName: "Buildings", x: 44, y: 33) { Offset = new Vector2(0f, 2.5f) },
                new ReflectableMapTile(layerName: "Front", x: 44, y: 32) { Offset = new Vector2(0f, 2.5f) },
                new ReflectableMapTile(layerName: "Front", x: 44, y: 31) { Offset = new Vector2(0f, 2.5f) },
            }),
            new ReflectableMapObject("Street_Lamp_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 90, y: 38) { Offset = new Vector2(0f, 2.5f) },
                new ReflectableMapTile(layerName: "Buildings", x: 90, y: 37) { Offset = new Vector2(0f, 2.5f) },
                new ReflectableMapTile(layerName: "Front", x: 90, y: 36) { Offset = new Vector2(0f, 2.5f) },
                new ReflectableMapTile(layerName: "Front", x: 90, y: 35) { Offset = new Vector2(0f, 2.5f) },
            }),
            new ReflectableMapObject("Willy_Fish_Shop", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 28, y: 33) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 29, y: 33) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 30, y: 33) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 31, y: 33) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 32, y: 33) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 33, y: 33) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 34, y: 33) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 35, y: 33) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Buildings", x: 28, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 29, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 30, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 31, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 32, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 33, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 34, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 35, y: 32) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Buildings", x: 28, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 29, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 30, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 31, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 32, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 33, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 34, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Buildings", x: 35, y: 31) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Front", x: 27, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 28, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 29, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 30, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 31, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 32, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 33, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 34, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 35, y: 32) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 36, y: 32) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Front", x: 27, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 28, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 29, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 30, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 31, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 32, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 33, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 34, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 35, y: 31) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 36, y: 31) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Front", x: 27, y: 30) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 28, y: 30) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 29, y: 30) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 30, y: 30) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 31, y: 30) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 32, y: 30) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 33, y: 30) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 34, y: 30) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 35, y: 30) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 36, y: 30) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Front", x: 27, y: 29) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 28, y: 29) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 29, y: 29) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 30, y: 29) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 31, y: 29) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 32, y: 29) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 33, y: 29) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 34, y: 29) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 35, y: 29) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 36, y: 29) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Front", x: 27, y: 28) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 28, y: 28) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 29, y: 28) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 30, y: 28) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 31, y: 28) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 32, y: 28) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 33, y: 28) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 34, y: 28) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 35, y: 28) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 36, y: 28) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Front", x: 28, y: 27) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 29, y: 27) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 30, y: 27) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 31, y: 27) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 32, y: 27) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 33, y: 27) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 34, y: 27) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 35, y: 27) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Front", x: 29, y: 26) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 30, y: 26) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 31, y: 26) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 32, y: 26) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 33, y: 26) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 34, y: 26) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 35, y: 26) { Offset = new Vector2(0f, 0f) },

                new ReflectableMapTile(layerName: "Front", x: 30, y: 25) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 31, y: 25) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 32, y: 25) { Offset = new Vector2(0f, 0f) },
                new ReflectableMapTile(layerName: "Front", x: 33, y: 25) { Offset = new Vector2(0f, 0f) }
            })
        };
    }
}
