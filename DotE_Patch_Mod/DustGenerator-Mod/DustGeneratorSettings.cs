using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustGenerator_Mod
{
    public class DustGeneratorSettings : ModSettings
    {
        [SettingsRange(0,100)]
        public float DustPerDoor = 10.0f;
        public bool DustFromProducing = true;
        public bool DustFromRoom = false;
        public DustGeneratorSettings() : base("DustGenerator")
        {
        }
    }
}
