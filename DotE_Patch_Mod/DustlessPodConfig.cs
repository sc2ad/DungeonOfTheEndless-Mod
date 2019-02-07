using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustlessPod_Mod
{
    class DustlessPodConfig : PodMod
    {
        public DustlessPodConfig(Type partialityType) : base(typeof(DustlessPodSettings), partialityType)
        {
        }

        public override string GetAnimationPod()
        {
            return "Refreezerator";
        }

        public override string GetDescription()
        {
            return "#D0B499#Note from sales brochure: ''We forgot to put dust in this pod, but at least mobs still drop some!'' #REVERT#";
        }

        public override string GetDungeonPod()
        {
            return "Pod";
        }

        public override int GetIndex()
        {
            return 10;
        }

        public override string[] GetInitialBlueprints()
        {
            return new string[]
            {
                "SpecialModule_Artifact", "SpecialModule_Stele", "SpecialModule_DustFactory", "SpecialModule_CryoCapsule",
                "MajorModule_Major0001_LVL1", "MajorModule_Major0002_LVL1", "MajorModule_Major0003_LVL1", "MinorModule_Minor0004_LVL1"
            };
        }

        public override int GetLevelCount()
        {
            return 12;
        }

        public override string GetName()
        {
            return "DustlessPod";
        }

        public override string GetSpecialText()
        {
            return "\n- Rooms provide no dust.\n- Monsters will drop slightly more dust.";
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
