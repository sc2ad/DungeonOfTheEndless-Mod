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
        ShinyHeroConfig mod = new ShinyHeroConfig(typeof(ShinyHeroMod));

        public override void Init()
        {
            mod.PartialityModReference = this;
            mod.Initialize();
            mod.settings.ReadSettings();
        }
        public override void OnLoad()
        {
            mod.Load();
        }
        public void UnLoad()
        {
            mod.UnLoad();
        }
    }
}
