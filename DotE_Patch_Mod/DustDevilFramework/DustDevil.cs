using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DustDevilFramework
{
    public class DustDevil
    {
        private static DustDevil Instance;

        public static int MajorVersion { get; } = 3;
        public static int MinorVersion { get; } = 0;
        public static int Revision { get; } = 0;

        private static List<ScadMod> ModList = new List<ScadMod>();

        public static void CreateInstance(ScadMod mod)
        {
            if (Instance == null)
            {
                Instance = new DustDevil();
            }
            ModList.Add(mod);
        }
        private DustDevil()
        {
            On.MainMenuPanel.OnLoad += MainMenuPanel_OnLoad;
        }

        private System.Collections.IEnumerator MainMenuPanel_OnLoad(On.MainMenuPanel.orig_OnLoad orig, MainMenuPanel self)
        {
            System.Collections.IEnumerator temp = orig(self);
            yield return temp;
            AgePrimitiveLabel label = new DynData<MainMenuPanel>(self).Get<AgePrimitiveLabel>("versionLabel");
            Debug.Log("Attempting to overwrite old version with DustDevil version!");
            Debug.Log("Current message is: " + label.Text);
            label.Text = "#4D4D4D#Dungeon of the Endless, VERSION ";
            label.Text += string.Concat(new object[]
            {
                "#REVERT##4D4D4D#",
                Amplitude.Unity.Framework.Application.Version.Major,
                ".",
                Amplitude.Unity.Framework.Application.Version.Minor,
                ".",
                Amplitude.Unity.Framework.Application.Version.Revision,
                "\nDustDevil VERSION: ",
                DustDevil.MajorVersion,
                ".",
                DustDevil.MinorVersion,
                ".",
                DustDevil.Revision,
                "#REVERT#"
            });
            Debug.Log("Final message is: " + label.Text);
            CreateModList(label);
            CreateModSettingsMenu(self);
            yield break;
        }

        private void CreateModList(AgePrimitiveLabel label)
        {
            Debug.Log("Attempting to create modlist!");
            Debug.Log("Label has position: " + label.transform.position);
            GameObject o = (GameObject)GameObject.Instantiate(label.gameObject, new Vector3(0, 0, 0), Quaternion.identity);
            o.transform.parent = label.gameObject.transform.parent;

            AgePrimitiveLabel l2 = o.GetComponent<AgePrimitiveLabel>();
            l2.Alignement = AgeTextAnchor.UpperLeft;
            string text = "ModList:\n";
            foreach (ScadMod mod in ModList)
            {
                text += "- " + mod.name + " - " + mod.MajorVersion + "." + mod.MinorVersion + "." + mod.Revision+"\n";
            }
            
            Debug.Log("Label has alignment: " + label.Alignement + " new label has alignment: " + l2.Alignement);
            Debug.Log("Label has 2d pos: " + label.Get2DPosition() + " new label has 2d pos: " + l2.Get2DPosition());
            Debug.Log("Label has agetransform: (" + label.AgeTransform.X + ", " + label.AgeTransform.Y + ", " + label.AgeTransform.Z + ")");
            Debug.Log("new label has agetransform: (" + l2.AgeTransform.X + ", " + l2.AgeTransform.Y + ", " + l2.AgeTransform.Z + ")");
            Debug.Log("Label pos vs new label pos: " + label.AgeTransform.Position.position + ", " + l2.AgeTransform.Position.position);

            l2.Text = text;
            Debug.Log("ModList Text: " + l2.Text);
            o.SetActive(true);
        }
        private void CreateModSettingsMenu(MainMenuPanel mainMenuPanel)
        {
            Debug.Log("Attempting to create ModSettings Menu!");

            AgeControlButton oldButton = new DynData<MainMenuPanel>(mainMenuPanel).Get<AgeControlButton>("continueButton");
            GameObject newO = (GameObject)GameObject.Instantiate(oldButton.gameObject, new Vector3(0, 0, 0), Quaternion.identity);

            AgeControlButton settingsMenuButton = newO.GetComponent<AgeControlButton>();
            Debug.Log("Old Menu Button Position: " + oldButton.transform.position + " new: " + settingsMenuButton.transform.position);
            newO.GetComponent<AgeTooltip>().Content = "Mod Settings";
            Debug.Log("Mod Settings Button Text: " + newO.GetComponent<AgeTooltip>().Content);
            newO.SetActive(true);
        }
        private void OnShowModSettingsMenu()
        {
            // Called when the ModMenu is opened.
            // First: Find all of the fields that are in all of the settings!
            foreach (ScadMod m in ModList)
            {
                foreach (FieldInfo f in m.settingsType.GetFields())
                {
                    // Now check attributes and if it is something that isn't displayable
                    object[] attributes = f.GetCustomAttributes(true);
                    bool show = true;
                    foreach (object attribute in attributes)
                    {
                        try
                        {
                            ModSettings.SettingsIgnore i = attribute as ModSettings.SettingsIgnore;
                            // The attribute is an ignore attribute, don't display it on the screen
                            show = false;
                            break;
                        } catch (InvalidCastException e)
                        {
                            // The Attribute isn't an ignore attribute
                        }
                    }
                    if (!(f.FieldType == typeof(float) || f.FieldType == typeof(int) || f.FieldType == typeof(bool)))
                    {
                        show = false;
                    }
                    if (f.FieldType == typeof(float) || f.FieldType == typeof(int))
                    {
                        // Required to have a SettingsRange
                    }
                    if (show)
                    {
                        // Show it to the screen using the provided attribute data and other stuff!
                        Debug.Log("ATTEMPTING TO SHOW FIELD: " + f.Name + " IN MOD: " + m.name);
                    }
                }
                Debug.Log("==========================================");
            }
        }
    }
}
