using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSashaItem_Mod
{
    class ShinyItemMod : PartialityMod
    {
        ShinyItemConfig mod = new ShinyItemConfig();

        public override void Init()
        {
            mod.Initialize();
            mod.ReadConfig();
        }
        public override void OnLoad()
        {
            mod.Load();
        }
    }
}
