using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping_Mod
{
    public class PingSettings : ModSettings
    {
        public string Key = "G";
        [SettingsRange(1,10)]
        public float SecondsActive = 3;
        public PingSettings(string name) : base(name)
        {
        }
    }
}
