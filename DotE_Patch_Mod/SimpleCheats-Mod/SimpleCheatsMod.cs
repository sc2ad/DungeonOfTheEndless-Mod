using DustDevilFramework;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimpleCheats_Mod
{
    class SimpleCheatsMod : PartialityMod
    {
        ScadMod mod = new ScadMod("SimpleCheats", typeof(SimpleCheatsSettings), typeof(SimpleCheatsMod));
        public override void Init()
        {
            mod.BepinPluginReference = this;
            mod.Initialize();

            mod.settings.ReadSettings();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                On.Dungeon.Update += Dungeon_Update;
            }
        }
        public void UnLoad()
        {
            mod.UnLoad();
            On.Dungeon.Update -= Dungeon_Update;
        }

        private void Dungeon_Update(On.Dungeon.orig_Update orig, Dungeon self)
        {
            orig(self);
            float increment = (mod.settings as SimpleCheatsSettings).IncrementAmount;
            KeyCode iKey = (KeyCode)Enum.Parse(typeof(KeyCode), (mod.settings as SimpleCheatsSettings).IndustryKey);
            if (Input.GetKeyDown(iKey))
            {
                mod.Log("Adding: " + increment + " industry!");
                Player.LocalPlayer.AddIndustry(increment);
            }
            KeyCode sKey = (KeyCode)Enum.Parse(typeof(KeyCode), (mod.settings as SimpleCheatsSettings).ScienceKey);
            if (Input.GetKeyDown(sKey))
            {
                mod.Log("Adding: " + increment + " science!");
                Player.LocalPlayer.AddScience(increment);
            }
            KeyCode fKey = (KeyCode)Enum.Parse(typeof(KeyCode), (mod.settings as SimpleCheatsSettings).FoodKey);
            if (Input.GetKeyDown(fKey))
            {
                mod.Log("Adding: " + increment + " food!");
                Player.LocalPlayer.AddFood(increment);
            }
            KeyCode dKey = (KeyCode)Enum.Parse(typeof(KeyCode), (mod.settings as SimpleCheatsSettings).DustKey);
            if (Input.GetKeyDown(dKey))
            {
                mod.Log("Adding: " + increment + " dust!");
                self.AddDust(increment);
            }
        }
    }
}
