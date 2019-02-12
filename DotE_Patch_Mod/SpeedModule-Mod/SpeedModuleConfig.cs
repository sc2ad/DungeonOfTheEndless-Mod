using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedModule_Mod
{
    public class SpeedModuleConfig : CustomModule
    {
        public SpeedModuleConfig(Type partialityType) : base("SpeedModule", typeof(SpeedModuleSettings), partialityType)
        {
        }

        public override string GetAssumedModuleName()
        {
            return "MajorModule_Major0004";
        }

        public override string GetAttackType()
        {
            return "";
        }

        public override bool GetAttractsMerchant()
        {
            return false;
        }

        public override float GetBuildDuration()
        {
            return 8;
        }

        public override float GetCostIncrement()
        {
            return 5;
        }

        public override string GetDescription()
        {
            return "This module provides speed to all heroes on the floor. As its level increases, so too does its speed bonus.";
        }

        public override string GetID()
        {
            return "5555";
        }

        public override float GetInitialCost()
        {
            return 40;
        }

        public override int GetLevels()
        {
            return 1;
        }

        public override ModuleCategory GetModuleCategory()
        {
            return ModuleCategory.MajorModule;
        }

        public override ModuleType GetModuleType()
        {
            return ModuleType.Major;
        }

        public override string GetName()
        {
            return "SpeedModule";
        }

        public override bool GetNeedRoomPower()
        {
            return true;
        }

        public override bool GetPowersRoom()
        {
            return false;
        }

        public override float[] GetResearchCosts()
        {
            return new float[]
            {
                40
            };
        }

        public override bool GetRotate()
        {
            return false;
        }

        public override SimDescriptorWrapper GetSimulationDescriptor()
        {
            SimDescriptorWrapper wrapper = new SimDescriptorWrapper();
            wrapper.Add(SimulationProperties.MoveSpeed, (settings as SpeedModuleSettings).SpeedIncrease);
            return wrapper;
            //return null;
        }

        public override string GetTitle()
        {
            return "Speed Module";
        }

        public override bool GetUnremovable()
        {
            return false;
        }
    }
}
