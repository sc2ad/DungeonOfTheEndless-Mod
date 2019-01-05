using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustlessPod_Mod
{
    public class DustlessPodSettings : PodSettings
    {
        [SettingsRange(1,100)]
        public float MinDustLoot = 1;
        [SettingsRange(1,100)]
        public float MaxDustLoot = 6;
        [SettingsRange(0,100)]
        public int dustLootProbability = 30;
        public float DustLootProbability { get { return dustLootProbability / 100.0f; } set { dustLootProbability = (int)(100 * value); } }
        public DustlessPodSettings() : base("DustlessPod")
        {
        }
    }
}
