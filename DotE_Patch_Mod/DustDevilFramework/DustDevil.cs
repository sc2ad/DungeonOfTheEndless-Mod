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
        public static int MinorVersion { get; } = 5;
        public static int Revision { get; } = 5;

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
            //On.MainMenuPanel.RefreshContent += MainMenuPanel_RefreshContent;
        }

        private void MainMenuPanel_RefreshContent(On.MainMenuPanel.orig_RefreshContent orig, MainMenuPanel self)
        {
            AgeControlButton oldButton = new DynData<MainMenuPanel>(self).Get<AgeControlButton>("multiplayerButton");
            Debug.Log("Height: " + oldButton.AgeTransform.Height);
            Debug.Log("Is Table Arranged? " + oldButton.AgeTransform.TableArrangement);
            Debug.Log("Data: " + oldButton.OnActivateData);
            Debug.Log("Data Object: " + oldButton.OnActivateDataObject);
            Debug.Log("Data Method: " + oldButton.OnActivateMethod);
            Debug.Log("Data GameObject: " + oldButton.OnActivateObject);

            Debug.Log("Old Menu Button Position: " + oldButton.transform.position);
            Debug.Log("Old Menu Buttom AgeTransform: (" + oldButton.AgeTransform.X + ", " + oldButton.AgeTransform.Y + ", " + oldButton.AgeTransform.Z + ")");
            Debug.Log("Old Name: " + oldButton.name);
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
            GameObject newO = (GameObject)GameObject.Instantiate(oldButton.gameObject);
            //GameObject newO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //newO.GetComponent<MeshRenderer>().enabled = false;
            //newO.transform.position = oldButton.transform.position;

            //AgeControlButton settingsMenuButton = newO.AddComponent<AgeControlButton>();
            AgeControlButton settingsMenuButton = newO.GetComponent<AgeControlButton>();
            //AgeControlButton settingsMenuButton = oldButton;
            settingsMenuButton.name = "Mod Settings";
            settingsMenuButton.AgeTransform.Enable = true;
            settingsMenuButton.AgeTransform.X = oldButton.AgeTransform.X;
            settingsMenuButton.AgeTransform.Y = oldButton.AgeTransform.Y;
            Component[] c = settingsMenuButton.GetComponents(typeof(Component));
            //Mod Settings(UnityEngine.Transform)
            //Mod Settings(AgeTransform)
            //Mod Settings(AgeControlButton)
            //Mod Settings(AgeTooltip)
            //Mod Settings(AgeAudio)
            //Mod Settings(AGESelectable)
            foreach (Component q in c)
            {
                Debug.Log(q.ToString());
            }
            //settingsMenuButton.GetComponent<AgeControlTextArea>().AgePrimitiveLabel.Text = "Mod Settings";

            Debug.Log("Data: " + oldButton.OnActivateData);
            Debug.Log("Data Object: " + oldButton.OnActivateDataObject);
            Debug.Log("Data Method: " + oldButton.OnActivateMethod);
            Debug.Log("Data GameObject: " + oldButton.OnActivateObject);

            Debug.Log("Old Menu Button Position: " + oldButton.transform.position + " new: " + settingsMenuButton.transform.position);
            Debug.Log("Old Menu Buttom AgeTransform: (" + oldButton.AgeTransform.X + ", " + oldButton.AgeTransform.Y + ", " + oldButton.AgeTransform.Z + ") new: (" + settingsMenuButton.AgeTransform.X + ", " + settingsMenuButton.AgeTransform.Y + ", " + settingsMenuButton.AgeTransform.Z + ")");
            Debug.Log("Old Name: " + oldButton.name + ", new name: " + settingsMenuButton.name);

            Debug.Log("Old Category: " + oldButton.GetComponent<AGESelectable>().Category);
            Debug.Log("Old SubCategory: " + oldButton.GetComponent<AGESelectable>().SubCategoryID);
            Debug.Log("Old Next Category: " + oldButton.GetComponent<AGESelectable>().NextCategory);

            //settingsMenuButton.gameObject.AddComponent<AgeTooltip>();
            newO.GetComponent<AgeTooltip>().Content = "Mod Settings";
            AGESelectable selectable = newO.GetComponent<AGESelectable>();
            // Or Something!
            // SelectionCategory only goes until 28
            
            selectable.Register((SelectionCategory)29);
            Debug.Log("Attempting to set display!");
            selectable.SetDisplay(true);
            Debug.Log("Set Display!");
            Debug.Log("GameObject of oldButton: " + oldButton.gameObject);

            DynData<AGESelectable> d = new DynData<AGESelectable>(oldButton.GetComponent<AGESelectable>());
            //Debug.Log("Old AGE AgeSelectionMarker: " + d.Get<AGESelectionMarker>("marker").ToString());
            Debug.Log("Old AGE Position: " + oldButton.GetComponent<AGESelectable>().Get2DPosition());
            Debug.Log("Old AGE Prev Category: " + d.Get<SelectionCategoryData>("previousCategory").Category + ", " + d.Get<SelectionCategoryData>("previousCategory").SubCategoryID);
            Debug.Log("Old AGE AgeControl: " + d.Get<AgeControl>("ageControl").ToString());

            DynData<AGESelectable> d2 = new DynData<AGESelectable>(selectable);
            //Debug.Log("New AGE AgeSelectionMarker: " + d2.Get<AGESelectionMarker>("marker").ToString());

            // Tricks it into thinking position is the same

            AgeTransform positionAgeTfm = d2.Get<AgeTransform>("positionAgeTfm");
            DynData<AgeTransform> oldPosTfm = new DynData<AgeTransform>(positionAgeTfm);
            DynData<AgeTransform> newAgeTfm = new DynData<AgeTransform>(selectable.AgeTfm);

            Debug.Log("New positionAgeTfm: (" + positionAgeTfm.X + ", " + positionAgeTfm.Y + ")");
            Debug.Log("New AgeTfm: (" + selectable.AgeTfm.X + ", " + selectable.AgeTfm.Y + ")");
            Debug.Log("Base Positions (Rect pos): " + oldPosTfm.Get<Rect>("basePosition").position);
            Debug.Log("New Positions (Rect pos): " + newAgeTfm.Get<Rect>("basePosition").position);
            string text = "Parent is null!";
            if (positionAgeTfm.GetParent() != null)
            {
                text = positionAgeTfm.GetParent().X + ", " + positionAgeTfm.GetParent().Y;
            }
            Debug.Log("Old Parent? " + text);
            text = "Parent is null!";
            if (selectable.AgeTfm.GetParent() != null)
            {
                text = selectable.AgeTfm.GetParent().X + ", " + selectable.AgeTfm.GetParent().Y;
            }
            Debug.Log("New Parent? " + text);

            Debug.Log("Old Monitor Changes? " + d.Get<bool>("monitorPositionChanges"));
            Debug.Log("New Monitor Changes? " + d2.Get<bool>("monitorPositionChanges"));
            Debug.Log("Old 2d offset: " + d.Get<Vector2>("twoDPositionOffset"));
            Debug.Log("New 2d offset: " + d2.Get<Vector2>("twoDPositionOffset"));
            Debug.Log("Old Position: " + d.Get<Vector2>("position"));
            Debug.Log("New Position: " + d2.Get<Vector2>("position"));
            Debug.Log("Old CenterXPosition: " + d.Get<bool>("centerXPosition"));
            Debug.Log("New CenterXPosition: " + d2.Get<bool>("centerXPosition"));

            //d2.Set("positionAgeTfm", d.Get<AgeTransform>("positionAgeTfm"));
            //d2.Set("position", d.Get<Vector2>("position"));

            Debug.Log("New AGE Position: " + selectable.Get2DPosition());
            Debug.Log("New AGE Prev Category: " + d2.Get<SelectionCategoryData>("previousCategory").Category + ", " + d2.Get<SelectionCategoryData>("previousCategory").SubCategoryID);
            Debug.Log("New AGE AgeControl: " + d2.Get<AgeControl>("ageControl").ToString());

            //GameObject.Destroy(settingsMenuButton.GetComponent<DestroyerIfNoMultiplayer>());
            // Need to add to the enum here: SelectionCategory needs to include ModSettings as an option, and then it needs to be set here
            Debug.Log("Category: " + selectable.Category);
            Debug.Log("SubCategory: " + selectable.SubCategoryID);
            Debug.Log("Next Category: " + selectable.NextCategory);

            float temp = oldButton.AgeTransform.Height;
            oldButton.AgeTransform.ForceHeight(temp / 2.0f);
            settingsMenuButton.AgeTransform.ForceHeight(temp);

            Debug.Log("Mod Settings Button Text: " + newO.GetComponent<AgeTooltip>().Content);
            newO.SetActive(true);
        }
        
        private class ModSettingsClickHandler : MonoBehaviour
        {
            public ModSettingsClickHandler()
            {
            }
            private void OnShowModSettingsMenu()
            {
                // Called when the ModMenu is opened.
                // First: Find all of the fields that are in all of the settings!
                // Also need to create the buttons and stuff and store them as instance variables
                foreach (ScadMod m in ModList)
                {
                    foreach (FieldInfo f in m.settingsType.GetFields())
                    {
                        // Now check attributes and if it is something that isn't displayable
                        object[] attributes = f.GetCustomAttributes(true);
                        bool show = true;
                        foreach (object attribute in attributes)
                        {
                            /*
                             * if (attribute instanceof ModSettings.SettingsIgnore)
                             */
                            try
                            {
                                ModSettings.SettingsIgnore i = attribute as ModSettings.SettingsIgnore;
                                // The attribute is an ignore attribute, don't display it on the screen
                                show = false;
                                break;
                            }
                            catch (InvalidCastException e)
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
            private void OnHideModSettingsMenu()
            {
                // Called when the ModMenu is closed.
                // Just need to delete all the buttons and make sure everything is adequately removed
            }
        }
    }
}
