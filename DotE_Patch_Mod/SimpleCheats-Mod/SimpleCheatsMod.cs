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
        ScadMod mod = new ScadMod("SimpleCheats");
        public override void Init()
        {
            mod.default_config = "# Modify this file to change various settings of the SimpleCheats Mod for DotE.\n" + mod.default_config;
            mod.Initialize();

            // Setup default values for config
            mod.Values.Add("Industry Key", KeyCode.M.ToString());
            mod.Values.Add("Science Key", KeyCode.Comma.ToString());
            mod.Values.Add("Food Key", KeyCode.Period.ToString());
            mod.Values.Add("Dust Key", KeyCode.Slash.ToString());
            mod.Values.Add("Increment Amount", "10");

            mod.ReadConfig();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                On.Dungeon.Update += Dungeon_Update;
            }
        }

        private void Dungeon_Update(On.Dungeon.orig_Update orig, Dungeon self)
        {
            orig(self);
            float increment = (float)Convert.ToDouble(mod.Values["Increment Amount"]);
            KeyCode iKey = (KeyCode)Enum.Parse(typeof(KeyCode), mod.Values["Industry Key"]);
            if (Input.GetKeyDown(iKey))
            {
                mod.Log("Adding: " + increment + " industry!");
                Player.LocalPlayer.AddIndustry(increment);
            }
            KeyCode sKey = (KeyCode)Enum.Parse(typeof(KeyCode), mod.Values["Science Key"]);
            if (Input.GetKeyDown(sKey))
            {
                mod.Log("Adding: " + increment + " science!");
                Player.LocalPlayer.AddScience(increment);
            }
            KeyCode fKey = (KeyCode)Enum.Parse(typeof(KeyCode), mod.Values["Food Key"]);
            if (Input.GetKeyDown(fKey))
            {
                mod.Log("Adding: " + increment + " food!");
                Player.LocalPlayer.AddFood(increment);
            }
            KeyCode dKey = (KeyCode)Enum.Parse(typeof(KeyCode), mod.Values["Dust Key"]);
            if (Input.GetKeyDown(dKey))
            {
                mod.Log("Adding: " + increment + " dust!");
                self.AddDust(increment);
            }
        }
    }
}
