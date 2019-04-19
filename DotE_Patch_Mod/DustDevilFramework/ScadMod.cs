using BepInEx.Configuration;
using BepInEx.Logging;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DustDevilFramework
{
    public class ScadMod
    {
        public string name = "placeholder";

        public Type BepinExPluginType;
        public BepInEx.BaseUnityPlugin BepinPluginReference;

        private ManualLogSource logger;

        private BepInEx.BepInPlugin pluginData;

        public Version Version { get; private set; }
        public string GUID { get; private set; }
        public ConfigWrapper<bool> EnabledWrapper { get; private set; }

        public ScadMod(string name, Type bepinModType, BepInEx.BaseUnityPlugin bepinPlugin)
        {
            this.name = name;
            BepinExPluginType = bepinModType;
            BepinPluginReference = bepinPlugin;
            SetupPluginData();
        }
        // ONLY USE THIS IF YOU ARE ATTEMPTING TO ADD DATA AFTER CONSTRUCTION (FOR THE CREATION OF ABSTRACT CLASSES ONLY!)
        public ScadMod()
        {
        }
        protected void SetupPluginData()
        {
            object[] customAttrs = BepinExPluginType.GetCustomAttributes(typeof(BepInEx.BepInPlugin), false);
            if (customAttrs.Length > 0 && customAttrs[0] != null)
            {
                pluginData = customAttrs[0] as BepInEx.BepInPlugin;
                Log("Overwritting name from: " + name + " to: " + pluginData.Name);
                name = pluginData.Name;
            }
            else
            {
                Log(LogLevel.Error, "Could not find valid BepInPlugin attribute on mod: " + name);
                Log(LogLevel.Warning, "Attempting to use placeholder pluginData!");
                pluginData = new BepInEx.BepInPlugin("com.sc2ad.placeholder", "placeholderName", "0.0.0");
            }
            Version = pluginData.Version;
            GUID = pluginData.GUID;
            EnabledWrapper = Util.GetConfigFile(this).Wrap("Settings", "Enabled", "Whether the mod is enabled or not", true);
            Util.GetConfigFile(this).Save();
        }
        public void Log(LogLevel level, object s)
        {
            if (logger == null)
            {
                try
                {
                    logger = new DynData<BepInEx.BaseUnityPlugin>(BepinPluginReference).Get<ManualLogSource>("Logger");
                } catch (Exception _)
                {
                    // Could not find the Logger for the plugin! Make our own!
                    logger = Logger.CreateLogSource(@"BepInEx\log\" + name + ".log");
                }
            }
            logger.Log(level, s);
        }
        public void Log(object s)
        {
            Log(LogLevel.Debug, s);
        }
        public void Initialize()
        {
            Log("===================================================================");
            Log("DATETIME: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString());
            Log("===================================================================");

            Util.GetConfigFile(this).Reload();
            DustDevil.CreateInstance(this);
        }
        public void Load()
        {
            Log("Loaded!");

            // Create and load hooks here!
        }

        public void UnLoad()
        {
            Log("Unloaded!");

            // Unload all of the loaded hooks here!
        }
    }
}
