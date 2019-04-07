using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotoFloor_Mod
{
    class GotoFloorSettings : ModSettings
    {
        // Add fields here! Fields that are supported include ints, floats, bools, doubles, strings
        // Other fields can be stored here, but will not be saved to a config file
        // You can preface each field with a flag from DustDevilFramework.ModSettings
        [SettingsRange(1, 12)]
        public int LevelTarget = 1;
        
        public GotoFloorSettings(string name) : base(name)
        {
        }
    }
}
