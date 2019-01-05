using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DustDevilFramework
{
    public class ModSettings
    {
        [SettingsIgnore]
        private string name;
        public bool Enabled { get; set; } = true;
        public ModSettings(string name)
        {
            this.name = name;
        }
        public void WriteSettings()
        {
            string config = name + "_config.txt";

            string s = "";
            FieldInfo[] fields = GetType().GetFields();
            foreach (FieldInfo q in fields)
            {
                if (q.IsPrivate)
                {
                    continue;
                }
                s += q.Name + ": " + q.GetValue(this) + "\n";
            }
            System.IO.File.WriteAllText(config, s);
        }
        public void ReadSettings()
        {
            string config = name + "_config.txt";

            if (!System.IO.File.Exists(config))
            {
                WriteSettings();
                return;
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
                foreach (FieldInfo f in GetType().GetFields())
                {
                    if (f.Name == spl[0])
                    {
                        // Will this work, cause spl[1] is a string?
                        f.SetValue(this, spl[1]);
                        break;
                    }
                }
                // This shouldn't happen, this means that something has gone wrong and that the field does not exist
                Debug.Log("No fields with name matching: " + spl[0]);
            }
        }
        public bool Exists()
        {
            return System.IO.File.Exists(name + "_config.txt");
        }
        [AttributeUsage(AttributeTargets.Field)]
        public class SettingsRange : Attribute
        {
            private int low;
            private int high;
            // Constructs a range of low-high
            public SettingsRange(int low, int high)
            {
                this.low = low;
                this.high = high;
            }
        }
        public class SettingsIgnore : Attribute
        {
            // Constructs a setting that is ignored, does not appear in the gui
        }
    }
}
