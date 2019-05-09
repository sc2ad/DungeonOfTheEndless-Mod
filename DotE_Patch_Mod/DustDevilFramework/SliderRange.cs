using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustDevilFramework
{
    class SliderRange
    {
        internal float lower = 0;
        internal float upper = 100;
        internal float increment = 1;
        internal SliderRange() { }
        internal SliderRange(float l, float u, float i)
        {
            lower = l;
            upper = u;
            increment = i;
        }
        internal SliderRange(float l, float u)
        {
            lower = l;
            upper = u;
        }
        public override string ToString()
        {
            return "(" + lower + ", " + upper + ", " + increment + ")";
        }
    }
}
