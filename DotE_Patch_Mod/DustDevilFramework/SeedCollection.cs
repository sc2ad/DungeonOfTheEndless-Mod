using Amplitude;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DustDevilFramework
{
    [Serializable]
    public class SeedData
    {
        public int DungeonSeed;
        public int RandomGeneratorSeed;
        public int UnityEngineSeed;

        public static int RandomGeneratorSavedSeed = 0;

        public SeedData()
        {
            DungeonSeed = new DynData<DungeonGenerator2>(SingletonManager.Get<DungeonGenerator2>(true)).Get<int>("randomSeed");
            RandomGeneratorSeed = RandomGenerator.Seed;
            UnityEngineSeed = UnityEngine.Random.seed;
        }
        public SeedData(int d, int r, int u)
        {
            DungeonSeed = d;
            RandomGeneratorSeed = r;
            UnityEngineSeed = u;
        }
        public SeedData(string s)
        {
            string[] data = s.Split(',');
            DungeonSeed = Convert.ToInt32(data[0]);
            RandomGeneratorSeed = Convert.ToInt32(data[1]);
            UnityEngineSeed = Convert.ToInt32(data[2]);
        }
        public void SetSeedData()
        {
            new DynData<DungeonGenerator2>(SingletonManager.Get<DungeonGenerator2>(true)).Set<int>("randomSeed", DungeonSeed);
            RandomGenerator.SetSeed(RandomGeneratorSeed);
            UnityEngine.Random.seed = UnityEngineSeed;
            // Saves Seed for TriggerEvents Consistency!
            RandomGeneratorSavedSeed = RandomGeneratorSeed;
            RandomGenerator.SaveSeed();
        }
        public override string ToString()
        {
            return DungeonSeed + "," + RandomGeneratorSeed + "," + UnityEngineSeed;
        }
        public static void SetSeedData(SeedData data)
        {
            data.SetSeedData();
        }
    }
    [Serializable]
    public class SeedCollection
    {
        private const string DateTimeFormat = "dd-MM-yyyy--hh-mm-ss";
        private const string SeedColor = "#98FB98#";

        private Dictionary<StaticString, Dictionary<int, SeedData>> seeds;

        public bool EnqueNotification;
        public bool AddNewSeeds;
        public bool Enabled;
        public string ReadFrom;

        public static List<SeedCollection> Collections;

        public static SeedCollection Create()
        {
            if (Collections == null)
            {
                Collections = new List<SeedCollection>();
                // Only called once
                On.DungeonGenerator2.GenerateDungeonCoroutine += DungeonGenerator2_GenerateDungeonCoroutine;
                On.Dungeon.TriggerEvents += Dungeon_TriggerEvents;
                On.ShipConfig.GetLocalizedDescription += ShipConfig_GetLocalizedDescription;
            }
            Collections.Add(new SeedCollection());
            return Collections[Collections.Count - 1];
        }

        private static string ShipConfig_GetLocalizedDescription(On.ShipConfig.orig_GetLocalizedDescription orig, ShipConfig self)
        {
            SeedCollection collection = GetMostCurrentSeeds(self.Name, 1);
            if (collection != null && collection.Enabled)
            {
                return orig(self) + "\n\n" + SeedColor + "Seeds: " + collection.GetSeedForShipLevel(self.Name, 1) + SeedColor;
            }
            return orig(self);
        }

        private static IEnumerator Dungeon_TriggerEvents(On.Dungeon.orig_TriggerEvents orig, Dungeon self, Room openingRoom, HeroMobCommon opener, bool canTriggerDungeonEvent)
        {
            SeedCollection collection = GetMostCurrentSeeds(self.ShipName, self.Level);
            if (collection == null || !collection.Enabled)
            {
                yield return self.StartCoroutine(orig(self, openingRoom, opener, canTriggerDungeonEvent));
                yield break;
            }
            else
            {
                RandomGenerator.RestoreSeed();

                // Calls TriggerEvents the proper number of times. Hangs on this call.
                // Random deviation seems to appear while this Coroutine is running, possibly due to monster random actions?
                // Could fix this by storing all before/after seeds, but doesn't that seem lame?
                // Would like to find a way of only wrapping the Random calls with this so that there is less UnityEngine.Random.seed
                // noise from other sources that occur during the runtime.
                // The above will probably not work, so instead wrap everything EXCEPT for the wait in the Random Save/Restore
                // Possible error from SpawnWaves, SpawnMobs (cause they have dedicated Coroutines that run)
                yield return self.StartCoroutine(orig(self, openingRoom, opener, canTriggerDungeonEvent));

                // I'm going to cheat for now and SKIP the saving step - the same exact seed is ALWAYS used for RandomGenerator
                // When using RandomGenerator seeds.
                //mod.Log("Saving Seed!");
                //RandomGenerator.SaveSeed();

                yield break;
            }
        }

        private static IEnumerator DungeonGenerator2_GenerateDungeonCoroutine(On.DungeonGenerator2.orig_GenerateDungeonCoroutine orig, DungeonGenerator2 self, int level, StaticString shipName)
        {
            // Preferably, sort all SeedCollections in Collections by most current file
            SeedCollection collection = GetMostCurrentSeeds(shipName, level);
            if (collection != null)
            {
                if (collection.Enabled)
                {
                    if (collection.EnqueNotification)
                    {
                        SingletonManager.Get<Dungeon>(false).EnqueueNotification("Using Seeds: " + collection.GetSeedForShipLevel(shipName, level));
                    }
                    collection.SetSeedForShipLevel(shipName, level);
                }
                if (collection.AddNewSeeds)
                {
                    if (collection.GetSeedsForShip(shipName) == null)
                    {
                        collection.Add(shipName);
                    }
                    collection.Add(shipName, level, new SeedData());
                    if (collection.EnqueNotification)
                    {
                        SingletonManager.Get<Dungeon>(false).EnqueueNotification("Added Seeds: " + collection.GetSeedForShipLevel(shipName, level));
                    }
                }
            }
            yield return orig(self, level, shipName);
        }

        public static SeedCollection GetMostCurrentSeeds(StaticString shipName, int level)
        {
            if (Collections == null)
            {
                return null;
            }
            SeedCollection best = null;
            DateTime now = DateTime.Now;
            DateTime currentBest = DateTime.MinValue;
            foreach (SeedCollection collection in Collections)
            {
                if (collection.GetSeedForShipLevel(shipName, level) == null)
                {
                    continue;
                }
                if (collection.ReadFrom.Length <= 0)
                {
                    // This collection hasn't been written/read from, so it is the most up to date.
                    return collection;
                }
                if (File.Exists(collection.ReadFrom))
                {
                    DateTime current = File.GetLastWriteTime(collection.ReadFrom);
                    if ((now - current) < (now - currentBest))
                    {
                        best = collection;
                        currentBest = current;
                    }
                }
            }
            return best;
        }

        public static SeedCollection CreateAndRead(string path)
        {
            SeedCollection c = Create();
            c.ReadSeeds(path);
            return c;
        }
        public static void ReadAll()
        {
            foreach (string file in Directory.GetFiles(".", "*.seeds"))
            {
                // Foreach .seeds file
                CreateAndRead(file);
            }
            if (Directory.Exists("Seeds"))
            {
                foreach (string file in Directory.GetFiles(@"Seeds\", "*.seeds"))
                {
                    // Foreach .seeds file in the Seeds directory
                    CreateAndRead(file);
                }
            }
        }
        public static void WriteAll()
        {
            foreach (SeedCollection c in Collections)
            {
                // Write each SeedCollection
                if (c.ReadFrom.Length <= 0)
                {
                    // Find the first place to place it
                    if (!Directory.Exists("Seeds"))
                    {
                        Directory.CreateDirectory("Seeds");
                    }
                    c.ReadFrom = @"Seeds\" + DateTime.Now.ToString(DateTimeFormat) + ".seeds";
                }
                c.WriteSeeds(c.ReadFrom);
            }
        }
        public static void Remove(SeedCollection coll)
        {
            Collections.Remove(coll);
        }

        private SeedCollection()
        {
            seeds = new Dictionary<StaticString, Dictionary<int, SeedData>>();
            ReadFrom = "";
            AddNewSeeds = false;
            EnqueNotification = false;
            Enabled = true;
        }
        public void WriteSeeds(string file)
        {
            string text = "Seeds:\nAddNewSeeds:" + AddNewSeeds + "\nEnqueNotification:" + EnqueNotification + "\nEnabled:" + Enabled + "\n";
            foreach (StaticString name in seeds.Keys)
            {
                foreach (int level in seeds[name].Keys)
                {
                    text += "N:" + name + ":L:" + level + ":" + seeds[name][level] + "\n";
                }
            }
            File.WriteAllText(file, text);
        }
        public void ReadSeeds(string file)
        {
            ReadFrom = file;
            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                if (line.IndexOf("AddNewSeeds:") != -1)
                {
                    AddNewSeeds = Convert.ToBoolean(line.Split(new string[] { "AddNewSeeds:" }, StringSplitOptions.None)[1]);
                    continue;
                }
                if (line.IndexOf("EnqueNotification:") != -1)
                {
                    EnqueNotification = Convert.ToBoolean(line.Split(new string[] { "EnqueNotification:" }, StringSplitOptions.None)[1]);
                    continue;
                }
                if (line.IndexOf("Enabled:") != -1)
                {
                    Enabled = Convert.ToBoolean(line.Split(new string[] { "Enabled:" }, StringSplitOptions.None));
                    continue;
                }
                if (line.IndexOf("N:") == -1)
                {
                    continue;
                }
                string[] spl = line.Split(new string[] { ":" }, StringSplitOptions.None);
                StaticString name = spl[1];
                string level = spl[3];
                string data = spl[spl.Length - 1];
                if (!seeds.ContainsKey(name))
                {
                    Add(name);
                }
                seeds[name].Add(Convert.ToInt32(level), new SeedData(data));
            }
            if (seeds.Keys.Count == 0)
            {
                // No seeds were loaded from this file. We need to remove this reference from the list.
                Remove(this);
            }
        }
        public Dictionary<int, SeedData> GetSeedsForShip(StaticString ship)
        {
            if (!seeds.ContainsKey(ship))
            {
                return null;
            }
            return seeds[ship];
        }
        public SeedData GetSeedForShipLevel(StaticString ship, int level)
        {
            if (!seeds.ContainsKey(ship) || !seeds[ship].ContainsKey(level))
            {
                return null;
            }
            return seeds[ship][level];
        }
        public void SetSeedForShipLevel(StaticString ship, int level)
        {
            GetSeedForShipLevel(ship, level).SetSeedData();
        }
        public void Add(StaticString ship)
        {
            if (seeds.ContainsKey(ship))
            {
                return;
            }
            seeds.Add(ship, new Dictionary<int, SeedData>());
        }
        public void Add(StaticString ship, int level, SeedData data)
        {
            if (!seeds.ContainsKey(ship))
            {
                Add(ship);
            }
            if (seeds[ship].ContainsKey(level))
            {
                return;
            }
            seeds[ship].Add(level, data);
        }
        public void Remove(StaticString ship, int level)
        {
            if (!seeds.ContainsKey(ship) || seeds[ship].ContainsKey(level))
            {
                return;
            }
            seeds[ship].Remove(level);
        }
        public void Remove(StaticString ship)
        {
            if (!seeds.ContainsKey(ship))
            {
                return;
            }
            seeds.Remove(ship);
        }
    }
}
