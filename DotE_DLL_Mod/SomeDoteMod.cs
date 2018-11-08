using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Partiality.Modloader;

namespace DotE_DLL_Mod
{
    public class SomeDoteMod : PartialityMod
    {
        private static string path = @"C:\Program Files (x86)\Steam\steamapps\common\Dungeon of the Endless\SomeDoteMod_log.txt";
        private static void Log(string s)
        {
            System.IO.File.AppendAllText(path, s+"\n");
        }
        public override void Init()
        {
            Log("Initialized!");
        }
        public override void OnLoad()
        {
            Log("Loaded!");
            
        }
        public override void OnEnable()
        {
            Log("Enabled!");
        }
        public override void OnDisable()
        {
            Log("Disabled!");
        }
    }
}
