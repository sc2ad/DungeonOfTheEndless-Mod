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
        public bool Enabled = true;
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
                if (q.Name == "name")
                {
                    continue;
                }
                s += q.Name + ": " + q.GetValue(this) + "\n";
            }
            Debug.Log("Wrote settings to file: " + config);
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
                bool temp = false;
                foreach (FieldInfo f in GetType().GetFields())
                {
                    //Debug.Log("Observed Field with name: " + f.Name);
                    if (f.Name == spl[0])
                    {
                        // Will this work, cause spl[1] is a string? answer: no
                        try
                        {
                            f.SetValue(this, spl[1]);
                        }
                        catch (ArgumentException _)
                        {
                            try
                            {
                                f.SetValue(this, (float)Convert.ToDouble(spl[1]));
                            }
                            catch (FormatException __)
                            {
                                f.SetValue(this, Convert.ToBoolean(spl[1]));
                            }
                            catch (ArgumentException __)
                            {
                                f.SetValue(this, Convert.ToBoolean(spl[1]));
                            }
                        }
                        Debug.Log("Set Field with name: " + spl[0] + " to: " + spl[1]);
                        temp = true;
                        break;
                    }
                }
                if (temp)
                {
                    continue;
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
            public float Low { get; set; }
            public float High { get; set; }
            public float Increment { get; set; } = 1;
            // Constructs a range of low-high
            public SettingsRange(float low, float high)
            {
                Low = low;
                High = high;
            }
            public SettingsRange(float low, float high, float increment)
            {
                Low = low;
                High = high;
                Increment = increment;
            }
        }
        [AttributeUsage(AttributeTargets.Field)]
        public class SettingsIgnore : Attribute
        {
            // Constructs a setting that is ignored, does not appear in the gui
        }
    }
}
