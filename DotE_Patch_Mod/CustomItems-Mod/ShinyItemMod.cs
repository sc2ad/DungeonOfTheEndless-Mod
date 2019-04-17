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
        ShinyItemConfig mod = new ShinyItemConfig(typeof(ShinyItemMod));

        public override void Init()
        {
            mod.BepinPluginReference = this;
            mod.Initialize();
            mod.settings.ReadSettings();
        }
        public override void OnLoad()
        {
            mod.Load();
        }
    }
}
