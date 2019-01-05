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
        ScadMod mod = new ScadMod("SeededDungeon", typeof(SeededDungeonSettings));
        Dictionary<int, int> seeds = new Dictionary<int, int>();
        public override void Init()
        {
            mod.Initialize();

            mod.settings.ReadSettings();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                // When the level loads, need to instead load a seed from config, for example
                // So the only things that seem to exist in the same locations are the layouts, layout sizes, and exits
                // Other static/dynamic events are unknown based off of solely Seed
                // However... What if I used recursive logging to find all of the data i need, then use recursive reading/setting in the same order.
                On.DungeonGenerator2.GenerateDungeonCoroutine += DungeonGenerator2_GenerateDungeonCoroutine;
                On.Dungeon.FillDungeonAsync += Dungeon_FillDungeonAsync;
            }
            if ((mod.settings as SeededDungeonSettings).ReadFromSeedLog)
            {
                ReadSeeds();
            }
        }
        public void UnLoad()
        {
            mod.UnLoad();
            On.DungeonGenerator2.GenerateDungeonCoroutine -= DungeonGenerator2_GenerateDungeonCoroutine;
            On.Dungeon.FillDungeonAsync -= Dungeon_FillDungeonAsync;
        }

        private System.Collections.IEnumerator Dungeon_FillDungeonAsync(On.Dungeon.orig_FillDungeonAsync orig, Dungeon self)
        {

            yield return orig(self);
            LogRoomData(GetRoomList(self));
            yield break;
        }

        private System.Collections.IEnumerator DungeonGenerator2_GenerateDungeonCoroutine(On.DungeonGenerator2.orig_GenerateDungeonCoroutine orig, DungeonGenerator2 self, int level, Amplitude.StaticString shipName)
        {
            if (seeds.ContainsKey(level))
            {
                // And if I actually want to apply the seed change...
                mod.Log("Creating the Dungeon with the provided seed from the dictionary: " + seeds[level]);
                new DynData<DungeonGenerator2>(self).Set<int>("randomSeed", seeds[level]);
            }
            yield return orig(self, level, shipName);
            if (!seeds.ContainsKey(level))
            {
                // And if I actually want to overwrite the existing seed...
                int seed = new DynData<DungeonGenerator2>(self).Get<int>("randomSeed");
                mod.Log("Adding a seed to the seeds dictionary: " + seed);
                seeds.Add(level, seed);
                if ((mod.settings as SeededDungeonSettings).LogSeeds)
                {
                    LogSeeds();
                }
            }
            yield break;
        }
        private void LogSeeds()
        {
            string text = "Seeds for each level in most recently played Pod\n";
            foreach (int level in seeds.Keys)
            {
                text += "Level: " + level + " seed: " + seeds[level] + "\n";
            }
            System.IO.File.WriteAllText((mod.settings as SeededDungeonSettings).SeedLogPath, text);
        }
        private void ReadSeeds()
        {
            string[] lines = System.IO.File.ReadAllLines((mod.settings as SeededDungeonSettings).SeedLogPath);
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
            string[] lines = System.IO.File.ReadAllLines((mod.settings as SeededDungeonSettings).RoomDataPath);
            // Need to somehow set various rooms' data to what is provided in this data
        }
        private List<Room> GetRoomList(Dungeon d)
        {
            List<Room> rooms = new List<Room>();
            // Recursively add all adjacent rooms that don't already exist within the list until all rooms have been added
            AddRoomRecursive(d.StartRoom, rooms);
            return rooms;
        }
        private void AddRoomRecursive(Room r, List<Room> rooms)
        {
            if (rooms.Contains(r))
            {
                return;
            }
            rooms.Add(r);
            foreach (Room r2 in r.AdjacentRooms)
            {
                AddRoomRecursive(r2, rooms);
            }
        }
    }
}
