using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DotE_Combo_Mod
{
    class HomogenyPodUtil
    {
        public static string GetDefaultPodXml()
        {
            string s = @"<ShipConfig Name=""HomogenyPod""
    LevelCount=""12"" 
    AbscissaValue=""9"" 
    >

    <InitialBluePrints>
        <BluePrint>SpecialModule_Artifact</BluePrint>
        <BluePrint>SpecialModule_Stele</BluePrint>
        <BluePrint>SpecialModule_DustFactory</BluePrint>
        <BluePrint>SpecialModule_CryoCapsule</BluePrint>
        <BluePrint>MajorModule_Major0001_LVL1</BluePrint>
        <BluePrint>MajorModule_Major0002_LVL1</BluePrint>
        <BluePrint>MajorModule_Major0003_LVL1</BluePrint>
        <BluePrint>MinorModule_Minor0004_LVL1</BluePrint>
    </InitialBluePrints>

    <UnavailableBluePrints>
    </UnavailableBluePrints>
    
    <InitialItems>
    </InitialItems>
    
    <UnavailableItems>
        <!-- Drugs -->
        <Item>Special022</Item>
        <Item>Special023</Item>
        <Item>Special024</Item>
        <Item>Special025</Item>
        <Item>Special028</Item>
        <Item>Special029</Item>
        <Item>Special030</Item>
        <Item>Special031</Item>
        <!-- /Drugs -->
    </UnavailableItems>
    
</ShipConfig>";
            return s;
        }
        public static ShipConfig GetConfig()
        {
            ShipConfig s = new ShipConfig();

            DynData<ShipConfig> shdyn = new DynData<ShipConfig>(s);
            shdyn.Set<Amplitude.StaticString>("Name", "HomogenyPod");
            shdyn.Set<int>("AbscissaValue", 9);
            shdyn.Set<int>("LevelCount", 12);
            string[] bps = new string[]
            {
                "SpecialModule_Artifact", "SpecialModule_Stele", "SpecialModule_DustFactory", "SpecialModule_CryoCapsule",
                "MajorModule_Major0001_LVL1", "MajorModule_Major0002_LVL1", "MajorModule_Major0003_LVL1", "MinorModule_Minor0004_LVL1"
            };
            shdyn.Set<string[]>("InitBluePrints", bps);
            shdyn.Set<string[]>("UnavailableBluePrints", new string[] { });
            string[] unItems = new string[]
            {
                "Special022", "Special023", "Special024", "Special025", "Special028", "Special029", "Special030", "Special031"
            };
            shdyn.Set<string[]>("UnavailableItems", unItems);
            shdyn.Set<ShipConfig.ItemDatatableReference[]>("InitialItems", new ShipConfig.ItemDatatableReference[] { });

            return s;
        }
        // Need to add the localization so that the mod shows up with proper titlings!
        public static void CheckLocalization()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            foreach (string s in lines)
            {
                if (s.IndexOf("%ShipTitle_HomogenyPod") != -1)
                {
                    // The Localization already contains the Key for the pod.
                    return;
                }
            }
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf("%ShipTitle_Locked_Infirmary") != -1)
                {
                    linesLst.Insert(i, "  <LocalizationPair Name=\"%ShipTitle_HomogenyPod\">Homogeny Pod</LocalizationPair>");
                    linesLst.Insert(i + 1, "  <LocalizationPair Name=\"%ShipDescription_HomogenyPod\">#D0B499#Note from sales brochure: ''This pod comes with a spawn jammer. It only spawns one type of mob per floor. We hope it's occupants make it out alive.'' #REVERT#");
                    linesLst.Insert(i + 2, "");
                    linesLst.Insert(i + 3, "- Only one type of mob will spawn per floor.</LocalizationPair>");
                }
            }
            System.IO.File.WriteAllLines(@"Public\Localization\english\ED_Localization_Locales.xml", linesLst.ToArray());
        }
        // Need to remove the localization when the mod is disabled!
        public static void RemoveLocalization()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < linesLst.Count; i++)
            {
                string s = linesLst[i];
                if (s.IndexOf("%ShipTitle_HomogenyPod") != -1)
                {
                    // The Localization already contains the Key for the pod.
                    linesLst.RemoveAt(i);
                }
                if (s.IndexOf("%ShipDescription_HomogenyPod") != -1)
                {
                    // The Localization already contains the Key for the pod.
                    linesLst.RemoveAt(i);
                }
                if (s.IndexOf("- Only one type of mob will spawn per floor.</LocalizationPair>") != -1)
                {
                    // The Localization already contains the Key for the pod.
                    linesLst.RemoveAt(i);
                }
            }
            System.IO.File.WriteAllLines(@"Public\Localization\english\ED_Localization_Locales.xml", linesLst.ToArray());
        }
    }
}
