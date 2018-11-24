using DustDevilFramework;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeededDungeon_Mod
{
    public class SeededDungeonMod : PartialityMod
    {
        ScadMod mod = new ScadMod("SeededDungeon");
        Dictionary<int, int> seeds = new Dictionary<int, int>();
        List<Room> tempListToSave;
        public override void Init()
        {
            mod.default_config = "# Modify this file to change various settings of the SeededDungeon Mod for DotE.\n" + mod.default_config;
            mod.Initialize();

            // Setup default values for config
            mod.Values.Add("Log seeds", "true");
            mod.Values.Add("SeedLog Path", "SeedLog.txt");
            mod.Values.Add("Read From SeedLog if possible", "false");


            mod.ReadConfig();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                // When the level loads, need to instead load a seed from config, for example
                // So the only things that seem to exist in the same locations are the layouts, layout sizes, and exits
                // Other static/dynamic events are unknown based off of solely Seed
                // However... What if I used recursive logging to find all of the data i need, then use recursive reading/setting in the same order.
                On.DungeonGenerator2.GenerateDungeonCoroutine += DungeonGenerator2_GenerateDungeonCoroutine;
                On.DungeonGenerator2.FreeMemory += DungeonGenerator2_FreeMemory;
            }
            if (Convert.ToBoolean(mod.Values["Read From SeedLog if possible"]))
            {
                ReadSeeds();
            }
        }

        private System.Collections.IEnumerator DungeonGenerator2_FreeMemory(On.DungeonGenerator2.orig_FreeMemory orig, DungeonGenerator2 self)
        {
            List<global::Room> roomsInDungeon = new DynData<DungeonGenerator2>(self).Get<List<global::Room>>("dungeonRooms");
            if (roomsInDungeon != null)
            {
                tempListToSave = new List<Room>();
                foreach (Room r in roomsInDungeon)
                {
                    // Need to copy the room to be used (for all intensive purposes...)
                    // Because "FreeMemory" deletes the rooms too!
                    // Or, in hindsight, we could just do recursive tracing
                    tempListToSave.Add(r);
                }
            }
            yield return orig(self);
            yield break;
        }

        private System.Collections.IEnumerator DungeonGenerator2_GenerateDungeonCoroutine(On.DungeonGenerator2.orig_GenerateDungeonCoroutine orig, DungeonGenerator2 self, int level, Amplitude.StaticString shipName)
        {
            if (seeds.ContainsKey(level))
            {
                // And if i actually want to apply the seed change...
                mod.Log("Creating the Dungeon with the provided seed from the dictionary: " + seeds[level]);
                new DynData<DungeonGenerator2>(self).Set<int>("randomSeed", seeds[level]);
            }
            yield return orig(self, level, shipName);
            if (!seeds.ContainsKey(level))
            {
                int seed = new DynData<DungeonGenerator2>(self).Get<int>("randomSeed");
                mod.Log("Adding a seed to the seeds dictionary: " + seed);
                seeds.Add(level, seed);
                if (Convert.ToBoolean(mod.Values["Log seeds"]))
                {
                    LogSeeds();
                }
            }
            LogRoomData(tempListToSave);
            yield break;
        }
        private void LogSeeds()
        {
            string text = "Seeds for each level in most recently played Pod\n";
            foreach (int level in seeds.Keys)
            {
                text += "Level: " + level + " seed: " + seeds[level] + "\n";
            }
            System.IO.File.WriteAllText(mod.Values["SeedLog Path"], text);
        }
        private void ReadSeeds()
        {
            string[] lines = System.IO.File.ReadAllLines(mod.Values["SeedLog Path"]);
            foreach (string line in lines)
            {
                if (line.IndexOf("Level: ") == -1 || line.IndexOf(" seed: ") == - 1)
                {
                    continue;
                }
                string[] spl = line.Split(new string[] { "Level: " }, StringSplitOptions.None);
                string[] data = spl[0].Split(new string[] { " seed: " }, StringSplitOptions.None);
                seeds.Add(Convert.ToInt32(data[0]), Convert.ToInt32(data[1]));
            }
        }
        private void LogRoomData(List<global::Room> rooms)
        {
            mod.Log("Logging room data:");
            string text = "";
            foreach (Room r in rooms)
            {
                mod.Log("Name: " + r.name + " event: " + r.StaticRoomEvent + "UID: " + r.UniqueID);
                text += "Name: " + r.name + " event: " + r.StaticRoomEvent + "UID: " + r.UniqueID + "\n";
            }

        }
        private void ReadRoomData()
        {
            mod.Log("Reading room data...");
            string[] lines = System.IO.File.ReadAllLines(mod.Values["RoomData Path"]);
            // Need to somehow set various rooms' data to what is provided in this data
        }
    }
}
