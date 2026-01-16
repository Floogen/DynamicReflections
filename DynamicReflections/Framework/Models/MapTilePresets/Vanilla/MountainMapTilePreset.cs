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
            new ReflectableMapObject("Mountain_North_StaticPine_Tree_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "AlwaysFront", x: 46, y: 14),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 46, y: 13),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 45, y: 12),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 46, y: 12),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 47, y: 12),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 45, y: 11),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 46, y: 11),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 47, y: 11),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 45, y: 10),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 46, y: 10),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 47, y: 10),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 45, y: 9),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 46, y: 9),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 47, y: 9)
            }),
            new ReflectableMapObject("Mountain_North_StaticFir_Tree_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 53, y: 13),

                new ReflectableMapTile(layerName: "Front", x: 53, y: 12),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 52, y: 11),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 53, y: 11),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 54, y: 11),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 52, y: 10),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 53, y: 10),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 54, y: 10),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 52, y: 9),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 53, y: 9),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 54, y: 9),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 52, y: 8),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 53, y: 8),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 54, y: 8)
            }),
            //Silly me, looking for a cusror draw, but can it be done tho? (This does not work, the boulder is not a layer)
            /*new ReflectableMapObject("Mountain_North_GlitteringBoulder", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 48, y: 13),
                new ReflectableMapTile(layerName: "Buildings", x: 49, y: 13),

                new ReflectableMapTile(layerName: "Buildings", x: 48, y: 12),
                new ReflectableMapTile(layerName: "Buildings", x: 49, y: 12),

                new ReflectableMapTile(layerName: "Buildings", x: 48, y: 11),
                new ReflectableMapTile(layerName: "Buildings", x: 49, y: 11)
            }),*/
            new ReflectableMapObject("Mountain_North_Wood_Plank_Long", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 61, y: 21) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 62, y: 21) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 63, y: 21) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 64, y: 21) { Offset = new Vector2(0f, 1.65f) },
                new ReflectableMapTile(layerName: "Buildings", x: 65, y: 21) { Offset = new Vector2(0f, 1.65f) }
            }),
            new ReflectableMapObject("Mountain_North_StaticFir_Tree_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 96, y: 5),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 96, y: 4),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 95, y: 3),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 96, y: 3),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 97, y: 3),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 95, y: 2),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 96, y: 2),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 97, y: 2),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 95, y: 1),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 96, y: 1),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 97, y: 1),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 95, y: 0),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 96, y: 0),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 97, y: 0)
            }),
            new ReflectableMapObject("Mountain_North_StaticFir_Tree_3", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 101, y: 23),

                new ReflectableMapTile(layerName: "Front", x: 101, y: 22),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 100, y: 21),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 101, y: 21),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 102, y: 21),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 100, y: 20),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 101, y: 20),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 102, y: 20),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 100, y: 19),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 101, y: 19),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 102, y: 19),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 100, y: 18),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 101, y: 18),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 102, y: 18)
            }),
            new ReflectableMapObject("Mountain_North_StaticPine_Tree_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 103, y: 32),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 103, y: 31),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 102, y: 30),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 103, y: 30),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 104, y: 30),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 102, y: 29),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 103, y: 29),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 104, y: 29),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 102, y: 28),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 103, y: 28),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 104, y: 28),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 102, y: 27),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 103, y: 27),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 104, y: 27)
            }),
        };
    }
}
