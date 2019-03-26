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
        ScadMod mod = new ScadMod("SeededDungeon", typeof(SeededDungeonSettings), typeof(SeededDungeonMod));
        Dictionary<int, List<int>> seeds = new Dictionary<int, List<int>>();
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
                // When the level loads, need to instead load a seed from config, for example
                // So the only things that seem to exist in the same locations are the layouts, layout sizes, and exits
                // Other static/dynamic events are unknown based off of solely Seed
                // However... What if I used recursive logging to find all of the data i need, then use recursive reading/setting in the same order.
                On.DungeonGenerator2.GenerateDungeonCoroutine += DungeonGenerator2_GenerateDungeonCoroutine;
                On.Dungeon.FillDungeonAsync += Dungeon_FillDungeonAsync;
                On.Dungeon.SetGenerationParams += Dungeon_SetGenerationParams;
                On.Dungeon.SetNewGenerationSeed += Dungeon_SetNewGenerationSeed;
            }
            if ((mod.settings as SeededDungeonSettings).ReadFromSeedLog)
            {
                ReadSeeds();
            }
        }

        private void Dungeon_SetNewGenerationSeed(On.Dungeon.orig_SetNewGenerationSeed orig)
        {
            mod.Log("Setting new GenerationSeed!");
            mod.Log("Dungeon.SetNewGenerationSeed");
            orig();
        }

        public void UnLoad()
        {
            mod.UnLoad();
            On.DungeonGenerator2.GenerateDungeonCoroutine -= DungeonGenerator2_GenerateDungeonCoroutine;
            On.Dungeon.FillDungeonAsync -= Dungeon_FillDungeonAsync;
            On.Dungeon.SetGenerationParams -= Dungeon_SetGenerationParams;
            On.Dungeon.SetNewGenerationSeed -= Dungeon_SetNewGenerationSeed;
        }

        private void Dungeon_SetGenerationParams(On.Dungeon.orig_SetGenerationParams orig, DungeonGenerationParams genParams)
        {
            mod.Log("Generation Seed: " + genParams.GenerationSeed);
            mod.Log("Final Seed: " + genParams.PostGenerationSeed);
            orig(genParams);
        }

        private System.Collections.IEnumerator Dungeon_FillDungeonAsync(On.Dungeon.orig_FillDungeonAsync orig, Dungeon self)
        {
            mod.Log("Filling Dungeon Async");
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
                new DynData<DungeonGenerator2>(self).Set<int>("randomSeed", seeds[level][0]);
                RandomGenerator.SetSeed(seeds[level][1]);
                UnityEngine.Random.seed = seeds[level][2];
            }
            yield return orig(self, level, shipName);
            if (!seeds.ContainsKey(level))
            {
                // And if I actually want to overwrite the existing seed...
                int seed = new DynData<DungeonGenerator2>(self).Get<int>("randomSeed");
                mod.Log("Adding a seed to the seeds dictionary: " + seed);
                seeds.Add(level, new List<int> { seed, RandomGenerator.Seed, UnityEngine.Random.seed });
                if ((mod.settings as SeededDungeonSettings).LogSeeds)
                {
                    LogSeeds();
                }
            }
            LogRoomData(GetRoomList(SingletonManager.Get<Dungeon>(false)));
            yield break;
        }
        private void LogSeeds()
        {
            string text = "Seeds for each level in most recently played Pod\n";
            foreach (int level in seeds.Keys)
            {
                text += "Level: " + level + " Dungeon Seed: " + seeds[level][0] + " RandomGenerator Seed: " + seeds[level][1] + " UnityEngine Seed: " + seeds[level][2] + "\n";
            }
            System.IO.File.WriteAllText((mod.settings as SeededDungeonSettings).SeedLogPath, text);
        }
        private void ReadSeeds()
        {
            string[] lines = System.IO.File.ReadAllLines((mod.settings as SeededDungeonSettings).SeedLogPath);
            foreach (string line in lines)
            {
                if (line.IndexOf("Level: ") == -1 || line.IndexOf(" Seed: ") == -1)
                {
                    continue;
                }
                string[] spl = line.Split(new string[] { "Level: " }, StringSplitOptions.None);
                string[] data = spl[0].Split(new string[] { " Dungeon Seed: " }, StringSplitOptions.None);
                string[] randomGen = data[1].Split(new string[] { " RandomGenerator Seed: " }, StringSplitOptions.None);
                string[] ex = randomGen[1].Split(new string[] { " UnityEngine Seed: " }, StringSplitOptions.None);
                seeds.Add(Convert.ToInt32(data[0]), new List<int> { Convert.ToInt32(randomGen[0]), Convert.ToInt32(ex[0]), Convert.ToInt32(ex[1]) });
            }
        }
        private void LogRoomData(List<Room> rooms)
        {
            mod.Log("Logging room data:");
            string text = "Room Count: " + rooms.Count + "\n";
            foreach (Room r in rooms)
            {
                mod.Log("Name: " + r.name + " event: " + r.StaticRoomEvent + "UID: " + r.UniqueID);
                text += "Name: " + r.name + " event: " + r.StaticRoomEvent + "UID: " + r.UniqueID + "\n";
            }
            System.IO.File.WriteAllText((mod.settings as SeededDungeonSettings).RoomDataPath, text);

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
            mod.Log("RoomList Count: " + rooms.Count);
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
