using DustDevilFramework;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TASTools_Mod
{
    public class TASToolsMod : PartialityMod
    {
        ScadMod mod = new ScadMod("TASToolsMod", typeof(TASToolsSettings), typeof(TASToolsMod));
        public override void Init()
        {
            mod.PartialityModReference = this;
            mod.Initialize();

            mod.settings.ReadSettings();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {

            }
        }

        public void UnLoad()
        {
            mod.UnLoad();
        }
    }
}
