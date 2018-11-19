using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amplitude;
using Amplitude.Unity.Simulation;
using DustDevilFramework;

namespace OPSashaItem_Mod
{
    class ShinyItemConfig : CustomItem
    {
        public override string GetAttackType()
        {
            return "FireGun";
        }

        public override bool GetCannotBeUnequipped()
        {
            return false;
        }

        public override string GetCategoryName()
        {
            return "ItemHero_Weapon";
        }

        public override string GetCategoryTypeName()
        {
            return "Weapon_Gun";
        }

        public override Dictionary<string, int> GetCommonRarity()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict.Add("Start", 0);
            dict.Add("LevelMin", 1);
            dict.Add("LevelMax", 999);
            dict.Add("StartValue", 50);
            dict.Add("DepthBonus", 0);
            dict.Add("MaxValue", -1);
            return dict;
        }

        public override bool GetDestroyOnDeath()
        {
            return false;
        }

        public override bool GetFlatSprite()
        {
            return true;
        }

        public override int GetMaxCount()
        {
            return -1;
        }

        public override int GetMaxLevel()
        {
            return 999;
        }

        public override int GetMaxOccurenceCount()
        {
            return 100;
        }

        public override int GetMinLevel()
        {
            return 1;
        }

        public override string GetName()
        {
            return "Weapon555";
        }

        public override Dictionary<string, int> GetRarity0Rarity()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict.Add("Start", 0);
            dict.Add("LevelMin", 1);
            dict.Add("LevelMax", 999);
            dict.Add("StartValue", 40);
            dict.Add("DepthBonus", 0);
            dict.Add("MaxValue", -1);
            return dict;
        }

        public override Dictionary<string, int> GetRarity1Rarity()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict.Add("Start", 0);
            dict.Add("LevelMin", 1);
            dict.Add("LevelMax", 999);
            dict.Add("StartValue", 30);
            dict.Add("DepthBonus", 0);
            dict.Add("MaxValue", -1);
            return dict;
        }

        public override Dictionary<string, int> GetRarity2Rarity()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict.Add("Start", 0);
            dict.Add("LevelMin", 1);
            dict.Add("LevelMax", 999);
            dict.Add("StartValue", 20);
            dict.Add("DepthBonus", 0);
            dict.Add("MaxValue", -1);
            return dict;
        }

        public override string GetRealDescription()
        {
            return "The OMEGA GAY GUN";
        }

        public override string GetRealName()
        {
            return "Actual Autism";
        }

        public override string[] GetSkills()
        {
            return new string[] { "Skill_A0033", "Skill_P0029" };
        }

        public override StaticString GetSpriteName()
        {
            return "Weapon001";
        }

        public override float GetStartingProbabilityWeight()
        {
            return 10000;
        }

        public override SimulationDescriptor GetBaseDescriptor()
        {
            SimDescriptorWrapper wrapper = new SimDescriptorWrapper();
            wrapper.Add(SimulationProperties.AttackCooldown, -0.5f);
            wrapper.Add(SimulationProperties.MoveSpeed, 3f);
            return wrapper.GetDescriptor(GetName());
        }

        public override SimulationDescriptor GetCommonDescriptor()
        {
            SimDescriptorWrapper wrapper = new SimDescriptorWrapper();
            wrapper.Add(SimulationProperties.AttackCooldown, -0.5f);
            wrapper.Add(SimulationProperties.MoveSpeed, 3f);
            return wrapper.GetDescriptor(GetName() + "_Common");
        }

        public override SimulationDescriptor GetRarity0Descriptor()
        {
            SimDescriptorWrapper wrapper = new SimDescriptorWrapper();
            wrapper.Add(SimulationProperties.AttackCooldown, -1.0f);
            wrapper.Add(SimulationProperties.MoveSpeed, 8f);
            wrapper.Add(SimulationProperties.MaxHealth, 10000f);
            wrapper.Add(SimulationProperties.AttackPower, 100);
            wrapper.Add(SimulationProperties.HealthRegen, 800);
            return wrapper.GetDescriptor(GetName() + "_Rarity0");
        }

        public override SimulationDescriptor GetRarity1Descriptor()
        {
            SimDescriptorWrapper wrapper = new SimDescriptorWrapper();
            wrapper.Add(SimulationProperties.AttackCooldown, -1.1f);
            wrapper.Add(SimulationProperties.MoveSpeed, 18f);
            wrapper.Add(SimulationProperties.MaxHealth, 10000f);
            wrapper.Add(SimulationProperties.AttackPower, 100);
            wrapper.Add(SimulationProperties.HealthRegen, 800);
            return wrapper.GetDescriptor(GetName() + "_Rarity1");
        }

        public override SimulationDescriptor GetRarity2Descriptor()
        {
            SimDescriptorWrapper wrapper = new SimDescriptorWrapper();
            wrapper.Add(SimulationProperties.AttackCooldown, -1.19999f);
            wrapper.Add(SimulationProperties.MoveSpeed, 90f);
            wrapper.Add(SimulationProperties.MaxHealth, 10000f);
            wrapper.Add(SimulationProperties.AttackPower, 100);
            wrapper.Add(SimulationProperties.HealthRegen, 800);
            return wrapper.GetDescriptor(GetName() + "_Rarity2");
        }
    }
}
