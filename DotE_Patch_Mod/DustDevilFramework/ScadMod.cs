using System;
using System.Collections.Generic;
using System.Text;

namespace DustDevilFramework
{
    public class ScadMod
    {
        public string name = "placeholder";
        public string path;

        public ModSettings settings = new ModSettings("placeholder");
        public Type settingsType;
        public Type PartialityModType;
        public object PartialityModReference;

        public int MajorVersion = 1;
        public int MinorVersion = 0;
        public int Revision = 0;

        public ScadMod(string name, Type settingsType, Type partialityModType)
        {
            this.name = name;
            settings = Activator.CreateInstance(settingsType, new object[] { name }) as ModSettings;
            this.settingsType = settingsType;
            path = name + "_log.txt";
            PartialityModType = partialityModType;
        }
        public ScadMod(string name, Type partialityModType)
        {
            this.name = name;
            settings = new ModSettings(name);
            settingsType = typeof(ModSettings);
            path = name + "_log.txt";
            PartialityModType = partialityModType;
        }
        // Don't use this constructor unless you plan on setting settingsType and PartialityModType later!
        public ScadMod()
        {
            path = name + "_log.txt";
        }
        public void Log(string s)
        {
            System.IO.File.AppendAllText(path, s + "\n");
        }
        public void ClearLog()
        {
            System.IO.File.WriteAllText(path, "");
        }
        public void Initialize()
        {
            ClearLog();
            Log("===================================================================");
            Log("DATETIME: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString());
            Log("===================================================================");

            settings.ReadSettings();
            DustDevil.CreateInstance(this);
        }
        public void Load()
        {
            Log("Loaded!");
            
            // Load/Create hooks here!
        }
        public void UnLoad()
        {
            Log("Unloaded!");

            // Unload all of the loaded hooks here!
        }
    }
}
