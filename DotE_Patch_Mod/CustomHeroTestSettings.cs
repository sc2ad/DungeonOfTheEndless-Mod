using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomHero_Mod
{
    public class CustomHeroTestSettings : CustomHeroSettings
    {
        public string HeroToReplace = "Skroig";
        public string HeroName = "Skroggerz";
        public bool ReplaceHero = false;
        [SettingsRange(100, 100000)]
        public float HeroHP = 1000;
        [SettingsRange(10,1000)]
        public float Speed = 80;
        public bool LogHeroData = true;
        public string PathToRanks = "Ranks.txt";
        public CustomHeroTestSettings(string name) : base(name)
        {
        }
    }
}
