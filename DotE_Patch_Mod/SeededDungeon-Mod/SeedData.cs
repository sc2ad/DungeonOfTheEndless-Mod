using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeededDungeon_Mod
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
}
