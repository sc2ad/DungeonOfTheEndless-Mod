using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimpleCheats_Mod
{
    public class SimpleCheatsSettings : ModSettings
    {
        public string IndustryKey = KeyCode.M.ToString();
        public string ScienceKey = KeyCode.Comma.ToString();
        public string FoodKey = KeyCode.Period.ToString();
        public string DustKey = KeyCode.Slash.ToString();
        [SettingsRange(0, 100)]
        public float IncrementAmount = 10;
        public SimpleCheatsSettings(string name) : base(name)
        {
        }
    }
}
