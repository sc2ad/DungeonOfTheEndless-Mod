using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShinyHero_Mod
{
    class ShinyHeroMod : PartialityMod
    {
        ShinyHeroConfig mod = new ShinyHeroConfig();

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
