using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicReflections.Framework.Utilities.Extensions
{
    public static class RandomExtensions
    {
        // Referenced from https://stackoverflow.com/questions/1064901/random-number-between-2-double-numbers
        public static double NextDouble(this Random random, double minValue, double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }
    }
}