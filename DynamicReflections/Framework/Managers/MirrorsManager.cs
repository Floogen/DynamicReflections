using DynamicReflections.Framework.Models;
using DynamicReflections.Framework.Models.ContentPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Managers
{
    internal class MirrorsManager
    {
        private Dictionary<string, MirrorSettings> _furnitureIdToMirrors;

        public MirrorsManager()
        {
            Reset();
        }

        public void Add(List<ContentPackModel> models)
        {
            foreach (var model in models)
            {
                if (model is null || String.IsNullOrEmpty(model.FurnitureId))
                {
                    continue;
                }

                _furnitureIdToMirrors[model.FurnitureId.ToLower()] = model.Mirror;
            }
        }

        public MirrorSettings? Get(string furnitureId)
        {
            if (_furnitureIdToMirrors.ContainsKey(furnitureId.ToLower()) is false)
            {
                return null;
            }

            return _furnitureIdToMirrors[furnitureId.ToLower()];
        }

        public void Reset()
        {
            _furnitureIdToMirrors = new Dictionary<string, MirrorSettings>();
        }
    }
}
