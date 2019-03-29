using DustDevilFramework;
using System;
using System.Collections.Generic;

namespace TASTools_Mod
{
    class TASIO
    {
        public static void WriteTAS(Dictionary<int, List<TASInput>> inputs, string filePath)
        {
            foreach (int level in TASInput.inputs.Keys)
            {
                Dungeon d = SingletonManager.Get<Dungeon>(false);
                string[] stringInputs = new string[inputs[level].Count + 1];
                stringInputs[0] = ":" + level + ":" + TASInput.seeds.GetSeedForShipLevel(d.ShipName, d.Level);
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
