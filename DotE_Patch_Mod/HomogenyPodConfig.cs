using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotE_Combo_Mod
{
    class HomogenyPodConfig : PodMod
    {
        public override string GetAnimationPod()
        {
            return "Armory";
        }

        public override string GetDescription()
        {
            return "#D0B499#Note from sales brochure: ''This pod comes with a spawn jammer. It only spawns one type of mob per floor. We hope it's occupants make it out alive.'' #REVERT#";
        }

        public override string GetDungeonPod()
        {
            return "Pod";
        }

        public override string GetName()
        {
            return "HomogenyPod";
        }

        public override string GetSpecialText()
        {
            return "\n- Only one type of mob will spawn per floor.";
        }

        public override string[] GetInitialBlueprints()
        {
            return new string[]
            {
                "SpecialModule_Artifact", "SpecialModule_Stele", "SpecialModule_DustFactory", "SpecialModule_CryoCapsule",
                "MajorModule_Major0001_LVL1", "MajorModule_Major0002_LVL1", "MajorModule_Major0003_LVL1", "MinorModule_Minor0004_LVL1"
            };
        }

        public override int GetIndex()
        {
            return 9;
        }

        public override int GetLevelCount()
        {
            return 12;
        }

        public override string[] GetUnavailableBlueprints()
        {
            return new string[0];
        }

        public override string[] GetUnavailableItems()
        {
            return new string[]
            {
                "Special022", "Special023", "Special024", "Special025", "Special028", "Special029", "Special030", "Special031"
            };
        }
    }
}
