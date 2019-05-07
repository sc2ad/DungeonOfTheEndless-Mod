﻿using BepInEx;
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

        private ConfigWrapper<bool> regenerateConfigWrapper;

        public void Awake()
        {
            mod = new ScadMod("TemplateName", typeof(TemplateMod), this);

            // Wrap Settings here!
            regenerateConfigWrapper = Config.Wrap("Settings", "RegenerateConfig", "Regenerates the Config file to use with this plugin.", false);

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (regenerateConfigWrapper.Value)
            {
                // Regenerate the Config file!
                // Then save it to the disk again! (Not necessary)
                Config.Save();
            }
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
