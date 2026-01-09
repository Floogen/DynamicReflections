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
        public virtual List<string> RequiredModIds { get; } = new List<string>();
        public virtual List<string> SkipWithModIds { get; } = new List<string>();
        public abstract List<ReflectableMapObject> MapObjects { get; }
    }
}
