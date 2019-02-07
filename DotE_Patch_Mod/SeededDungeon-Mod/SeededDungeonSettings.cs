using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeededDungeon_Mod
{
    public class SeededDungeonSettings : ModSettings
    {
        public bool LogSeeds = true;
        public bool ReadFromSeedLog = false;
        public string SeedLogPath = "SeedLog.txt";
        public string RoomDataPath = "RoomData.txt";
        public SeededDungeonSettings(string name) : base(name)
        {
        }
    }
}
