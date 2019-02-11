using System;
using System.Collections.Generic;
using System.Text;

namespace DustDevilFramework
{
    public class PodSettings : ModSettings
    {
        public string AssumedPod = "PLACEHOLDER";
        public string DungeonPod = "PLACEHOLDER";
        public PodSettings(string name) : base(name)
        {
        }
    }
}
