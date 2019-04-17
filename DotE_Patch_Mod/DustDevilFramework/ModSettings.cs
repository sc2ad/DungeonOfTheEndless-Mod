using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using BepInEx.Configuration;
using MonoMod.Utils;

namespace DustDevilFramework
{
    public class ModSettings
    {
        [SettingsIgnore]
        private ScadMod mod;
        [SettingsIgnore]
        private ConfigFile configFile;
        public bool Enabled = true;
        public ModSettings(ScadMod mod)
        {
            this.mod = mod;
            try
            {
                configFile = new DynData<BepInEx.BaseUnityPlugin>(mod.BepinPluginReference).Get<ConfigFile>("Config");
            } catch (Exception _)
            {
                mod.Log(BepInEx.Logging.LogLevel.Error, "Could not find config file for plugin with name: " + mod.name);
                string configPath = @"BepInEx\config\" + mod.name + ".cfg";
                mod.Log(BepInEx.Logging.LogLevel.Warning, "Attempting to use default config: " + configPath);
                configFile = new ConfigFile(configPath, true);
            }
        }
        public void WriteSettings()
        {
            FieldInfo[] fields = GetType().GetFields();
            foreach (FieldInfo q in fields)
            {
                if (q.Name == "mod" || q.Name == "configFile")
                {
                    continue;
                }
                configFile.Wrap("Settings", q.Name, null, q.GetValue(this));
            }
            configFile.Save();
            configFile.SaveOnConfigSet = true;

            Debug.Log("Wrote settings to file: " + configFile.ConfigFilePath);            
        }
        public void ReadSettings()
        {
            if (configFile.ConfigDefinitions.Count == 0)
            {
                WriteSettings();
            }
            configFile.Reload();

            foreach (ConfigDefinition d in configFile.ConfigDefinitions)
            {
                bool temp = false;
                foreach (FieldInfo f in GetType().GetFields())
                {
                    //Debug.Log("Observed Field with name: " + f.Name);
                    if (f.Name == d.Key)
                    {
                        // Will this work, cause spl[1] is a string? answer: no
                        ConfigWrapper<object> wrapper = new ConfigWrapper<object>(configFile, d);
                        try
                        {
                            f.SetValue(this, wrapper.Value);
                        }
                        catch (ArgumentException _)
                        {
                            try
                            {
                                f.SetValue(this, (float)Convert.ToDouble(wrapper.Value));
                            }
                            catch (FormatException __)
                            {
                                f.SetValue(this, Convert.ToBoolean(wrapper.Value));
                            }
                            catch (ArgumentException __)
                            {
                                f.SetValue(this, Convert.ToInt32(wrapper.Value));
                            }
                        }
                        Debug.Log("Set Field with name: " + d.Key + " to: " + wrapper.Value);
                        temp = true;
                        break;
                    }
                }
                if (temp)
                {
                    continue;
                }
                // This shouldn't happen, this means that something has gone wrong and that the field does not exist
                Debug.Log("No fields with name matching: " + d.Key);
            }
        }
        public bool Exists()
        {
            return configFile.ConfigDefinitions.Count > 0;
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
