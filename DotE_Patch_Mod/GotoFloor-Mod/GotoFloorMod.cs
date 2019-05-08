using Amplitude.Unity.Audio;
using Amplitude.Unity.Framework;
using DustDevilFramework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using BepInEx.Configuration;

namespace GotoFloor_Mod
{
    [BepInPlugin("com.sc2ad.GotoFloor", "GotoFloor", "1.0.0")]
    class GotoFloorMod : BaseUnityPlugin
    {
        private ScadMod mod;

        private bool CompletedSkip = false;
        private ConfigWrapper<int> levelTargetWrapper;

        public void Awake()
        {
            mod = new ScadMod("GotoFloor", typeof(GotoFloorMod), this);

            levelTargetWrapper = Config.Wrap<int>("Settings", "LevelTarget", "The target level to go to. Must be between 2 and 12.", 12);

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
            {
                On.Dungeon.PrepareForNewGame += Dungeon_PrepareForNewGame;
                On.Dungeon.PrepareForNextLevel += Dungeon_PrepareForNextLevel;
                On.Dungeon.Update += Dungeon_Update;
                // Add hooks here!
            }
        }

        private void Dungeon_Update(On.Dungeon.orig_Update orig, Dungeon self)
        {
            orig(self);
            List<Hero> heroes = Hero.GetAllPlayersActiveRecruitedHeroes();
            if (!CompletedSkip && self.Level == 1 && levelTargetWrapper.Value >= 2 && self.ShipConfig != null && self.RoomCount != 0 && self.StartRoom != null && heroes != null && heroes.Count > 0 && heroes[0] != null && heroes[0].RoomElement != null && levelTargetWrapper.Value <= 12)
            {
                Room exit = self.StartRoom;

                var method = typeof(Dungeon).GetMethod("SpawnExit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method.Equals(null))
                {
                    mod.Log("SpawnExit method is null!");
                    return;
                }
                if (Services.GetService<IAudioLayeredMusicService>() == null)
                {
                    // Waiting for music!
                    mod.Log("Music does not exist!");
                    return;
                }

                mod.Log("Calling SpawnExit!");

                method.Invoke(self, new object[] { self.StartRoom });

                mod.Log("Attempting to plug exit and end level immediately!");
                self.NotifyCrystalStateChanged(CrystalState.Unplugged);
                self.ExitRoom = exit;
                //new DynData<Dungeon>(self).Set("ExitRoom", exit);

                mod.Log("Setting heroes to contain crystal and be in exit room!");
                typeof(Hero).GetProperty("HasCrystal").SetValue(heroes[0], true, null);
                //new DynData<Hero>(heroes[0]).Set("HasCrystal", true);
                foreach (Hero h in heroes)
                {
                    h.RoomElement.SetParentRoom(exit);
                    h.WasInExitRoomAtExitTime = true;
                }
                // Must be greater than 0!
                float delay = 1f;
                new DynData<Dungeon>(self).Set("vistoryScreenDisplayDelay", delay);
                mod.Log("Attempting to end level with wait delay of: " + delay);
                self.LevelOver(true);
                self.OnCrystalPlugged();
                CompletedSkip = true;
            }
        }

        private void Dungeon_PrepareForNextLevel(On.Dungeon.orig_PrepareForNextLevel orig)
        {
            Dungeon d = SingletonManager.Get<Dungeon>(false);
            if (d.Level == 1)
            {
                mod.Log("Setting Level for next level to: " + (levelTargetWrapper.Value - 1));
                //new DynData<Dungeon>(d).Set("Level", levelTargetWrapper.Value - 1);
                typeof(Dungeon).GetProperty("Level").SetValue(d, levelTargetWrapper.Value - 1, null);
            }
            orig();
            CompletedSkip = false;
        }

        private void Dungeon_PrepareForNewGame(On.Dungeon.orig_PrepareForNewGame orig, bool multiplayer)
        {
            orig(multiplayer);
            // Get the nextDungeonGenerationParams to modify!
            var field = typeof(Dungeon).GetField("nextDungeonGenerationParams", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            DungeonGenerationParams p = (DungeonGenerationParams)field.GetValue(null);
            p.Level = levelTargetWrapper.Value;
            mod.Log("Set the nextDungeonGenerationParams to level: " + levelTargetWrapper.Value);
            CompletedSkip = false;
        }

        public void UnLoad()
        {
            mod.UnLoad();
            // Remove hooks here!
            On.Dungeon.PrepareForNewGame -= Dungeon_PrepareForNewGame;
            On.Dungeon.PrepareForNextLevel -= Dungeon_PrepareForNextLevel;
            On.Dungeon.Update -= Dungeon_Update;
        }
    }
}
