using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceLimiter_Mod
{
    public class ResourceLimiterSettings : ModSettings
    {
        public string Use = "Percentage";
        public double FlatRate = 3;
        public double Percentage = 0.75;
        public ResourceLimiterSettings(string name) : base(name)
        {
        }
    }
}
