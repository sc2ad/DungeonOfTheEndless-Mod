using DustDevilFramework;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedFinder_Mod
{
    public class SeedFinderMod : PartialityMod
    {
        ScadMod mod = new ScadMod("SeedFinder", typeof(SeedFinderSettings), typeof(SeedFinderMod));
        List<int> seeds = new List<int>();
        SeedComparatorType comparator;

        public override void Init()
        {
            mod.BepinPluginReference = this;
            mod.Initialize();

            mod.settings.ReadSettings();
            comparator = new SeedComparatorType((SeedComparatorType.ComparatorType) Enum.Parse(typeof(SeedComparatorType.ComparatorType), (mod.settings as SeedFinderSettings).ComparatorType));

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                On.GameClientState_AllDungeonsFilled.Begin += GameClientState_AllDungeonsFilled_Begin; ;
            }
        }

        private void GameClientState_AllDungeonsFilled_Begin(On.GameClientState_AllDungeonsFilled.orig_Begin orig, GameClientState_AllDungeonsFilled self, object[] parameters)
        {
            // Test to see if the seed has already been covered or not
            if (seeds.Contains(SingletonManager.Get<DungeonGenerator2>(true).RandomSeed))
            {
                //orig(self, abort);
                return;
            }
            // Otherwise, create more!
            mod.Log("Attempting to test seeds...");
            int tries = 0;
            int bestSeed = (mod.settings as SeedFinderSettings).LowestSeed;
            if ((mod.settings as SeedFinderSettings).UseRandom)
            {
                mod.Log("Using random seed selection!");

                while (seeds.Count < (long)(mod.settings as SeedFinderSettings).LargestSeed - (mod.settings as SeedFinderSettings).LowestSeed)
                {
                    // Find a new random seed and test that. Also use the provided delegate depending on what we are searching for.
                    int seedToTest = RandomGenerator.RangeInt((mod.settings as SeedFinderSettings).LowestSeed, (mod.settings as SeedFinderSettings).LargestSeed);
                    if (seeds.Contains(seedToTest))
                    {
                        continue;
                    }
                    if (tries % 10 == 0)
                    {
                        mod.Log("Testing seed: " + seedToTest + " with bestSeed: " + bestSeed + " and bestDepth: " + comparator.bestDepth);
                    }
                    seeds.Add(seedToTest);
                    Util.SetSeed(new SeedData(seedToTest, RandomGenerator.Seed, UnityEngine.Random.seed));
                    SingletonManager.Get<DungeonGenerator2>(false).GenerateDungeonUsingSeedCoroutine(SingletonManager.Get<Dungeon>(false).Level, seedToTest, SingletonManager.Get<Dungeon>(false).ShipName);
                    if (comparator.comparison(bestSeed, seedToTest) > 0)
                    {
                        mod.Log("Found a new best seed! Replacing: " + bestSeed + " With: " + seedToTest + " with new depth: " + comparator.bestDepth);
                        bestSeed = seedToTest;
                    }
                    tries++;
                }
            }
            else
            {
                mod.Log("Using iterative seed selection!");

                for (int seed = (mod.settings as SeedFinderSettings).LowestSeed; seed <= (mod.settings as SeedFinderSettings).LargestSeed; seed++)
                {
                    if (tries % 10 == 0)
                    {
                        mod.Log("Testing seed: " + seed + " with bestSeed: " + bestSeed + " and bestDepth: " + comparator.bestDepth);
                    }
                    seeds.Add(seed);
                    Util.SetSeed(new SeedData(seed, RandomGenerator.Seed, UnityEngine.Random.seed));
                    SingletonManager.Get<DungeonGenerator2>(false).GenerateDungeonUsingSeedCoroutine(SingletonManager.Get<Dungeon>(false).Level, seed, SingletonManager.Get<Dungeon>(false).ShipName);
                    if (comparator.comparison(bestSeed, seed) > 0)
                    {
                        mod.Log("Found a new best seed! Replacing: " + bestSeed + " With: " + seed + " with new depth: " + comparator.bestDepth);
                        bestSeed = seed;
                    }
                    tries++;
                }
            }
            mod.Log("Final best seed: " + bestSeed + " with depth: " + comparator.bestDepth);
            orig(self, parameters);
        }

        public void UnLoad()
        {
            mod.UnLoad();
            On.GameClientState_AllDungeonsFilled.Begin -= GameClientState_AllDungeonsFilled_Begin;
        }


    }
}
