using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustDevilFramework
{
    public abstract class CustomModule : ScadMod
    {
        public abstract string GetName();

        // Returns the ModuleType this class represents
        public abstract ModuleType GetModuleType();
        // Returns a 4-character digit that contains no overlap with existing IDs. Ex: 5555
        public abstract string GetID();
        // Returns the Title of the Module
        public abstract string GetTitle();
        // Returns the description of the Module
        public abstract string GetDescription();
        // Returns the number of levels the module has
        public abstract int GetLevels();

        public CustomModule(Type settingsType)
        {
            name = GetName();
            settings = (CustomModuleSettings)settingsType.TypeInitializer.Invoke(new object[] { name });
            this.settingsType = settingsType;
        }

        public void Initialize()
        {
            base.Initialize();

            // Setup default values for config
            settings.ReadSettings();

            if (settings.Enabled)
            {
                Log("Attempting to create Localization changes!");
                //CreateLocalizationChanges();
                List<string> linesLst = new List<string>();
                linesLst.Add("  <LocalizationPair Name=\"%" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "_Title\">" + GetTitle() + "</LocalizationPair>");
                for (int i = 1; i <= GetLevels(); i++)
                {
                    linesLst.Add("  <LocalizationPair Name=\"%" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "_LVL" + i + "_Title\">" + GetTitle() + " " + i + "</LocalizationPair>");
                }
                linesLst.Add("  <LocalizationPair Name=\"%" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "_Description\">" + GetDescription() + "</LocalizationPair>");
                Util.ApplyLocalizationChange("%MinorModule_Minor0020_Description", 0, linesLst.ToArray());
            } else
            {
                Log("Attempting to remove Localization changes!");
                Util.RemoveLocalizationChangeInclusive(GetModuleType() + "Module_", GetModuleType() + "Module_" + GetModuleType() + GetID() + "_Description");
                // Remove Localization Changes
            }
            Log("Initialized!");
        }

        public void Load()
        {
            base.Load();
            if (settings.Enabled)
            {

            }
        }
        public void UnLoad()
        {
            base.UnLoad();

        }
        public enum ModuleType
        {
            Minor, Major
        }
    }
}
