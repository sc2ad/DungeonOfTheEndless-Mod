using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustDevilFramework
{
    class SliderRange
    {
        internal float Min { get; } = 0;
        internal float Max { get; } = 100;
        internal float Increment { get; private set; } = 1;
        internal SliderRange() { }
        internal SliderRange(float l, float u, float i)
        {
            Min = l;
            Max = u;
            Increment = i;
        }
        internal SliderRange(float l, float u)
        {
            Min = l;
            Max = u;
        }
        public void SetIncrement(float incr)
        {
            Increment = incr;
        }
        public override string ToString()
        {
            return "(" + Min + ", " + Max + ", " + Increment + ")";
        }
    }
}
