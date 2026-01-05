using DynamicReflections.Framework.Models.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models.MapTilePresets
{
    internal abstract class MapTilePresetTemplate
    {
        public abstract string MapName { get; }
        public abstract List<ReflectableMapObject> MapObjects { get; }
    }
}
