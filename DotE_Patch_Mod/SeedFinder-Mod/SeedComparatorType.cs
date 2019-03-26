using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedFinder_Mod
{
    public class SeedComparatorType
    {
        public enum ComparatorType
        {
            MinimizeExitDepth
        }
        /// <summary>
        /// Returns a positive integer when bestSeed should be changed, negative or 0 otherwise.
        /// </summary>
        public Comparison<int> comparison;
        public int bestDepth = Int16.MaxValue;

        public SeedComparatorType(ComparatorType t)
        {
            switch (t)
            {
                case ComparatorType.MinimizeExitDepth:
                    comparison = delegate (int bestSeed, int newSeed)
                    {
                        int d = Util.GetExitDepth();
                        if (bestDepth < d)
                        {
                            int r = d - bestDepth;
                            bestDepth = d;
                            return r;
                        }
                        return d - bestDepth;
                    };
                    break;
            }
        }
    }
}
