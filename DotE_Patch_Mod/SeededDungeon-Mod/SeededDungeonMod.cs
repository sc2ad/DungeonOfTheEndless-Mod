using DustDevilFramework;
using System;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;

namespace SeededDungeon_Mod
{
    [BepInPlugin("com.sc2ad.SeededDungeon", "Seeded Dungeon", "1.0.0")]
    public class SeededDungeonMod : BaseUnityPlugin
    {
        private ScadMod mod;

        private ConfigWrapper<bool> overwriteWrapper;
        private ConfigWrapper<string> saveKeyWrapper;
        private ConfigWrapper<string> createNewSeedKeyWrapper;
        public void Awake()
        {
            mod = new ScadMod("SeededDungeon", typeof(SeededDungeonMod), this);

            overwriteWrapper = Config.Wrap("Settings", "OverwriteSeeds", "Whether to overwrite seeds or create new ones.", true);
            saveKeyWrapper = Config.Wrap("Settings", "SaveKey", "The UnityEngine.KeyCode to use for saving seeds.", KeyCode.Backspace.ToString());
            createNewSeedKeyWrapper = Config.Wrap("Settings", "CreateNewSeedKey", "The UnityEngine.KeyCode to use for creating new seeds.", KeyCode.Equals.ToString());

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
            {
                // This reads all of the seeds that we have in local files into RAM so that we can use it.
                SeedCollection.ReadAll();
                // When the level loads, need to instead load a seed from config, for example
                // So the only things that seem to exist in the same locations are the layouts, layout sizes, and exits
                // Other static/dynamic events are unknown based off of solely Seed
                // However... What if I used recursive logging to find all of the data i need, then use recursive reading/setting in the same order.
                On.InputManager.Update += InputManager_Update;
            }
        }

        private void InputManager_Update(On.InputManager.orig_Update orig, InputManager self)
        {
            orig(self);
            Dungeon d = SingletonManager.Get<Dungeon>(false);
            if (!overwriteWrapper.Value)
            {
                SeedCollection.UnLoad();
            }
            else
            {
                if (!SeedCollection.Loaded)
                {
                    mod.Log("Reinitializing SeedCollection because it isn't loaded!");
                    SeedCollection.ReadAll();
                }
            }
            if (d == null)
            {
                return;
            }
            if (Input.GetKeyUp((KeyCode) Enum.Parse(typeof(KeyCode), saveKeyWrapper.Value)))
            {
                mod.Log("Saving SeedData to SeedCollection!");
                SeedCollection best = SeedCollection.GetMostCurrentSeeds(d.ShipName, d.Level);
                if (best == null)
                {
                    mod.Log("Creating new SeedCollection because there were no matching SeedCollections!");
                    best = SeedCollection.Create();
                }
                SeedData data = new SeedData();
                d.EnqueueNotification("Saved SeedData: " + data + " to: " + best.ReadFrom);
                best.Add(d.ShipName, d.Level, data);
                SeedCollection.WriteAll();
                mod.Log("Wrote SeedCollection to: " + best.ReadFrom);
            }
            if (Input.GetKeyUp((KeyCode) Enum.Parse(typeof(KeyCode), createNewSeedKeyWrapper.Value)))
            {
                mod.Log("Created new SeedCollection!");
                SeedCollection best = SeedCollection.Create();
                SeedData data = new SeedData();
                best.Add(d.ShipName, d.Level, data);
                SeedCollection.WriteAll();
                d.EnqueueNotification("Saved SeedData: " + data + " to new: " + best.ReadFrom);
                mod.Log("Wrote SeedCollection to: " + best.ReadFrom);
            }
        }

        public void UnLoad()
        {
            mod.UnLoad();
            On.InputManager.Update -= InputManager_Update;
            SeedCollection.UnLoad();
        }
    }
}
