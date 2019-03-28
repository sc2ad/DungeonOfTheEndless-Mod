using Amplitude.Unity.Framework;
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
        Dictionary<Amplitude.StaticString, Dictionary<int, SeedData>> shipSeeds = new Dictionary<Amplitude.StaticString, Dictionary<int, SeedData>>();
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
                On.Dungeon.TriggerEvents += Dungeon_TriggerEvents;
                On.ShipConfig.GetLocalizedDescription += ShipConfig_GetLocalizedDescription;
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
            On.ShipConfig.GetLocalizedDescription -= ShipConfig_GetLocalizedDescription;
            On.Dungeon.TriggerEvents -= Dungeon_TriggerEvents;
        }

        private System.Collections.IEnumerator Dungeon_TriggerEvents(On.Dungeon.orig_TriggerEvents orig, Dungeon self, Room openingRoom, HeroMobCommon opener, bool canTriggerDungeonEvent)
        {
            mod.Log("Triggering Events!");
            RandomGenerator.RestoreSeed();
            mod.Log("Restoring Seed!");
            int temp = RandomGenerator.Seed;
            RandomGenerator.RestoreSeed();
            int saved = RandomGenerator.Seed;
            RandomGenerator.SetSeed(temp);

            mod.Log("Before SeedData: " + new SeedData() + " saved: " + saved);

            // Calls TriggerEvents the proper number of times!
            yield return self.StartCoroutine(orig(self, openingRoom, opener, canTriggerDungeonEvent));
            temp = RandomGenerator.Seed;
            RandomGenerator.RestoreSeed();
            saved = RandomGenerator.Seed;
            RandomGenerator.SetSeed(temp);
            mod.Log("After SeedData: " + new SeedData() + " saved: " + saved);

            mod.Log("Saving Seed!");
            RandomGenerator.SaveSeed();
            yield break;
        }

        private string ShipConfig_GetLocalizedDescription(On.ShipConfig.orig_GetLocalizedDescription orig, ShipConfig self)
        {
            if (shipSeeds.ContainsKey(self.Name) && shipSeeds[self.Name].ContainsKey(1))
            {
                // If the seed for the first level exists, display the seed at the bottom of the description.
                string temp = orig(self) + "\n\n#98FB98#Seeds: " + shipSeeds[self.Name][1] + "#98FB98#";
                mod.Log("Attempting to update description to include seeds!");
                return temp;
            }
            return orig(self);
        }

        private System.Collections.IEnumerator DungeonGenerator2_GenerateDungeonCoroutine(On.DungeonGenerator2.orig_GenerateDungeonCoroutine orig, DungeonGenerator2 self, int level, Amplitude.StaticString shipName)
        {
            if (shipSeeds.ContainsKey(shipName) && shipSeeds[shipName].ContainsKey(level))
            {
                // And if I actually want to apply the seed change...
                mod.Log("Creating the Dungeon with the provided seed from the dictionary: " + shipSeeds[shipName][level]);
                SingletonManager.Get<Dungeon>(false).EnqueueNotification("Using Seeds: " + shipSeeds[shipName][level]);
                shipSeeds[shipName][level].SetSeedData();
            }
            yield return orig(self, level, shipName);
            if (!shipSeeds.ContainsKey(shipName))
            {
                shipSeeds.Add(shipName, new Dictionary<int, SeedData>());
            }
            if (!shipSeeds[shipName].ContainsKey(level))
            {
                // And if I actually want to overwrite the existing seed...
                int seed = new DynData<DungeonGenerator2>(self).Get<int>("randomSeed");
                mod.Log("Adding a seed to the seeds dictionary: " + seed);
                shipSeeds[shipName].Add(level, new SeedData(seed, RandomGenerator.Seed, UnityEngine.Random.seed));
                SingletonManager.Get<Dungeon>(false).EnqueueNotification("Added Seeds: " + shipSeeds[shipName][level]);
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
            foreach (Amplitude.StaticString name in shipSeeds.Keys)
            {
                Dictionary<int, SeedData> seeds = shipSeeds[name];
                foreach (int level in seeds.Keys)
                {
                    text += "N:" + name + ":L:" + level + ":" + seeds[level] + "\n";
                }
            }
            System.IO.File.WriteAllText((mod.settings as SeededDungeonSettings).SeedLogPath, text);
        }
        private void ReadSeeds()
        {
            mod.Log("Reading Seeds from: " + (mod.settings as SeededDungeonSettings).SeedLogPath);
            string[] lines = System.IO.File.ReadAllLines((mod.settings as SeededDungeonSettings).SeedLogPath);
            foreach (string line in lines)
            {
                if (line.IndexOf("N:") == -1)
                {
                    continue;
                }
                string[] spl = line.Split(new string[] { ":" }, StringSplitOptions.None);
                Amplitude.StaticString name = spl[1];
                string level = spl[3];
                string data = spl[spl.Length - 1];
                mod.Log("Adding Seed for ship: " + name + " with level: " + level + " with data: " + data);
                if (!shipSeeds.ContainsKey(name))
                {
                    shipSeeds.Add(name, new Dictionary<int, SeedData>());
                }
                shipSeeds[name].Add(Convert.ToInt32(level), new SeedData(data));
            }
        }
        private void LogRoomData(List<Room> rooms)
        {
            mod.Log("Logging room data:");
            string text = "Room Count: " + rooms.Count + "\n";
            foreach (Room r in rooms)
            {
                mod.Log("Name: " + r.name + " event: " + r.StaticRoomEvent + " UID: " + r.UniqueID + " Dust: " + r.DustLootAmount);
                text += "Name: " + r.name + " event: " + r.StaticRoomEvent + " UID: " + r.UniqueID + " Dust: " + r.DustLootAmount + "\n";
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
