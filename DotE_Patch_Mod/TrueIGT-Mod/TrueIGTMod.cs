using DustDevilFramework;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueIGT_Mod
{
    class TrueIGTMod : PartialityMod
    {
        //public static Dictionary<DateTime, bool> dict = new Dictionary<DateTime, bool>(); // For some reason, using normal values doesn't want to work.

        public static DateTime StartTime;
        public float LastGameStartTime = float.NegativeInfinity;

        ScadMod mod = new ScadMod();
        public override void Init()
        {
            mod.name = "TrueIGT";
            mod.default_config = "# Modify this file to change various settings of the TrueIGT Mod for DotE.\n" + mod.default_config;
            mod.Initialize();

            // Setup default values for config

            mod.ReadConfig();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                On.Dungeon.RPC_DoLevelOver += Dungeon_RPC_DoLevelOver;
                //On.Dungeon.FillDungeonAsync += Dungeon_FillDungeonAsync;
                //On.Session.PostStateChange += Session_PostStateChange;
                On.Session.Update += Session_Update;
                //On.Dungeon.PrepareForNewGame += Dungeon_PrepareForNewGame;
                //On.Dungeon.PrepareForSavedGame += Dungeon_PrepareForSavedGame;
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

        private void Dungeon_PrepareForSavedGame(On.Dungeon.orig_PrepareForSavedGame orig, string saveKey, bool multiplayer)
        {
            mod.Log("Calling Prep Saved Game!");
            orig(saveKey, multiplayer);
            //dict.Add(DateTime.Now, true);
        }

        private void Dungeon_PrepareForNewGame(On.Dungeon.orig_PrepareForNewGame orig, bool multiplayer)
        {
            mod.Log("Calling Prep New Game!");
            orig(multiplayer);
            //dict.Add(DateTime.Now, true);
        }

        private System.Collections.IEnumerator Dungeon_FillDungeonAsync(On.Dungeon.orig_FillDungeonAsync orig, Dungeon self)
        {
            mod.Log("Calling FillDungeon!");
            System.Collections.IEnumerator temp = orig(self);
            //dict.Add(DateTime.Now, true);
            // Could do something with setting GameStartTime here, and using it instead of having to rewrite OnLevelOver
            return temp;
        }

        private void Dungeon_RPC_DoLevelOver(On.Dungeon.orig_RPC_DoLevelOver orig, Dungeon self, bool victory)
        {
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
            // This should never error, cause it is using that start time to actually do everything. If StarTime is invalid, then it won't enter this if statement
        }
    }
}
