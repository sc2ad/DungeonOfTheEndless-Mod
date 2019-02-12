using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedModule_Mod
{
    public class SpeedModuleSettings : CustomModuleSettings
    {
        [SettingsRange(0, 10, 1)]
        public float SpeedIncrease = 5;
        public SpeedModuleSettings(string name) : base(name)
        {
        }
    }
}
