using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DustDevilFramework
{
    // Really, I should make a CustomPopupMenuPanel class and use that for everything
    // This is like a MonoBehavior or a Component, so it needs to be added to a GameObject to properly function!
    public class ModSettingsPopupMenuPanel : PopupMenuPanel
    {
        private OptionsPanel optionsPanel;
        private GameObject frame;
        List<ScadMod> mods;
        List<FieldInfo> fields;

        private readonly float toggleYDisplacement = 50;
        private readonly float settingSpacing = 200;

        public ModSettingsPopupMenuPanel()
        {
            Debug.Log("Constructing a ModSettings Popup Menu Panel for a GameObject!");
            fields = new List<FieldInfo>();
            Debug.Log("Construction of Component for GO Complete!");
        }
        private void SetupModlist(List<ScadMod> m)
        {
            Debug.Log("Setting up ModList!");
            mods = m;
            Debug.Log("Creating Frame");
            CreateFrame();
            Debug.Log("Creating Settings Components!");
            CreateSettings();
        }
        private void SetupOptionsPanel(OptionsPanel p)
        {
            Debug.Log("Setting OptionsPanel for Component!");
            optionsPanel = p;
        }

        public static ModSettingsPopupMenuPanel Create(List<ScadMod> m)
        {
            Debug.Log("Constructing a ModSettings Popup Menu Panel!");
            GameObject obj = SingletonManager.Get<OptionsPanel>(true).gameObject;
            OptionsPanel old = obj.GetComponent<OptionsPanel>();

            GameObject o = (GameObject)GameObject.Instantiate(obj, obj.transform.position, obj.transform.rotation);
            o.name = "ModSettingsPanel";
            foreach (Transform t in o.transform)
            {
                // Need to destroy all of the children of this GO
                Debug.Log("Destroying: " + t.gameObject);
                GameObject.DestroyImmediate(t.gameObject);
            }
            GameObject.DestroyImmediate(o.GetComponent<OptionsPanel>());
            ModSettingsPopupMenuPanel instanceForGO = o.AddComponent<ModSettingsPopupMenuPanel>();
            Debug.Log("Created InstanceForGO");
            instanceForGO.transform.parent = obj.transform.parent;
            Debug.Log("Current Parent: " + instanceForGO.transform.parent);
            instanceForGO.SetupOptionsPanel(old);
            instanceForGO.SetupModlist(m);

            Debug.Log("Components of Self:");
            foreach (Component _ in o.GetComponents(typeof(Component)))
            {
                Debug.Log("- " + _);
            }
            Debug.Log("Parent: " + o.transform.parent);
            Debug.Log("Parent of Parent: " + o.transform.parent.parent);
            Debug.Log("Components of 1st Parent:");
            foreach (Component _ in o.transform.parent.GetComponents(typeof(Component)))
            {
                Debug.Log("- " + _);
            }
            Debug.Log("Components of 2nd Parent:");
            foreach (Component _ in o.transform.parent.parent.GetComponents(typeof(Component)))
            {
                Debug.Log("- " + _);
            }
            return instanceForGO;

            //settingsPanel.Show(new object[0]);
            //Parent: GameResolutionScaler(UnityEngine.Transform)
            //Components:
            //- 50 - OptionsPanel_Mouse(Clone)(UnityEngine.Transform)
            //- 50 - OptionsPanel_Mouse(Clone)(AgeTransform)
            //- 50 - OptionsPanel_Mouse(Clone)(OptionsPanel)
            //- 50 - OptionsPanel_Mouse(Clone)(Singleton)
            //- 50 - OptionsPanel_Mouse(Clone)(AgeModifierAlpha)
        }
        protected override void Awake()
        {
            base.Awake();
            Hide();
        }

        // This method creates all of the AgeControl items that correspond to mod settings
        // They are placed within the optionsPanel instance variable
        private void CreateSettings()
        {
            Debug.Log("Creating Settings!");
            foreach (ScadMod m in mods)
            {
                Type basicType = m.settingsType;
                List<FieldInfo> fields = new List<FieldInfo>();
                while (!basicType.Equals(typeof(object)))
                {
                    
                    Debug.Log("Attempting to find inheritance tree with settings type: " + basicType);

                    fields.AddRange(basicType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    Debug.Log("Found: " + fields.Count + " so far.");
                    if (basicType.Equals(typeof(ModSettings)))
                    {
                        Debug.Log("Found a total of: " + fields.Count + " fields");
                        break;
                    }
                    if (basicType.BaseType.Equals(typeof(object)))
                    {
                        // You must provide a Settings class that extends ModSettings as the Settings Type!
                        Debug.LogError("You must provide a Settings class that extends ModSettings as the Settings Type for Mod: " + m.name);
                        break;
                    }
                    basicType = basicType.BaseType;
                }
                Debug.Log("Mod: " + m.name + " has settingsType: " + m.settingsType.Name + " with: " + fields.Count + " fields.");
                foreach (FieldInfo f in fields)
                {
                    // Now check attributes and if it is something that isn't displayable
                    bool show = true;
                    if (f.FieldType.Equals(typeof(float)) || f.FieldType.Equals(typeof(int)))
                    {
                        show = false;
                        // Required to have a SettingsRange
                    }

                    //object[] allCustomAttributes = f.GetCustomAttributes(true);
                    object[] allCustomAttributes = new object[0];
                    //TODO DEBUG!

                    Debug.Log("Attribute count: " + allCustomAttributes.Length);
                    foreach (object ob in allCustomAttributes)
                    {
                        //ModSettings.SettingsIgnore ignore = f.GetCustomAttributes<ModSettings.SettingsIgnore>(true).First();
                        if (ob.GetType().Equals(typeof(ModSettings.SettingsIgnore)))
                        {
                            // Hey, it is a SettingsIgnore attribute!
                            show = false;
                            Debug.Log("Ignoring setting with name: " + f.Name + " and type: " + f.FieldType);
                            break;
                        }
                        if (ob.GetType().Equals(typeof(ModSettings.SettingsRange)) && (f.FieldType.Equals(typeof(float)) || f.FieldType.Equals(typeof(int))))
                        {
                            ModSettings.SettingsRange range = ob as ModSettings.SettingsRange;
                            show = true;
                            Debug.Log("Showing float/int with name: " + Util.GetName(m, f) + " and type: " + f.FieldType + " because it has a SettingsRange of: " + range.Low + " to " + range.High + " with increment: " + range.Increment);
                            // Lets create a slider!
                            CreateSlider(m, f, range.Low, range.High, (float)f.GetValue(m.settings), range.Increment);
                            break;
                        }
                    }
                    if (show && f.FieldType.Equals(typeof(bool)))
                    {
                        Debug.Log("Showing bool with name: " + Util.GetName(m, f));
                        // Let's create a toggle!
                        CreateToggle(m, f, (bool)f.GetValue(m.settings));
                    }
                }
                Debug.Log("==========================================");
            }
        }
        public override void RefreshContent()
        {
            base.RefreshContent();
        }
        protected override IEnumerator OnLoad()
        {
            return base.OnLoad();
        }
        public override void Show(params object[] parameters)
        {
            Debug.Log("Showing ModSettings Panel!");
            base.Show(parameters);
            //optionsPanel.Show(parameters);
            //GameResolutionAgeScaler scaler = optionsPanel.transform.parent.GetComponent<GameResolutionAgeScaler>();
            //// Hopefully fixes the scaling problem!
            //typeof(GameResolutionAgeScaler).GetMethod("ApplyCurrentGameResolution", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(scaler, new object[0]);
        }
        protected override IEnumerator OnShow(params object[] parameters)
        {
            Debug.Log("OnShow!");
            return base.OnShow(parameters);
        }
        public override void Hide(bool instant = false)
        {
            Debug.Log("Hiding ModSettings Panel!");
            Debug.Log("Attempting to write settings to files!");
            foreach (ScadMod m in mods)
            {
                m.settings.WriteSettings();
            }
            Debug.Log("Wrote settings to files!");
            base.Hide(instant);
        }
        protected override IEnumerator OnHide(bool instant)
        {
            Debug.Log("OnHide!");
            return base.OnHide(instant);
        }
        public override void OnEscapeBehavior()
        {
            if (AgeManager.Instance.FocusedControl == null || IsVisible)
            {
                Selectable currentCategorySelectedElement = SingletonManager.Get<SelectableManager>(true).GetCurrentCategorySelectedElement();
                if (currentCategorySelectedElement != null)
                {
                    currentCategorySelectedElement.OnUnselect();
                }
                base.OnEscapeBehavior();
            }
        }
        private void OnCancelButtonClick(GameObject obj)
        {
            //this.saveSettings = false;
            Hide();
        }
        private void OnResetButtonClick()
        {
            //TODO Add!
            //requesterPanel.Display("%ResetControlsConfirmMessage", new global::RequesterPanel.ResultEventHandler(this.OnSettingsResetConfrim), global::RequesterPanel.ButtonsMode.YesNo, "%ResetControlsConfirmTitle", -1f, true);
        }
        private void OnConfirmButtonClick(GameObject obj)
        {
            Hide();
        }
        public void CreateFrame()
        {
            Debug.Log("Creating the Frame!");
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlToggle>("vSyncToggle").transform.parent.parent.parent.parent.gameObject;
            // oldObj in this case is the "Frame" object that we would like to duplicate.
            GameObject frame = (GameObject)GameObject.Instantiate(oldObj);
            // The frame is a child of the current display.
            frame.transform.parent = transform;
            // Delete the Frame from the GUI control, there can only be one frame (otherwise weird things happen)
            GameObject.DestroyImmediate(transform.FindChild("1-Frame").gameObject);
            GameObject bg = (GameObject)GameObject.Instantiate(optionsPanel.transform.FindChild("0-Bg").gameObject);
            bg.transform.parent = transform;

            transform.parent = optionsPanel.AgeTransform.GetParent().transform;
            transform.GetComponent<AgeTransform>().Position = optionsPanel.AgeTransform.Position;

            bg.name = "ModSettingsBackground";
            frame.name = "ModSettingsFrame";
            foreach (Component c in frame.GetComponents<Component>())
            {
                Debug.Log("- " + c);
            }
            Debug.Log("Children of the Frame:");
            foreach (Transform t in frame.transform)
            {
                Debug.Log("- " + t);
                Debug.Log("- Children:");
                foreach (Transform q in t)
                {
                    Debug.Log("-- " + q);
                }
            }
            // Destroys the useless LeftSide (which holds the controls)
            // Instead, we could put stuff like labels there, or something
            // We need to delete all of its children too, and its children's children, etc.

            //GameObject.DestroyImmediate(frame.transform.FindChild("2-LeftPart"));
            Util.DeleteChildrenInclusive(frame.transform.FindChild("2-LeftPart").gameObject);
            Util.DeleteChildrenExclusive(frame.transform.FindChild("3-RightPart").gameObject);
            Util.DeleteChildrenInclusive(frame.transform.FindChild("3-RightPart").FindChild("4-AudioConfig").gameObject);
            Debug.Log("Children of Right part after deletion");
            foreach (Transform t in frame.transform.FindChild("3-RightPart"))
            {
                Debug.Log("- " + t);
            }
            //GameObject.DestroyImmediate(frame.transform.FindChild("3-RightPart"));
            AgeControlButton cancel = frame.transform.FindChild("7-CancelButton").GetComponent<AgeControlButton>();
            AgeControlButton confirm = frame.transform.FindChild("9-ConfirmButton").GetComponent<AgeControlButton>();
            confirm.AgeTransform.Position = frame.transform.FindChild("9-ConfirmButton").GetComponent<AgeControlButton>().AgeTransform.Position; // Fixes a strange bug
            AgeControlButton reset = frame.transform.FindChild("8-ResetButton").GetComponent<AgeControlButton>();
            cancel.OnActivateObject = gameObject;
            confirm.OnActivateObject = gameObject;
            reset.OnActivateObject = gameObject;

            frame.GetComponent<AgeTransform>().GetParent().Position = optionsPanel.GetComponent<AgeTransform>().GetParent().Position;
            //frame.GetComponent<AgeTransform>().AutoResizeHeight = false;
            //frame.GetComponent<AgeTransform>().AutoResizeWidth = false;
            frame.GetComponent<AgeTransform>().Position = optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>().Position;

            Debug.Log("Children of (AgeTransform) Frame:");
            foreach (AgeTransform t in frame.GetComponent<AgeTransform>().GetChildren())
            {
                Debug.Log("- " + t.name + " " + t.Position);
            }
            Debug.Log("Frame AgeTransform: " + frame + " " + frame.GetComponent<AgeTransform>().Position);
            Debug.Log("Parent (AgeTransform): " + frame.GetComponent<AgeTransform>().GetParent().name + " " + frame.GetComponent<AgeTransform>().GetParent().Position);
            Debug.Log("Panel AgeTransform: " + name + " " + GetComponent<AgeTransform>().Position);
            Debug.Log("Parent of Panel: " + GetComponent<AgeTransform>().GetParent().name + " " + GetComponent<AgeTransform>().GetParent().Position);
            Debug.Log("OptionsPanel AgeTransform: " + optionsPanel.GetComponent<AgeTransform>().name + " " + optionsPanel.GetComponent<AgeTransform>().Position);
            Debug.Log("OptionsPanel Parent (AgeTransform): " + optionsPanel.GetComponent<AgeTransform>().GetParent().name + " " + optionsPanel.GetComponent<AgeTransform>().GetParent().Position);
            Debug.Log("OptionsPanel Parent Parent: " + optionsPanel.GetComponent<AgeTransform>().GetParent().GetParent().name + " " + optionsPanel.GetComponent<AgeTransform>().GetParent().GetParent().Position);
            Debug.Log("Children of (AgeTransform) OptionsPanel:");
            foreach (AgeTransform t in optionsPanel.GetComponent<AgeTransform>().GetChildren())
            {
                Debug.Log("- " +  t.name + " " + t.Position);
            }
            //frame.GetComponent<AgeTransform>().ForceHeight(optionsPanel.GetComponent<AgeTransform>().Height);

            this.frame = frame;
        }
        public void CreateSlider(ScadMod m, FieldInfo f, float low, float high, float current, float increment)
        {
            Debug.Log("Creating Slider with name: " + Util.GetName(m, f));
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlSlider>("masterVolSlider").gameObject;
            GameObject slider = (GameObject)GameObject.Instantiate(oldObj);
            slider.transform.parent = transform;
            Debug.Log("Parent: " + slider.transform.parent);
            Debug.Log("Slider components:");
            foreach (Component _ in slider.GetComponents(typeof(Component)))
            {
                Debug.Log("- " + _);
            }
            AgeControlSlider sliderControl = slider.GetComponent<AgeControlSlider>();
            sliderControl.CurrentValue = current;
            sliderControl.MinValue = low;
            sliderControl.MaxValue = high;
            sliderControl.Increment = increment;
            slider.GetComponent<AgeTooltip>().Content = Util.GetName(m, f);
            Debug.Log("AgeTfm: " + sliderControl.AgeTransform.Get2DPosition());
            Debug.Log("Arm - Method: " + sliderControl.OnArmMethod + " GameObject: " + sliderControl.OnArmObject);
            Debug.Log("Drag - Method: " + sliderControl.OnDragMethod + " GameObject: " + sliderControl.OnDragObject);
            Debug.Log("Release - Method: " + sliderControl.OnReleaseMethod + " GameObject: " + sliderControl.OnReleaseObject);

            fields.Add(f);
        }
        public void CreateToggle(ScadMod m, FieldInfo f, bool current)
        {
            // I believe the Old Object's parent is a special display component that contains both text and the toggle button
            Debug.Log("Creating Toggle with name: " + Util.GetName(m, f));
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlToggle>("vSyncToggle").transform.parent.gameObject;
            // oldObj is now the group object!

            Debug.Log("Old Parent: " + oldObj.transform.parent);
            Debug.Log("Old Parent's Parent: " + oldObj.transform.parent.parent);
            Debug.Log("Old Parent's Parent's Parent: " + oldObj.transform.parent.parent.parent);

            GameObject toggleGroup = (GameObject)GameObject.Instantiate(oldObj);
            toggleGroup.name = Util.GetName(m, f);
            toggleGroup.transform.parent = frame.transform.FindChild("3-RightPart");
            Debug.Log("Toggle Group Object Created!");

            Debug.Log("New Parent: " + toggleGroup.transform.parent);
            Debug.Log("New Parent's Parent: " + toggleGroup.transform.parent.parent);
            Debug.Log("New Parent's Parent's Parent: " + toggleGroup.transform.parent.parent.parent);
            //toggleGroup.transform.parent = transform;

            //Debug.Log("Parent: " + toggleGroup.transform.parent);
            //Debug.Log("Toggle group components:");
            //foreach (Component _ in toggleGroup.GetComponents(typeof(Component)))
            //{
            //    Debug.Log("- " + _);
            //}
            Debug.Log("Components of Parent:");
            foreach (Component _ in toggleGroup.transform.parent.GetComponents(typeof(Component)))
            {
                Debug.Log("- " + _);
            }

            Transform toggle = toggleGroup.transform.GetChild(2);
            toggle.gameObject.name = Util.GetName(m,f) + "_Toggle";
            Debug.Log("Toggle Object: " + toggle.gameObject);
            AgeControlToggle toggleControl = toggle.GetComponent<AgeControlToggle>();
            toggleControl.State = current;
            Debug.Log("Set state to: " + current);
            toggle.GetComponent<AgeTooltip>().Content = Util.GetName(m, f);
            Debug.Log("Finding Label Object");
            Transform label = toggleGroup.transform.GetChild(1);
            label.gameObject.name = Util.GetName(m, f) + "_Label";
            Debug.Log("Label Object: " + label.gameObject);
            label.GetComponent<AgePrimitiveLabel>().Text = Util.GetName(m, f);
            toggleGroup.GetComponent<AgeTransform>().Position = oldObj.GetComponent<AgeTransform>().Position;
            toggle.GetComponent<AgeTransform>().Position = oldObj.transform.GetChild(2).GetComponent<AgeTransform>().Position;
            toggleGroup.GetComponent<AgeTransform>().Y = 0;
            Debug.Log("Old Toggle Button AgeTfm: " + oldObj.transform.GetChild(2).GetComponent<AgeTransform>().Position);
            Debug.Log("New Toggle Button AgeTfm: " + toggle.GetComponent<AgeTransform>().Position);
            //toggleGroup.GetComponent<AgeTransform>().Y += toggleYDisplacement - (settingSpacing + toggleGroup.GetComponent<AgeTransform>().Height) * fields.Count;
            //toggleGroup.GetComponent<AgeTransform>().ForceHeight(oldObj.GetComponent<AgeTransform>().Height * 2);
            Debug.Log("Old AgeTfm: " + oldObj.GetComponent<AgeTransform>().Position);
            Debug.Log("AgeTfm: " + toggleGroup.GetComponent<AgeTransform>().Position);
            toggleControl.OnSwitchMethod = "OnTogglePressed";
            toggleControl.OnSwitchObject = transform.gameObject;
            Debug.Log("Switch - Method: " + toggleControl.OnSwitchMethod + " GameObject: " + toggleControl.OnSwitchObject);


            fields.Add(f);
        }
        public void CreateScrollArea()
        {
            AgeControlScrollView scroll = new DynData<OptionsPanel>(optionsPanel).Get<AgeControlScrollView>("controlBindingsScrollView");
            Debug.Log("VirutalArea pos: " + scroll.VirtualArea.Position);
            Debug.Log("VirtualArea Children Length: " + scroll.VirtualArea.GetChildren().Count);
            Debug.Log("ViewPort pos: " + scroll.Viewport.Position);
            Debug.Log("ViewPort Children Length: " + scroll.Viewport.GetChildren().Count);
            Debug.Log("Parent: " + scroll.transform.parent);
            Debug.Log("Age Transform Pos: " + scroll.AgeTransform.Position);
            Debug.Log("Components:");
            foreach (Component _ in scroll.GetComponents(typeof(Component)))
            {
                Debug.Log("- " + _);
            }
        }
        private void OnTogglePressed(GameObject o)
        {
            Debug.Log("Registered a Toggle Press! GO: " + o);
            foreach (ScadMod mod in mods)
            {
                foreach (FieldInfo f in fields)
                {
                    // We need to set the field that corresponds to this toggle to the toggle value
                    // We can cheat by checking the name of the AgeTooltip attatched to 'o'
                    if (o.GetComponent<AgeTooltip>().Content == Util.GetName(mod, f))
                    {
                        // This is the field that needs to be modified.
                        // It should be a bool, if the button is working as intended!
                        Debug.Log("Setting value of: " + Util.GetName(mod, f) + " to: " + o.GetComponent<AgeControlToggle>().State + " in settings type: " + mod.settingsType + " in mod: " + mod.name);
                        f.SetValue(mod.settings, o.GetComponent<AgeControlToggle>().State);
                        return;
                    }
                }
            }
            Debug.Log("A toggle button exists but it doesn't have a matching field in all mods!");
        }
        private void OnSliderDragged(GameObject o)
        {
            Debug.Log("Registered a Drag Event Press! GO: " + o);
            foreach (ScadMod mod in mods)
            {
                foreach (FieldInfo f in fields)
                {
                    // We need to set the field that corresponds to this toggle to the toggle value
                    // We can cheat by checking the name of the AgeTooltip attatched to 'o'
                    if (o.GetComponent<AgeTooltip>().Content == Util.GetName(mod, f))
                    {
                        // This is the field that needs to be modified.
                        // It should be a bool, if the button is working as intended!
                        Debug.Log("Setting value of: " + Util.GetName(mod, f) + " to: " + o.GetComponent<AgeControlSlider>().CurrentValue + " in settings type: " + mod.settingsType + " in mod: " + mod.name);
                        f.SetValue(mod.settings, o.GetComponent<AgeControlSlider>().CurrentValue);
                        return;
                    }
                }
            }
            Debug.Log("A slider exists but it doesn't have a matching field in all mods!");
        }
    }
}
