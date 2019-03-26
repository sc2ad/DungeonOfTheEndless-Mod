using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TASTools_Mod
{
    public class TASToolsSettings : ModSettings
    {
        public string TasFileExtension = @".tas";
        public bool PlayFromFile = false;
        public TASToolsSettings(string name) : base(name)
        {
        }
    }
}
