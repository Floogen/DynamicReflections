using DynamicReflections.Framework.Models.Reflections;
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
            })
        };
    }
}
