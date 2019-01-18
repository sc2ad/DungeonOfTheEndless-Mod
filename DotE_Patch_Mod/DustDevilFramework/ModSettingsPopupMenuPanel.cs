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
        List<ScadMod> mods;
        List<FieldInfo> fields;

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
            GameObject toggleGroup = (GameObject)GameObject.Instantiate(oldObj);
            toggleGroup.name = Util.GetName(m, f);
            Debug.Log("Toggle Group Object Created!");
            //toggleGroup.transform.parent = transform;
            toggleGroup.transform.parent = oldObj.transform.parent;
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
            //Debug.Log("Children of Parent:");
            //for (int i = 0; i < toggleGroup.transform.parent.childCount; i++)
            //{
            //    Transform t = toggleGroup.transform.parent.GetChild(i);
            //    Debug.Log("- ID: " + i + ": " + t.gameObject);
            //    Debug.Log("- Components:");
            //    foreach (Component _ in t.GetComponents(typeof(Component)))
            //    {
            //        Debug.Log("-- " + _);
            //    }
            //}

            //Children of 24 - VSyncGroup(Clone)(UnityEngine.GameObject):
            //-ID: 0: 1 - Background(UnityEngine.GameObject)
            //- Components:
            //-- 1 - Background(UnityEngine.Transform)
            //-- 1 - Background(AgeTransform)
            //-- 1 - Background(AgePrimitiveImage)
            //-- 1 - Background(AgeModifierColorSwitch)
            //- ID: 1: 2 - Label(UnityEngine.GameObject)
            //- Components:
            //-- 2 - Label(UnityEngine.Transform)
            //-- 2 - Label(AgeTransform)
            //-- 2 - Label(AgePrimitiveLabel)
            //-- 2 - Label(AgeModifierColorSwitch)
            //- ID: 2: 2 - VSyncToggle(UnityEngine.GameObject)
            //- Components:
            //-- 2 - VSyncToggle(UnityEngine.Transform)
            //-- 2 - VSyncToggle(AgeTransform)
            //-- 2 - VSyncToggle(AgeControlToggle)
            //-- 2 - VSyncToggle(AgeAudio)
            //-- 2 - VSyncToggle(AgeTooltip)

            //Debug.Log("Children of " + toggleGroup.gameObject + ":");
            //for (int i = 0; i < toggleGroup.transform.childCount; i++)
            //{
            //    Transform t = toggleGroup.transform.GetChild(i);
            //    Debug.Log("- ID: " + i + ": " + t.gameObject);
            //    Debug.Log("- Components:");
            //    foreach (Component _ in t.GetComponents(typeof(Component)))
            //    {
            //        Debug.Log("-- " + _);
            //    }
            //}
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
            toggleGroup.GetComponent<AgeTransform>().ForceHeight(oldObj.GetComponent<AgeTransform>().Height * 2);
            Debug.Log("Old AgeTfm: " + oldObj.GetComponent<AgeTransform>().Position);
            Debug.Log("AgeTfm: " + toggleGroup.GetComponent<AgeTransform>().Position);
            Debug.Log("Old Position: " + oldObj.transform.position);
            Debug.Log("New Position: " + toggleGroup.transform.position);
            Debug.Log("Old 2d: " + oldObj.Get2DPosition());
            Debug.Log("New 2d: " + toggleGroup.Get2DPosition());
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
