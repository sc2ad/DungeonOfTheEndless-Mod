using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExitFinder_Mod
{
    public class ExitFinderSettings : ModSettings
    {
        public bool DisplayExit = true;
        public string Key = "Backslash";
        public ExitFinderSettings(string name) : base(name)
        {
        }
    }
}
