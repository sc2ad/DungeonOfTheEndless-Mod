using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASTools_Mod
{
    class TASIO
    {
        public static void WriteTAS(Dictionary<int, List<TASInput>> inputs, string filePath)
        {
            foreach (int level in TASInput.seeds.Keys)
            {
                if (!inputs.ContainsKey(level))
                {
                    // The seed exists but there is no TAS data for this level!
                    // Make it anyway, but with an empty amount of data.
                    inputs.Add(level, new List<TASInput>());
                }
                string[] stringInputs = new string[inputs[level].Count + 1];
                stringInputs[0] = ":" + level + ":" + TASInput.seeds[level].ToString();
                for (int i = 1; i < inputs[level].Count + 1; i++)
                {
                    stringInputs[i] = inputs[level][i - 1].ToString();
                }
                System.IO.File.WriteAllLines("level" + level + filePath, stringInputs);
            }
        }
        public static void ReadTAS(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            int level = -1;
            foreach (string s in lines)
            {
                if (s.StartsWith("#"))
                {
                    continue;
                }
                if (s.StartsWith(":"))
                {
                    string[] spl = s.Split(':');
                    level = Convert.ToInt32(spl[1]);
                    TASInput.AddSeed(level, new SeedData(spl[2]));
                    continue;
                }
                TASInput i = new TASInput(level, s);
            }
        }
    }
}
