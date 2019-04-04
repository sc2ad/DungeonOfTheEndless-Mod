using DustDevilFramework;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template_Mod
{
    class TemplateMod : PartialityMod
    {
        internal ScadMod mod = new ScadMod("TemplateName", typeof(TemplateSettings), typeof(TemplateMod));

        /// <summary>
        /// This is the private reference variable to your settings that has the correct type, as oppossed to mod.settings.
        /// This gets set to after mod.settings is read from.
        /// </summary>
        private TemplateSettings settings;

        public override void Init()
        {
            mod.PartialityModReference = this;
            mod.Initialize();

            // Versioning
            mod.MajorVersion = 0;
            mod.MinorVersion = 0;
            mod.Revision = 1;

            mod.settings.ReadSettings();

            settings = (mod.settings as TemplateSettings);

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
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
