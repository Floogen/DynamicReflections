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
            // Doen't look that good
            /*new ReflectableMapObject("Forest_North_WoodenPole_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 31, y: 21) { Offset = new Vector2(0f, 1.2f) },
            }),
            new ReflectableMapObject("Forest_North_WoodenPole_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 31, y: 24) { Offset = new Vector2(0f, 1.2f) },
            }),
            new ReflectableMapObject("Forest_North_WoodenPole_3", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 37, y: 20) { Offset = new Vector2(0f, 1.2f) },
            }),
            new ReflectableMapObject("Forest_North_WoodenPole_4", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 37, y: 26) { Offset = new Vector2(0f, 1.2f) },
            }),*/
            new ReflectableMapObject("Forest_North_StaticFir_Tree_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 29, y: 78),

                new ReflectableMapTile(layerName: "Front", x: 29, y: 77),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 28, y: 76),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 29, y: 76),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 30, y: 76),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 28, y: 75),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 29, y: 75),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 30, y: 75),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 28, y: 74),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 29, y: 74),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 30, y: 74),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 28, y: 73),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 29, y: 73),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 30, y: 73)
            }),
            new ReflectableMapObject("Forest_North_StaticPine_Tree_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 50, y: 86),

                new ReflectableMapTile(layerName: "Front", x: 50, y: 85),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 49, y: 84),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 50, y: 84),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 51, y: 84),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 49, y: 83),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 50, y: 83),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 51, y: 83),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 49, y: 82),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 50, y: 82),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 51, y: 82),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 49, y: 81),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 50, y: 81),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 51, y: 81)
            }),
            new ReflectableMapObject("Forest_North_StaticPine_Tree_2", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 40, y: 66),

                new ReflectableMapTile(layerName: "Front", x: 40, y: 65),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 39, y: 64),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 40, y: 64),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 41, y: 64),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 39, y: 63),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 40, y: 63),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 41, y: 63),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 39, y: 62),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 40, y: 62),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 41, y: 62),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 39, y: 61),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 40, y: 61),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 41, y: 61)
            }),
            new ReflectableMapObject("Forest_North_StaticPine_Tree_3", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 80, y: 54),

                new ReflectableMapTile(layerName: "Front", x: 80, y: 53),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 79, y: 52),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 80, y: 52),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 81, y: 52),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 79, y: 51),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 80, y: 51),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 81, y: 51),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 79, y: 50),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 80, y: 50),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 81, y: 50),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 79, y: 49),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 80, y: 49),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 81, y: 49)
            }),
            new ReflectableMapObject("Forest_North_StaticBirch_Tree_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 95, y: 48),

                new ReflectableMapTile(layerName: "Front", x: 95, y: 47),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 94, y: 46),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 95, y: 46),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 96, y: 46),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 94, y: 45),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 95, y: 45),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 96, y: 45),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 94, y: 44),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 95, y: 44),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 96, y: 44),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 94, y: 43),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 95, y: 43),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 96, y: 43)
            }),
            new ReflectableMapObject("Forest_North_StaticMaple_Tree_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 98, y: 46),

                new ReflectableMapTile(layerName: "Front", x: 98, y: 45),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 97, y: 44),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 98, y: 44),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 99, y: 44),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 97, y: 43),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 98, y: 43),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 99, y: 43),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 97, y: 42),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 98, y: 42),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 99, y: 42),

                new ReflectableMapTile(layerName: "AlwaysFront", x: 97, y: 41),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 98, y: 41),
                new ReflectableMapTile(layerName: "AlwaysFront", x: 99, y: 41)
            }),
            new ReflectableMapObject("Forest_North_StaticBigBush_1", new List<ReflectableMapTile>()
            {
                new ReflectableMapTile(layerName: "Buildings", x: 104, y: 36),
                new ReflectableMapTile(layerName: "Buildings", x: 105, y: 36),
                new ReflectableMapTile(layerName: "Buildings", x: 106, y: 36),

                new ReflectableMapTile(layerName: "Front", x: 104, y: 35),
                new ReflectableMapTile(layerName: "Front", x: 105, y: 35),
                new ReflectableMapTile(layerName: "Front", x: 106, y: 35),

                new ReflectableMapTile(layerName: "Front", x: 104, y: 34),
                new ReflectableMapTile(layerName: "Front", x: 105, y: 34),
                new ReflectableMapTile(layerName: "Front", x: 106, y: 34)
            }),
        };
    }
}
