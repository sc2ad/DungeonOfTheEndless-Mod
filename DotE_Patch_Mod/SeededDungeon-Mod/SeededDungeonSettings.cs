using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SeededDungeon_Mod
{
    public class SeededDungeonSettings : ModSettings
    {
        public string SaveKey = KeyCode.Backspace.ToString();
        public string CreateNewSeedKey = KeyCode.Equals.ToString();

        public SeededDungeonSettings(string name) : base(name)
        {
        }
    }
}
