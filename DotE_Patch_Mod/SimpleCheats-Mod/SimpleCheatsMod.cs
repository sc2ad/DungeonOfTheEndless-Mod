using BepInEx;
using BepInEx.Configuration;
using DustDevilFramework;
using System;
using UnityEngine;

namespace SimpleCheats_Mod
{
    [BepInPlugin("com.sc2ad.SimpleCheats", "Simple Cheats", "1.0.0")]
    public class SimpleCheatsMod : BaseUnityPlugin
    {
        private ScadMod mod;

        private ConfigWrapper<float> incrementWrapper;
        private ConfigWrapper<string> indKeyWrapper;
        private ConfigWrapper<string> sciKeyWrapper;
        private ConfigWrapper<string> foodKeyWrapper;
        private ConfigWrapper<string> dustKeyWrapper;

        public void Awake()
        {
            mod = new ScadMod("Simple Cheats", this);

            incrementWrapper = Config.Wrap("Settings", "IncrementAmount", "How much to increment for each key press", 10.0f);
            indKeyWrapper = Config.Wrap("Settings", "IndustryKey", "The key to press to receive industry", KeyCode.M.ToString());
            sciKeyWrapper = Config.Wrap("Settings", "ScienceKey", "The key to press to receive science", KeyCode.Comma.ToString());
            foodKeyWrapper = Config.Wrap("Settings", "FoodKey", "The key to press to receive food", KeyCode.Period.ToString());
            dustKeyWrapper = Config.Wrap("Settings", "DustKey", "The key to press to receive dust", KeyCode.Slash.ToString());

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
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
            float increment = incrementWrapper.Value;
            KeyCode iKey = (KeyCode)Enum.Parse(typeof(KeyCode), indKeyWrapper.Value);
            if (Input.GetKeyDown(iKey))
            {
                mod.Log("Adding: " + increment + " industry!");
                Player.LocalPlayer.AddIndustry(increment);
            }
            KeyCode sKey = (KeyCode)Enum.Parse(typeof(KeyCode), sciKeyWrapper.Value);
            if (Input.GetKeyDown(sKey))
            {
                mod.Log("Adding: " + increment + " science!");
                Player.LocalPlayer.AddScience(increment);
            }
            KeyCode fKey = (KeyCode)Enum.Parse(typeof(KeyCode), foodKeyWrapper.Value);
            if (Input.GetKeyDown(fKey))
            {
                mod.Log("Adding: " + increment + " food!");
                Player.LocalPlayer.AddFood(increment);
            }
            KeyCode dKey = (KeyCode)Enum.Parse(typeof(KeyCode), dustKeyWrapper.Value);
            if (Input.GetKeyDown(dKey))
            {
                mod.Log("Adding: " + increment + " dust!");
                self.AddDust(increment);
            }
        }
    }
}
