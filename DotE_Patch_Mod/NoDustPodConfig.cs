using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoDustPod_Mod
{
    class NoDustPodConfig : PodMod
    {
        public NoDustPodConfig(Type partialityType) : base(typeof(NoDustPodSettings), partialityType)
        {
        }

        public override string GetAnimationPod()
        {
            return "Infirmary";
        }

        public override string GetDescription()
        {
            return "#D0B499#Note from sales brochure: ''We... uh... may have forgot to put dust in this one. Good luck!'' #REVERT#";
        }

        public override string GetDungeonPod()
        {
            return "Pod";
        }

        public override string GetName()
        {
            return "NoDustPod";
        }

        public override string GetSpecialText()
        {
            return "\n- Maximum of 9 dust per floor.";
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
            return 11;
        }

        public override int GetLevelCount()
        {
            return 3;
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
