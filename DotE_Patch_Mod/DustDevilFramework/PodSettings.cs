using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustDevilFramework
{
    public abstract class PodSettings : ModSettings
    {
        public string AssumedPod = "PLACEHOLDER";
        public string DungeonPod = "PLACEHOLDER";
        public PodSettings(string name) : base(name)
        {
        }
    }
}
