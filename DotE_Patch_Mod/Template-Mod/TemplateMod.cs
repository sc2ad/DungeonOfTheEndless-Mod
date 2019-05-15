using BepInEx;
using BepInEx.Configuration;
using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Template_Mod
{
    [BepInPlugin("com.sc2ad.TemplatePlugin", "Template Name", "1.0.0")]
    public class TemplateMod : BaseUnityPlugin
    {
        private ScadMod mod;

        private ConfigWrapper<bool> someBoolWrapper;
        private ConfigWrapper<int> someIntWrapper;

        public void Awake()
        {
            mod = new ScadMod("TemplateName", typeof(TemplateMod), this);

            // Wrap Settings here!
            someBoolWrapper = Config.Wrap("Settings", "SomeBoolean", "Some boolean used by the template mod.", true);
            someIntWrapper = Config.Wrap("Settings", "SomeInt", "Some random integer used by the template mod.", 0);
            // Provides maximums, minimums, and increments for SomeInt.
            // If these are not provided, the default min is 0, max is 100, increment is 1
            Config.Wrap("SettingsIgnore", "SomeIntMin", defaultValue: 0);
            Config.Wrap("SettingsIgnore", "SomeIntMax", defaultValue: 5);
            Config.Wrap("SettingsIgnore", "SomeIntIncrement", defaultValue: 2);

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
            {
                // Add hooks here!
            }
        }
        public void UnLoad()
        {
            mod.UnLoad();
            // Remove hooks here!
        }
    }
}
