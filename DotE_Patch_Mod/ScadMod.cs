using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotE_Patch_Mod
{
    class ScadMod
    {
        public string path = @"placeholder_log.txt";

        public string config = @"placeholder_config.txt";
        public string default_config = "# Modify this file to change various settings of the DustGenerator Mod for DotE.\n# If these values are removed, the game will crash on initialization!\n";

        public Dictionary<string, string> Values = new Dictionary<string, string>();

        public void Log(string s)
        {
            System.IO.File.AppendAllText(path, s + "\n");
        }
        public void ClearLog()
        {
            System.IO.File.WriteAllText(path, "");
        }
        public void ReadConfig()
        {
            Log("Attempting to read from config file: " + config);
            if (!System.IO.File.Exists(config))
            {
                Log("Need to create a new config file at: " + config);
                string s = default_config;
                foreach (string q in Values.Keys)
                {
                    s += q + ": " + Values[q] + "\n";
                }
                System.IO.File.WriteAllText(config, s);
            }
            string[] lines = System.IO.File.ReadAllLines(config);
            foreach (string line in lines)
            {
                if (line.StartsWith("#") || line.IndexOf(": ") == -1)
                {
                    continue;
                }
                string[] spl = line.Split(new string[] { ": " }, StringSplitOptions.None);
                string value = spl[1].Trim();
                Values[spl[0]] = value;
                Log("Loaded: " + spl[0] + " = " + Values[spl[0]] + " from config");
            }
            Log("Values have been loaded successfully!");
        }
        public void Initalize()
        {
            ClearLog();
            Log("===================================================================");
            Log("DATETIME: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString());
            Log("===================================================================");

            Values.Add("Enabled", "True");
            // Add values here to the Values dictionary!
        }
        public void Load()
        {
            Log("Loaded!");
            
            // Load/Create hooks here!
        }
    }
}
