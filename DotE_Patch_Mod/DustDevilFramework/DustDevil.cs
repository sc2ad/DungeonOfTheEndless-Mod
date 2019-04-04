using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DustDevilFramework
{
    public class DustDevil
    {
        private static DustDevil Instance;

        public static int MajorVersion { get; } = 4;
        public static int MinorVersion { get; } = 0;
        public static int Revision { get; } = 7;

        private static List<ScadMod> ModList = new List<ScadMod>();
        private static ModSettingsPopupMenuPanel SettingsPanel;

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

        private System.Collections.IEnumerator MainMenuPanel_OnLoad(On.MainMenuPanel.orig_OnLoad orig, MainMenuPanel self)
        {
            if (SettingsPanel == null)
            {
                SettingsPanel = ModSettingsPopupMenuPanel.Create(ModList);
            }
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

            AgeControlButton oldButton = new DynData<MainMenuPanel>(mainMenuPanel).Get<AgeControlButton>("multiplayerButton");
            GameObject newO = (GameObject)GameObject.Instantiate(oldButton.gameObject, oldButton.transform.position, oldButton.transform.localRotation);
            //GameObject newO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //newO.GetComponent<MeshRenderer>().enabled = false;
            //newO.transform.position = oldButton.transform.position;

            //AgeControlButton settingsMenuButton = newO.AddComponent<AgeControlButton>();
            AgeControlButton settingsMenuButton = newO.GetComponent<AgeControlButton>();
            //AgeControlButton settingsMenuButton = oldButton;
            settingsMenuButton.gameObject.name = "Mod Settings";
            settingsMenuButton.AgeTransform.Enable = true;
            settingsMenuButton.AgeTransform.X = oldButton.AgeTransform.X;
            settingsMenuButton.AgeTransform.Y = oldButton.AgeTransform.Y;

            GameObject.Destroy(settingsMenuButton.GetComponent<DestroyerIfNoMultiplayer>());

            Component[] c = settingsMenuButton.GetComponents(typeof(Component));
            //Mod Settings(UnityEngine.Transform)
            //Mod Settings(AgeTransform)
            //Mod Settings(AgeControlButton)
            //Mod Settings(AgeTooltip)
            //Mod Settings(AgeAudio)
            //Mod Settings(AGESelectable)

            //Child: 0-BG (UnityEngine.GameObject)
            //Components:
            //- 0-BG (UnityEngine.Transform)
            //- 0-BG (AgeTransform)
            //- 0-BG (AgePrimitiveImage)
            //- 0-BG (AgeModifierColorSwitch)
            //Child: 1-Label (UnityEngine.GameObject)
            //Components:
            //- 1-Label (UnityEngine.Transform)
            //- 1-Label (AgeTransform)
            //- 1-Label (AgePrimitiveLabel)
            //- 1-Label (AgeModifierColorSwitch)
            foreach (Component q in c)
            {
                Debug.Log(q.ToString());
            }

            newO.GetComponentInChildren<AgePrimitiveLabel>().Text = "Mod Settings";

            //foreach (Transform child in oldButton.gameObject.transform)
            //{
            //    Debug.Log("Child: " + child.gameObject.ToString());
            //    Debug.Log("Components:");
            //    foreach (Component _ in child.gameObject.GetComponents(typeof(Component)))
            //    {
            //        Debug.Log("- " + _.ToString());
            //    }
            //}

            //settingsMenuButton.GetComponent<AgeControlTextArea>().AgePrimitiveLabel.Text = "Mod Settings";

            Debug.Log("Old Menu Button Position: " + oldButton.transform.position + " new: " + settingsMenuButton.transform.position);
            Debug.Log("Old Menu Buttom AgeTransform: (" + oldButton.AgeTransform.X + ", " + oldButton.AgeTransform.Y + ", " + oldButton.AgeTransform.Z + ") new: (" + settingsMenuButton.AgeTransform.X + ", " + settingsMenuButton.AgeTransform.Y + ", " + settingsMenuButton.AgeTransform.Z + ")");
            Debug.Log("Old Name: " + oldButton.name + ", new name: " + settingsMenuButton.name);

            Debug.Log("Old Category: " + oldButton.GetComponent<AGESelectable>().Category);
            Debug.Log("Old SubCategory: " + oldButton.GetComponent<AGESelectable>().SubCategoryID);
            Debug.Log("Old Next Category: " + oldButton.GetComponent<AGESelectable>().NextCategory);

            AGESelectable selectable = newO.GetComponent<AGESelectable>();
            // Or Something!
            // SelectionCategory only goes until 28
            

            Debug.Log("GameObject of oldButton: " + oldButton.gameObject);

            DynData<AGESelectable> d = new DynData<AGESelectable>(oldButton.GetComponent<AGESelectable>());
            //Debug.Log("Old AGE AgeSelectionMarker: " + d.Get<AGESelectionMarker>("marker").ToString());
            Debug.Log("Old AGE Position: " + oldButton.GetComponent<AGESelectable>().Get2DPosition());
            Debug.Log("Old AGE Prev Category: " + d.Get<SelectionCategoryData>("previousCategory").Category + ", " + d.Get<SelectionCategoryData>("previousCategory").SubCategoryID);
            Debug.Log("Old AGE AgeControl: " + d.Get<AgeControl>("ageControl").ToString());

            DynData<AGESelectable> d2 = new DynData<AGESelectable>(selectable);
            AgeTransform positionAgeTfm = d2.Get<AgeTransform>("positionAgeTfm");
            DynData<AgeTransform> oldPosTfm = new DynData<AgeTransform>(positionAgeTfm);
            DynData<AgeTransform> newAgeTfm = new DynData<AgeTransform>(selectable.AgeTfm);
            DynData<AgeTransform> oldBtnPosTfm = new DynData<AgeTransform>(d.Get<AgeTransform>("positionAgeTfm"));

            //settingsMenuButton.gameObject.AddComponent<AgeTooltip>();
            newO.GetComponent<AgeTooltip>().Content = "Mod Settings";

            selectable.Register((SelectionCategory)29);
            Debug.Log("Attempting to set display!");
            selectable.SetDisplay(true);
            Debug.Log("Set Display!");

            Debug.Log("Old ActiveData: " + oldButton.GetComponent<AgeControlButton>().OnActivateData);
            Debug.Log("Old ActiveObject: " + oldButton.GetComponent<AgeControlButton>().OnActivateDataObject);
            Debug.Log("Old ActiveMethod: " + oldButton.GetComponent<AgeControlButton>().OnActivateMethod);
            Debug.Log("Old ActiveGameObject: " + oldButton.GetComponent<AgeControlButton>().OnActivateObject.ToString());

            newO.AddComponent<ClickHandler>();
            newO.GetComponent<AgeControlButton>().OnActivateMethod = "ShowModSettingsMenu";
            newO.GetComponent<AgeControlButton>().OnActivateObject = newO;

            Debug.Log("New ActiveData: " + newO.GetComponent<AgeControlButton>().OnActivateData);
            Debug.Log("New ActiveObject: " + newO.GetComponent<AgeControlButton>().OnActivateDataObject);
            Debug.Log("New ActiveMethod: " + newO.GetComponent<AgeControlButton>().OnActivateMethod);
            Debug.Log("New ActiveGameObject: " + newO.GetComponent<AgeControlButton>().OnActivateObject.ToString());


            // Tricks it into thinking position is the same
            newO.transform.SetParent(oldButton.transform.parent);
            Rect newRect = oldBtnPosTfm.Get<Rect>("basePosition");
            newRect.y += oldButton.AgeTransform.Height / 2.0f + 2; 

            oldPosTfm.Set("basePosition", newRect);
            newAgeTfm.Set("basePosition", newRect);


            Debug.Log("Old Transform Parent? " + oldButton.transform.parent);
            Debug.Log("New Transform Parent? " + newO.transform.parent);

            Debug.Log("New positionAgeTfm: (" + positionAgeTfm.X + ", " + positionAgeTfm.Y + ")");
            Debug.Log("New AgeTfm: (" + selectable.AgeTfm.X + ", " + selectable.AgeTfm.Y + ")");
            Debug.Log("Base Positions (Rect ToString): " + oldBtnPosTfm.Get<Rect>("basePosition").ToString());
            Debug.Log("New Positions (Rect ToString): " + newAgeTfm.Get<Rect>("basePosition").ToString());
            string text = "Parent is null!";
            if (d.Get<AgeTransform>("positionAgeTfm").GetParent() != null)
            {
                text = d.Get<AgeTransform>("positionAgeTfm").GetParent().X + ", " + d.Get<AgeTransform>("positionAgeTfm").GetParent().Y;
            }
            Debug.Log("Old PositionAgeTfm Parent? " + text);
            text = "Parent is null!";
            if (selectable.AgeTfm.GetParent() != null)
            {
                text = selectable.AgeTfm.GetParent().X + ", " + selectable.AgeTfm.GetParent().Y;
            }
            Debug.Log("New PositionAgeTfm Parent? " + text);

            Debug.Log("Old Monitor Changes? " + d.Get<bool>("monitorPositionChanges"));
            Debug.Log("New Monitor Changes? " + d2.Get<bool>("monitorPositionChanges"));
            Debug.Log("Old 2d offset: " + d.Get<Vector2>("twoDPositionOffset"));
            Debug.Log("New 2d offset: " + d2.Get<Vector2>("twoDPositionOffset"));
            Debug.Log("Old Position: " + d.Get<Vector2>("position"));
            Debug.Log("New Position: " + d2.Get<Vector2>("position"));
            Debug.Log("Old CenterXPosition: " + d.Get<bool>("centerXPosition"));
            Debug.Log("New CenterXPosition: " + d2.Get<bool>("centerXPosition"));
            Debug.Log("Old Transform LocalPosition: " + oldButton.transform.localPosition);
            Debug.Log("New Transform LocalPosition: " + settingsMenuButton.transform.localPosition);
            Debug.Log("Old Transform LocalRotation: " + oldButton.transform.localRotation);
            Debug.Log("New Transform LocalRotation: " + settingsMenuButton.transform.localRotation);

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
            oldButton.AgeTransform.ForceHeight(temp / 2.0f - 2);
            settingsMenuButton.AgeTransform.ForceHeight(temp / 2.0f - 2);

            Debug.Log("Mod Settings Button Text: " + newO.GetComponent<AgeTooltip>().Content);
            newO.SetActive(true);
        }
        private class ClickHandler : MonoBehaviour
        {
            public void ShowModSettingsMenu(GameObject o)
            {
                Debug.Log("Attempting to show mod settings menu!");
                SettingsPanel.Show(new object[0]);
            }
        }
    }
}
