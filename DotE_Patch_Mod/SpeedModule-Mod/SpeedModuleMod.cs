using Amplitude.Unity.Simulation;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedModule_Mod
{
    public class SpeedModuleMod : PartialityMod
    {
        SpeedModuleConfig mod = new SpeedModuleConfig(typeof(SpeedModuleMod));
        public override void Init()
        {
            mod.PartialityModReference = this;
            mod.Initialize();

            mod.settings.ReadSettings();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                On.Module.Init += Module_Init;
            }
        }

        public void UnLoad()
        {
            mod.UnLoad();
            On.Module.Init -= Module_Init;
        }

        private void Module_Init(On.Module.orig_Init orig, Module self, ulong ownerPlayerID, BluePrintConfig bpConfig, Room parentRoom, ModuleSlot slot, float buildDuration, bool playAppearanceAnimations, bool instantBuild, bool restoration, bool checkOwnershipForBuildComplete)
        {
            if (bpConfig != null)
            {
                if (bpConfig.ModuleName == mod.GetModuleCategory() + "Module_" + mod.GetModuleCategory() + mod.GetID())
                {
                    // This is the speed module that is being initialized!
                    mod.Log("Creating a speed module!");
                }
                SimulationDescriptor desc = SimMonoBehaviour.GetDBDescriptorByName("SpecialModule_Stele_Pos_Speed");
                mod.Log("Speed SimDescriptor: " + desc);
                if (desc != null)
                {
                    if (desc.SimulationPropertyDescriptors != null)
                    {
                        foreach (SimulationPropertyDescriptor p in desc.SimulationPropertyDescriptors)
                        {
                            mod.Log("Name: " + p.Name + " Base: " + p.BaseValue + " Min: " + p.MinValue + " Max: " + p.MaxValue + " Composition: " + p.Composition + " Rounding: " + p.RoundingFunction);
                        }
                    }
                    if (desc.SimulationModifierDescriptors != null)
                    {
                        foreach (SimulationModifierDescriptor d in desc.SimulationModifierDescriptors)
                        {
                            mod.Log("Target Property: " + d.TargetPropertyName + " Operation: " + d.Operation);
                        }
                    }
                }
            }
            orig(self, ownerPlayerID, bpConfig, parentRoom, slot, buildDuration, playAppearanceAnimations, instantBuild, restoration, checkOwnershipForBuildComplete);
        }
    }
}
