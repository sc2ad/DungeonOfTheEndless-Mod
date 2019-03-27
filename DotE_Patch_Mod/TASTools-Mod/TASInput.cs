using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASTools_Mod
{
    class TASInput
    {
        public static Dictionary<int, List<TASInput>> inputs = new Dictionary<int, List<TASInput>>();
        public static Dictionary<int, SeedData> seeds = new Dictionary<int, SeedData>();

        public static void CreateAndAdd(int level)
        {
            new TASInput(level);
        }
        public static void AddSeed(int level, SeedData data)
        {
            if (!seeds.ContainsKey(level))
            {
                seeds.Add(level, data);
            }
        }
        public static void Clear()
        {
            inputs = new Dictionary<int, List<TASInput>>();
            seeds = new Dictionary<int, SeedData>();
        }
        public static void ClearForLevel(int level)
        {
            inputs[level] = null;
            seeds[level] = null;
        }

        public string input;
        private void add(int level)
        {
            if (!inputs.ContainsKey(level))
            {
                inputs.Add(level, new List<TASInput>());
            }
            inputs[level].Add(this);
        }
        public TASInput(int level)
        {
            input = UnityEngine.Input.inputString;
            add(level);
        }
        public TASInput(int level, string s)
        {
            input = s;
            add(level);
        }
        public override string ToString()
        {
            return input;
        }
    }
}
