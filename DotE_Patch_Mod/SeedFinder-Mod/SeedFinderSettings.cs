using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedFinder_Mod
{
    class SeedFinderSettings : ModSettings
    {
        public int LowestSeed = Int32.MinValue;
        public int LargestSeed = Int32.MaxValue;
        public bool UseRandom = true;
        [SettingsIgnore()]
        public string ComparatorType = SeedComparatorType.ComparatorType.MinimizeExitDepth.ToString();
        public SeedFinderSettings(string name) : base(name)
        {
        }
    }
}
