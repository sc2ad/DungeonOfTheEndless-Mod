using BepInEx;
using DustDevilFramework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrueIGT_Mod
{
    [BepInPlugin("com.sc2ad.TrueIGT", "TrueIGT", "2.5.1")]
    public class TrueIGTMod : BaseUnityPlugin
    {
        internal DateTime StartTime;
        internal bool HasStarted = false;
        internal float LastGameStartTime = float.NegativeInfinity;

        internal ScadMod mod;
        public void Awake()
        {
            mod = new ScadMod("TrueIGT", typeof(TrueIGTMod), this);

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
            {
                On.Dungeon.RPC_DoLevelOver += Dungeon_RPC_DoLevelOver;
                On.Session.Update += Session_Update;
                On.Hero.MoveToDoor += Hero_MoveToDoor;
                On.Hero.MoveToRoom += Hero_MoveToRoom;
                On.VictoryPanel.Show += VictoryPanel_Show;
                On.Dungeon.PrepareForNewGame += Dungeon_PrepareForNewGame;
            }
        }
        public void UnLoad()
        {
            mod.UnLoad();
            On.Dungeon.RPC_DoLevelOver -= Dungeon_RPC_DoLevelOver;
            On.Session.Update -= Session_Update;
            On.Hero.MoveToDoor -= Hero_MoveToDoor;
            On.Hero.MoveToRoom -= Hero_MoveToRoom;
            On.VictoryPanel.Show -= VictoryPanel_Show;
            On.Dungeon.PrepareForNewGame -= Dungeon_PrepareForNewGame;
        }

        private void Dungeon_PrepareForNewGame(On.Dungeon.orig_PrepareForNewGame orig, bool multiplayer)
        {
            HasStarted = false;
            orig(multiplayer);
        }

        private void VictoryPanel_Show(On.VictoryPanel.orig_Show orig, VictoryPanel self, object[] parameters)
        {
            orig(self, parameters);
            Dungeon d = SingletonManager.Get<Dungeon>(true);
            AgePrimitiveLabel label = new DynData<VictoryPanel>(self).Get<AgePrimitiveLabel>("informationLabel");
            label.Text += " - DustDevil: v" + DustDevil.GetVersion();
            label.Text += " - Time: " + d.Statistics.GetStat(DungeonStatistics.Stat_GameTime).DurationToString();
        }

        private void Hero_MoveToRoom(On.Hero.orig_MoveToRoom orig, Hero self, Room room, bool allowMoveInterruption, bool isMoveOrderedByPlayer, bool triggerTutorialEvent)
        {
            orig(self, room, allowMoveInterruption, isMoveOrderedByPlayer, triggerTutorialEvent);
            mod.Log("Currently attempting to move to a room!");
        }

        private void Hero_MoveToDoor(On.Hero.orig_MoveToDoor orig, Hero self, Door door, bool allowMoveInterruption, Door nextMoveDoorTarget, bool isMoveOrderedByPlayer)
        {
            orig(self, door, allowMoveInterruption, nextMoveDoorTarget, isMoveOrderedByPlayer);
            Dungeon d = SingletonManager.Get<Dungeon>(false);
            mod.Log("Attempting to move to a door!");
            if (d == null)
            {
                return;
            }
            if (d.Level == 1 && !HasStarted)
            {
                // Only do this for the first level...
                StartTime = DateTime.Now;
                mod.Log("Set Floor 1 StartTime!");
                mod.Log("StartTime: " + StartTime.ToLongTimeString());
                HasStarted = true;
            }
        }

        private void Session_Update(On.Session.orig_Update orig, Session self)
        {
            orig(self);
            Dungeon d = SingletonManager.Get<Dungeon>(false);
            if (d == null)
            {
                return;
            }
            if (d.GameStartTime != LastGameStartTime)
            {
                // Don't actually do this for floor 1, instead calculate it based off of when the first door is selected to be opened
                if (d.Level == 1)
                {
                    // Skip this
                    return;
                }
                if (d.IsDisplayed && UnityEngine.Time.timeScale != 0) // Basically calculates load time from the very frame the dungeon is loaded and the game is unpaused
                {
                    if (LastGameStartTime != 0)
                    {
                        //dict.Add(DateTime.Now, true);
                        StartTime = DateTime.Now;
                    }
                    LastGameStartTime = d.GameStartTime;
                    mod.Log("Set LastGameStartTime!");
                    mod.Log("Times: ");
                    mod.Log(StartTime.ToLongTimeString());
                }
            }
        }

        private void Dungeon_RPC_DoLevelOver(On.Dungeon.orig_RPC_DoLevelOver orig, Dungeon self, bool victory)
        {
            if (float.IsNegativeInfinity(LastGameStartTime))
            {
                // So if the door wasn't even opened, and we exited the game, we should just count the time as one second.
                StartTime = DateTime.Now.AddSeconds(-1);
            }
            mod.Log("Times: ");
            mod.Log(StartTime.ToLongTimeString());

            mod.Log("DoLevelOverRPC!");
            orig(self, victory);
            mod.Log("Delta Time: " + (DateTime.Now - StartTime).TotalSeconds);

            mod.Log("Time Level Took (RealIGT Seconds): " + (DateTime.Now - StartTime).TotalSeconds);
            mod.Log("Time Level Took (InGame IGT): " + (UnityEngine.Time.time - self.GameStartTime));
            mod.Log("Time game played (InGame IGT): " + self.Statistics.GetStat(DungeonStatistics.Stat_GameTime));
            self.Statistics.SetStat(DungeonStatistics.Stat_LevelTime, (float)(DateTime.Now - StartTime).TotalSeconds);
            self.Statistics.IncrementStat(DungeonStatistics.Stat_GameTime, -(UnityEngine.Time.time - self.GameStartTime));
            self.Statistics.IncrementStat(DungeonStatistics.Stat_GameTime, (float)(DateTime.Now - StartTime).TotalSeconds);
            mod.Log("Time game played (RealIGT): " + self.Statistics.GetStat(DungeonStatistics.Stat_GameTime));
            // This should never error, cause it is using that start time to actually do everything. If StartTime is invalid, then it won't enter this if statement
        }
    }
}
