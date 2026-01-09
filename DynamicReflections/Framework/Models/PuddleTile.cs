using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Models
{
    internal class PuddleTile
    {
        public int PuddleIndex { get; set; } = -1;
        public int BigPuddleIndex { get; set; } = -1;
        public int PuddleEffect { get; set; }
        public float PuddleRotation { get; set; }

        public bool IsValid()
        {
            return PuddleIndex != -1;
        }
    }
}
