using System;
using System.Collections.Generic;
using System.Text;

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
        // Returns the model of the module (should already exist)

        // Module Config
        // Returns the base cost (industry) of the module
        public abstract int GetInitialCost();
        // Returns the increase in cost for each existing such module on the current floor
        public abstract int GetCostIncrement();
        // Returns the number of seconds the module takes to build, when there are waves
        public abstract int GetBuildDuration();
        // Returns the attack type for attacking modules.
        // Currently there are only certain attack types: Laser, Tesla, Bomb, ModuleGun, Sebastroll, ScienceLaser, TroubleMaker, Posion
        // If the modules does not attack, return an empty string.
        public abstract string GetAttackType();
        // Returns whether the module attracts a merchant or not
        public abstract bool GetAttractsMerchant();
        // Returns whether the module powers a room or not
        public abstract bool GetPowersRoom();
        // Returns whether the module needs room power or not (default should be true)
        public abstract bool GetNeedRoomPower();
        // Returns whether the module rotates in order to attack
        public abstract bool GetRotate();
        // Returns whether the modules is removable or not
        public abstract bool GetUnRemovable();

        // Blueprint Config
        // Returns the module category the module is
        public abstract ModuleCategory GetModuleCategory();
        // Returns an array with length = GetLevels() that holds the research costs for each level
        public abstract int[] GetResearchCosts();

        public CustomModule(Type settingsType)
        {
            name = GetName();
            settings = Activator.CreateInstance(settingsType, new object[] { name }) as CustomModuleSettings;
            this.settingsType = settingsType;
        }

        public new void Initialize()
        {
            base.Initialize();

            // Setup default values for config
            settings.ReadSettings();

            if (settings.Enabled)
            {
                Log("Attempting to create Localization changes!");
                List<string> linesLst = new List<string>();
                linesLst.Add("  <LocalizationPair Name=\"%" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "_Title\">" + GetTitle() + "</LocalizationPair>");
                for (int i = 1; i <= GetLevels(); i++)
                {
                    linesLst.Add("  <LocalizationPair Name=\"%" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "_LVL" + i + "_Title\">" + GetTitle() + " " + i + "</LocalizationPair>");
                }
                linesLst.Add("  <LocalizationPair Name=\"%" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "_Description\">" + GetDescription() + "</LocalizationPair>");
                Util.ApplyLocalizationChange("%MinorModule_Minor0020_Description", 0, linesLst);
                // ModuleConfig
                linesLst = new List<string>();
                linesLst.Add("  <ModuleConfig Name=\"" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "\" AITargetType=\"" + GetModuleType() + "\" SimClass=\"" + GetModuleType() + "\" BuildBaseIndustryCost=\""
                    + GetInitialCost() + "\" BuildIndustryCostIncrement=\"" + GetCostIncrement() + "\" BuildDuration=\"" + GetBuildDuration() + (GetAttackType().Length != 0 ? "\" AttackType=\"" + GetAttackType() + "\"" : "\"")
                    + (GetAttractsMerchant() ? " AttractMerchant=\"1\"" : "") + (GetPowersRoom() ? " PowersRoom=\"true\"" : "") + (GetNeedRoomPower() ? "" : " NeedRoomPower=\"0\"") + (GetRotate() ? " Rotate=\"1\"" : "")
                    + (GetUnRemovable() ? " Unremovable=\"1\"" : "") + "/>");
                Util.ApplyFileChange(@"Public\Configuration\ModuleConfigs.xml", "Minor0017", 1, linesLst);
                // BluePrintConfig
                linesLst = new List<string>();
                for (int i = 1; i <= GetLevels(); i++) {
                    linesLst.Add("  <BluePrintConfig Name=\"" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "_LVL" + i + "\" ModuleCategory=\"" + GetModuleCategory() + "\" ModuleName=\""
                        + GetModuleType() + "Module_" + GetModuleType() + GetID() + "\" ModuleLevel=\"" + i + "\" ResearchScienceCost=\"" + GetResearchCosts()[i - 1] + "\"/>");
                }
                Util.ApplyFileChange(@"Public\Configuration\BluePrintConfigs.xml", "Minor0017", 1, linesLst);
            } else
            {
                Log("Attempting to remove Localization changes!");
                Util.RemoveLocalizationChangeInclusive(GetModuleType() + "Module_", GetModuleType() + "Module_" + GetModuleType() + GetID() + "_Description");
                Util.RemoveFileChangeInclusive(@"Public\Configuration\ModuleConfigs.xml", GetID(), GetID());
                Util.RemoveFileChangeInclusive(@"Public\Configuration\BluePrintConfigs.xml", GetID(), GetID());
                // Remove Localization Changes
            }
            Log("Initialized!");
        }

        public new void Load()
        {
            base.Load();
            if (settings.Enabled)
            {

            }
        }
        public new void UnLoad()
        {
            base.UnLoad();

        }
        public enum ModuleType
        {
            Minor, Major
        }
    }
}
