using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceModifier.MathFuncs
{
    public static class Map
    {
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        public static float InverseLerp(float a, float b, float t)
        {
            return (t - a) / (b - a);
        }
        public static float Range(float fa, float fb, float ta, float tb, float t)
        {
            return Lerp(ta, tb, InverseLerp(fa, fb, t));
        }
    }
}
