using Amplitude.Unity.Framework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DustDevilFramework
{
    public abstract class CustomModule : ScadMod
    {
        public abstract string GetName();
        /// <summary>
        /// Returns the ModuleType this class represents
        /// </summary>
        /// <returns></returns>
        public abstract ModuleType GetModuleType();
        /// <summary>
        /// Returns a 4-character digit that contains no overlap with existing IDs. Ex: 5555
        /// </summary>
        /// <returns></returns>
        public abstract string GetID();
        /// <summary>
        /// Returns the Title of the Module
        /// </summary>
        /// <returns></returns>
        public abstract string GetTitle();
        /// <summary>
        /// Returns the description of the Module
        /// </summary>
        /// <returns></returns>
        public abstract string GetDescription();
        /// <summary>
        /// Returns the number of levels the module has
        /// </summary>
        /// <returns></returns>
        public abstract int GetLevels();

        // Module Config
        /// <summary>
        /// Returns the base cost (industry) of the module
        /// </summary>
        /// <returns></returns>
        public abstract float GetInitialCost();
        /// <summary>
        /// Returns the increase in cost for each existing such module on the current floor
        /// </summary>
        /// <returns></returns>
        public abstract float GetCostIncrement();
        /// <summary>
        /// Returns the number of seconds the module takes to build, when there are waves
        /// </summary>
        /// <returns></returns>
        public abstract float GetBuildDuration();
        /// <summary>
        /// Returns the attack type for attacking modules.
        /// Currently there are only certain attack types: 
        /// Laser, Tesla, Bomb, ModuleGun, Sebastroll, ScienceLaser, TroubleMaker, Posion
        /// If the modules does not attack, return an empty string.
        /// </summary>
        /// <returns></returns>
        public abstract string GetAttackType();
        /// <summary>
        /// Returns whether the module attracts a merchant or not
        /// </summary>
        /// <returns></returns>
        public abstract bool GetAttractsMerchant();
        /// <summary>
        /// Returns whether the module powers a room or not
        /// </summary>
        /// <returns></returns>
        public abstract bool GetPowersRoom();
        /// <summary>
        /// Returns whether the module needs room power or not (default should be true)
        /// </summary>
        /// <returns></returns>
        public abstract bool GetNeedRoomPower();
        /// <summary>
        /// Returns whether the module rotates in order to attack
        /// </summary>
        /// <returns></returns>
        public abstract bool GetRotate();
        /// <summary>
        /// Returns whether the modules is removable or not
        /// </summary>
        /// <returns></returns>
        public abstract bool GetUnremovable();

        // Blueprint Config
        /// <summary>
        /// Returns the module category the module is
        /// </summary>
        /// <returns></returns>
        public abstract ModuleCategory GetModuleCategory();
        /// <summary>
        /// Returns an array with Length = GetLevels() that holds the research costs for each level
        /// </summary>
        /// <returns></returns>
        public abstract float[] GetResearchCosts();
        /// <summary>
        /// Returns the SimulationDescriptorWrapper for any effects that are desired
        /// (default should be null)
        /// </summary>
        public abstract SimDescriptorWrapper GetSimulationDescriptor();
        /// <summary>
        /// Returns the Module to use for animations.
        /// MUST BE IN THE FORMAT: MinorModule_Minor0017
        /// </summary>
        /// <returns></returns>
        public abstract string GetAssumedModuleName();

        public CustomModule(string name, Type settingsType, Type partialityType) : base(name, settingsType, partialityType)
        {
            settings = Activator.CreateInstance(settingsType, new object[] { name }) as CustomModuleSettings;
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
                //// ModuleConfig
                //linesLst = new List<string>();
                //linesLst.Add("  <ModuleConfig Name=\"" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "\" AITargetType=\"" + GetModuleType() + "Module\" SimClass=\"" + GetModuleType() + "Module\" BuildBaseIndustryCost=\""
                //    + GetInitialCost() + "\" BuildIndustryCostIncrement=\"" + GetCostIncrement() + "\" BuildDuration=\"" + GetBuildDuration() + (GetAttackType().Length != 0 ? "\" AttackType=\"" + GetAttackType() + "\"" : "\"")
                //    + (GetAttractsMerchant() ? " AttractMerchant=\"1\"" : "") + (GetPowersRoom() ? " PowersRoom=\"true\"" : "") + (GetNeedRoomPower() ? "" : " NeedRoomPower=\"0\"") + (GetRotate() ? " Rotate=\"1\"" : "")
                //    + (GetUnremovable() ? " Unremovable=\"1\"" : "") + "/>");
                //Util.ApplyFileChange(@"Public\Configuration\ModuleConfigs.xml", "Minor0017", 1, linesLst);
                //// BluePrintConfig
                //linesLst = new List<string>();
                //for (int i = 1; i <= GetLevels(); i++) {
                //    linesLst.Add("  <BluePrintConfig Name=\"" + GetModuleType() + "Module_" + GetModuleType() + GetID() + "_LVL" + i + "\" ModuleCategory=\"" + GetModuleCategory() + "\" ModuleName=\""
                //        + GetModuleType() + "Module_" + GetModuleType() + GetID() + "\" ModuleLevel=\"" + i + "\" ResearchScienceCost=\"" + GetResearchCosts()[i - 1] + "\"/>");
                //}
                //Util.ApplyFileChange(@"Public\Configuration\BluePrintConfigs.xml", "Minor0017", 1, linesLst);
            } else
            {
                Log("Attempting to remove Localization changes!");
                Util.RemoveLocalizationChangeInclusive(GetModuleType() + "Module_", GetModuleType() + "Module_" + GetModuleType() + GetID() + "_Description");
                //Util.RemoveFileChangeInclusive(@"Public\Configuration\ModuleConfigs.xml", GetID(), GetID());
                //Util.RemoveFileChangeInclusive(@"Public\Configuration\BluePrintConfigs.xml", GetID(), GetID());
                // Remove Localization Changes
            }
            Log("Initialized!");
        }

        public new void Load()
        {
            base.Load();
            if (settings.Enabled)
            {
                On.Module.Init += Module_Init;
                On.Module.InitAnims += Module_InitAnims;
                On.MainGameScreen.Awake += MainGameScreen_Awake;
            }
        }

        private void AddBPConfigsToDatabase()
        {
            Log("Adding " + GetLevels() + " BluePrintConfigs to Database!");
            for (int level = 1; level <= GetLevels(); level++) {
                BluePrintConfig bpConfig = new BluePrintConfig();
                DynData<BluePrintConfig> d = new DynData<BluePrintConfig>(bpConfig);
                bpConfig.XmlSerializableName = GetModuleType() + "Module_" + GetModuleType() + GetID();
                d.Set("ModuleName", GetModuleType() + "Module_" + GetModuleType() + GetID());
                d.Set("ModuleCategory", GetModuleCategory());
                d.Set("ModuleLevel", level);
                d.Set("ResearchScienceCost", GetResearchCosts()[level - 1]);
                Databases.GetDatabase<BluePrintConfig>(false).Add(bpConfig);
            }
        }

        private void AddModuleConfigToDatabase()
        {
            Log("Adding ModuleConfig to Database!");
            ModuleConfig config = new ModuleConfig();
            DynData<ModuleConfig> d = new DynData<ModuleConfig>(config);
            config.XmlSerializableName = GetModuleType() + "Module_" + GetModuleType() + GetID();
            config.XmlSerializableAITargetType = GetModuleType() + "Module";
            d.Set("AttackType", GetAttackType());
            d.Set("SimClass", GetModuleType() + "Module");
            d.Set("AttractMerchant", GetAttractsMerchant());
            d.Set("BuildBaseIndustryCost", GetInitialCost());
            d.Set("BuildDuration", GetBuildDuration());
            d.Set("BuildIndustryCostIncrement", GetCostIncrement());
            d.Set("NeedRoomPower", GetNeedRoomPower());
            d.Set("PowersRoom", GetPowersRoom());
            d.Set("Rotate", GetRotate());
            d.Set("Unremovable", GetUnremovable());
            Databases.GetDatabase<ModuleConfig>(false).Add(config);
        }

        private void MainGameScreen_Awake(On.MainGameScreen.orig_Awake orig, MainGameScreen self)
        {
            // Placeholder to simply add all the info to the Database.
            if (!Databases.GetDatabase<BluePrintConfig>(false).ContainsKey(GetModuleType() + "Module_" + GetModuleType() + GetID() + "_LVL1"))
            {
                AddBPConfigsToDatabase();
            }
            if (!Databases.GetDatabase<ModuleConfig>(false).ContainsKey(GetModuleType() + "Module_" + GetModuleType() + GetID()))
            {
                AddModuleConfigToDatabase();
            }
            orig(self);
        }

        private void Module_InitAnims(On.Module.orig_InitAnims orig, Module self)
        {
            if (self.GetConfigName() == GetModuleType() + "Module_" + GetModuleType() + GetID())
            {
                // This is our module!
                DynData<Module> d = new DynData<Module>(self);
                SpriteAnimationRuntime2 baseAnim = d.Get<SpriteAnimationRuntime2>("baseSpriteAnim");
                SpriteAnimationRuntime2 faceSprite = d.Get<SpriteAnimationRuntime2>("faceSpriteAnim");
                SpriteAnimationRuntime2 iconSpriteAnim = d.Get<SpriteAnimationRuntime2>("iconSpriteAnim");
                baseAnim.OverrideClipsFromPath("SpriteAnimations/Modules/Default_" + GetModuleCategory() + "/LVL1", false);
                baseAnim.OverrideClipsFromPath("SpriteAnimations/Modules/" + GetAssumedModuleName() + "/LVL1", false);
                faceSprite.OverrideClipsFromPath("SpriteAnimations/Modules/Default_" + GetModuleCategory() + "/LVL1", false);
                faceSprite.OverrideClipsFromPath("SpriteAnimations/Modules/" + GetAssumedModuleName() + "/LVL1", false);
                if (iconSpriteAnim != null)
                {
                    iconSpriteAnim.Stop(true);
                }
                return;
            }
            orig(self);
        }

        public new void UnLoad()
        {
            base.UnLoad();
            On.Module.Init -= Module_Init;
            On.Module.InitAnims -= Module_InitAnims;
        }

        private void Module_Init(On.Module.orig_Init orig, Module self, ulong ownerPlayerID, BluePrintConfig bpConfig, Room parentRoom, ModuleSlot slot, float buildDuration, bool playAppearanceAnimations, bool instantBuild, bool restoration, bool checkOwnershipForBuildComplete)
        {
            orig(self, ownerPlayerID, bpConfig, parentRoom, slot, buildDuration, playAppearanceAnimations, instantBuild, restoration, checkOwnershipForBuildComplete);
            // Add the descriptor to the db, if it does anything, and then create it as normal.
            if (GetSimulationDescriptor() == null)
            {
                return;
            }
            if (SimMonoBehaviour.GetDBDescriptorByName(GetModuleType() + "Module_" + GetModuleType() + GetID()) == null)
            {
                SimMonoBehaviour.GetSimDescDatabase().Add(GetSimulationDescriptor().GetDescriptor(GetModuleType() + "Module_" + GetModuleType() + GetID()));
            }
            self.AddSimDescriptor(SimMonoBehaviour.GetDBDescriptorByName(GetModuleType() + "Module_" + GetModuleType() + GetID()));
        }

        public enum ModuleType
        {
            Minor, Major
        }
    }
}
