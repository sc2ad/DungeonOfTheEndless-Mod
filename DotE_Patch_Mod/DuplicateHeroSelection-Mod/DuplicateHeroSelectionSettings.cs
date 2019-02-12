using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DustDevilFramework.ModSettings;

namespace DuplicateHeroSelection_Mod
{
    class DuplicateHeroSelectionSettings : ModSettings
    {
        [SettingsRange(0, 4)]
        public int AllowedDuplicateCount = 4;
        public DuplicateHeroSelectionSettings(string name) : base(name)
        {
        }
    }
}
