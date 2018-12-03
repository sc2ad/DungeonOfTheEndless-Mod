using Amplitude;
using Amplitude.Unity.Simulation;
using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShinyHero_Mod
{
    class ShinyHeroConfig : CustomHero
    {
        public override string GetArchetype()
        {
            return "A Guy";
        }

        public override string GetAttackType()
        {
            return "LaserGun";
        }

        public override string GetCrystalQuote1()
        {
            return "NANI!?";
        }

        public override string GetCrystalQuote2()
        {
            return "Ez WR right here";
        }

        public override string GetDoorQuote1()
        {
            return "Oh wait, I think I'm behind now...";
        }

        public override string GetDoorQuote2()
        {
            return "I'm feeling good about this one";
        }

        public override string GetDoorQuote3()
        {
            return "Ah, I bet the exit is somewhere else";
        }

        public override string GetDoorQuote4()
        {
            return "Welp, I tried";
        }

        public override HeroConfig.HeroFaction GetFaction()
        {
            return HeroConfig.HeroFaction.Other;
        }

        public override StaticString GetIconName()
        {
            return "H0015";
        }

        public override bool IsKnown()
        {
            return true;
        }

        public override StaticString GetAnimationName()
        {
            return "H0027";
        }

        public override string GetIntro1()
        {
            return "It is I, the current WR holder";
        }

        public override string GetIntro2()
        {
            return "Welp, guess I'll die";
        }

        public override string GetIntro3()
        {
            return "I seem to be missing some competition...";
        }

        public override Dictionary<int, string[]> GetLevelUpData()
        {
            Dictionary<int, string[]> dict = new Dictionary<int, string[]>();
            dict.Add(1, new string[] { "0", "Skill_P0001" });
            dict.Add(2, new string[] { "15", "Skill_P0002" });
            dict.Add(3, new string[] { "25", "Skill_P0003" });
            dict.Add(4, new string[] { "30", "Skill_P0004" });
            dict.Add(5, new string[] { "35" });
            dict.Add(6, new string[] { "40", "Skill_P0005" });
            dict.Add(7, new string[] { "45", "Skill_P0006" });
            dict.Add(8, new string[] { "50" });
            dict.Add(9, new string[] { "40", "Skill_P0007" });
            dict.Add(10, new string[] { "30", "Skill_P0008", "Skill_P0009" });
            dict.Add(11, new string[] { "25" });
            dict.Add(12, new string[] { "20", "Skill_P0010" });
            dict.Add(13, new string[] { "20", "Skill_P0011" });
            dict.Add(14, new string[] { "20", "Skill_P0012" });
            dict.Add(15, new string[] { "5", "Skill_P0013" });
            dict.Add(16, new string[] { "0" });
            return dict;
        }

        public override SimulationDescriptor[] GetLevelupSimulationDescriptors()
        {
            List<SimulationDescriptor> lst = new List<SimulationDescriptor>();
            // Level 1
            for (int i = 1; i <= 15; i++)
            {
                SimDescriptorWrapper wrapper = new SimDescriptorWrapper();
                wrapper.Add(SimulationProperties.MaxHealth, 100);
                wrapper.Add(SimulationProperties.MoveSpeed, 5);
                lst.Add(wrapper.GetDescriptor("Hero_" + GetName() + "_LVL" + i, SimulationProperties.SimDescTypeHero));
            }
            lst.Add(new SimDescriptorWrapper().GetDescriptor("Hero_" + GetName() + "_LVL" + 16, SimulationProperties.SimDescTypeHero));
            return lst.ToArray();
        }

        public override StaticString GetName()
        {
            return "H5555";
        }

        public override string GetRealDescription()
        {
            return "An obscure WR holder, he really does suck at this game.";
        }

        public override string GetRealFirstName()
        {
            return "WR";
        }

        public override StaticString GetRealName()
        {
            return "WR Holder";
        }

        public override float GetRecruitCost()
        {
            return 10;
        }

        public override string GetRepairQuote1()
        {
            return "But I don't even know how to repair...";
        }

        public override string GetRepairQuote2()
        {
            return "EECS Major coming through! I got this.";
        }

        public override EquipmentSlotConfig GetSecondSlot()
        {
            EquipmentSlotConfig config = new EquipmentSlotConfig();
            config.XmlSerializableCategoryName = "ItemHero_Armor";
            return config;
        }

        public override SimulationDescriptor GetSimulationDescriptor()
        {
            SimDescriptorWrapper wrapper = new SimDescriptorWrapper();
            //wrapper.Add(SimulationProperties.MaxHealth, 510, null);
            //wrapper.Add(SimulationProperties.MoveSpeed, 44, null);
            //wrapper.Add(SimulationProperties.Wit, 15, null);
            //wrapper.Add(SimulationProperties.AttackCooldown, 0.2f, null);
            //wrapper.Add(SimulationProperties.AttackPower, 45, null);
            //wrapper.Add(SimulationProperties.AttackRange, 5, null);
            //wrapper.Add(SimulationProperties.Defense, 10, null);
            //wrapper.Add(SimulationProperties.HealthRegen, 5, null);
            wrapper.Add("MaxHealth", 510);
            wrapper.Add("MoveSpeed", 44);
            wrapper.Add("Wit", 15);
            wrapper.Add("AttackCooldown", 0.5f);
            wrapper.Add("AttackPower", 45);
            wrapper.Add("AttackRange", 5);
            wrapper.Add("Defense", 10);
            wrapper.Add("HealthRegen", 5);
            wrapper.Add("AttackHitCount", 1);
            return wrapper.GetDescriptor("Hero_" + GetName(), SimulationProperties.SimDescTypeHero);
            //return wrapper.GetDescriptor(GetName());
        }

        public override AITargetType GetTargetType()
        {
            return AITargetType.LongRangeHero;
        }

        public override EquipmentSlotConfig GetThirdSlot()
        {
            EquipmentSlotConfig config = new EquipmentSlotConfig();
            config.XmlSerializableCategoryName = "ItemHero_Special";
            return config;
        }

        public override int GetUnlockLevelCount()
        {
            return 3;
        }

        public override EquipmentSlotConfig GetWeaponSlot()
        {
            EquipmentSlotConfig config = new EquipmentSlotConfig();
            config.XmlSerializableCategoryName = "ItemHero_Weapon";
            config.XmlSerializableTypeName = "Weapon_Gun";
            return config;
        }

        public override string GetWoundedQuote1()
        {
            return "At this rate I won't get WR";
        }

        public override string GetWoundedQuote2()
        {
            return "Do you mind?";
        }
    }
}
