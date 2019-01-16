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
    // Really, I should make a CustomPopupMenuPanel class and use that for everything
    public class ModSettingsPopupMenuPanel : PopupMenuPanel
    {
        private OptionsPanel optionsPanel;
        List<ScadMod> mods;
        List<FieldInfo> fields;
        public ModSettingsPopupMenuPanel(List<ScadMod> m)
        {
            Debug.Log("Constructing a ModSettings Popup Menu Panel!");
            GameObject obj = (GameObject)GameObject.Instantiate(SingletonManager.Get<OptionsPanel>(true).gameObject, SingletonManager.Get<OptionsPanel>(true).transform.position, SingletonManager.Get<OptionsPanel>(true).transform.rotation);
            obj.transform.parent = SingletonManager.Get<OptionsPanel>(true).transform.parent;
            optionsPanel = obj.GetComponent<OptionsPanel>();
            mods = m;
            fields = new List<FieldInfo>();
            CreateSettings();
            //settingsPanel.Show(new object[0]);
            //Parent: GameResolutionScaler(UnityEngine.Transform)
            //Components:
            //- 50 - OptionsPanel_Mouse(Clone)(UnityEngine.Transform)
            //- 50 - OptionsPanel_Mouse(Clone)(AgeTransform)
            //- 50 - OptionsPanel_Mouse(Clone)(OptionsPanel)
            //- 50 - OptionsPanel_Mouse(Clone)(Singleton)
            //- 50 - OptionsPanel_Mouse(Clone)(AgeModifierAlpha)
        }
        // This method creates all of the AgeControl items that correspond to mod settings
        // They are placed within the optionsPanel instance variable
        private void CreateSettings()
        {
            foreach (ScadMod m in mods)
            {
                Debug.Log("Mod: " + m.name + " has settingsType: " + m.settingsType.Name + " with: " + m.settingsType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length + " fields.");
                foreach (FieldInfo f in m.settingsType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
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
                            Debug.Log("Showing float/int with name: " + Util.GetName(f) + " and type: " + f.FieldType + " because it has a SettingsRange of: " + range.Low + " to " + range.High + " with increment: " + range.Increment);
                            // Lets create a slider!
                            CreateSlider(f, range.Low, range.High, (float)f.GetValue(m.settings), range.Increment);
                            break;
                        }
                    }
                    if (show && f.FieldType.Equals(typeof(bool)))
                    {
                        Debug.Log("Showing bool with name: " + Util.GetName(f));
                        // Let's create a toggle!
                        CreateToggle(f, (bool)f.GetValue(m.settings));
                    }
                }
                Debug.Log("==========================================");
            }
        }
        public override void Show(params object[] parameters)
        {
            Debug.Log("Showing ModSettings Panel!");
            optionsPanel.Show(parameters);
        }
        public override void Hide(bool instant = false)
        {
            Debug.Log("Hiding ModSettings Panel!");
            optionsPanel.Hide(instant);
        }
        public void CreateSlider(FieldInfo f, float low, float high, float current, float increment)
        {
            Debug.Log("Creating Slider with name: " + Util.GetName(f));
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlSlider>("masterVolSlider").gameObject;
            GameObject slider = (GameObject)GameObject.Instantiate(oldObj);
            slider.transform.parent = oldObj.transform.parent;
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
            slider.GetComponent<AgeTooltip>().Content = Util.GetName(f);
            Debug.Log("AgeTfm: " + sliderControl.AgeTransform.Get2DPosition());
            Debug.Log("Arm - Method: " + sliderControl.OnArmMethod + " GameObject: " + sliderControl.OnArmObject);
            Debug.Log("Drag - Method: " + sliderControl.OnDragMethod + " GameObject: " + sliderControl.OnDragObject);
            Debug.Log("Release - Method: " + sliderControl.OnReleaseMethod + " GameObject: " + sliderControl.OnReleaseObject);

            fields.Add(f);
        }
        public void CreateToggle(FieldInfo f, bool current)
        {
            Debug.Log("Creating Toggle with name: " + Util.GetName(f));
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlToggle>("vSyncToggle").gameObject;
            GameObject toggle = (GameObject)GameObject.Instantiate(oldObj);
            toggle.transform.parent = oldObj.transform.parent;
            Debug.Log("Parent: " + toggle.transform.parent);
            Debug.Log("Toggle components:");
            foreach (Component _ in toggle.GetComponents(typeof(Component)))
            {
                Debug.Log("- " + _);
            }
            AgeControlToggle toggleControl = toggle.GetComponent<AgeControlToggle>();
            toggleControl.State = current;
            toggle.GetComponent<AgeTooltip>().Content = Util.GetName(f);
            Debug.Log("AgeTfm: " + toggleControl.AgeTransform.Get2DPosition());
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
                    if (o.GetComponent<AgeTooltip>().Content == Util.GetName(f))
                    {
                        // This is the field that needs to be modified.
                        // It should be a bool, if the button is working as intended!
                        Debug.Log("Setting value of: " + Util.GetName(f) + " to: " + o.GetComponent<AgeControlToggle>().State + " in settings type: " + mod.settingsType + " in mod: " + mod.name);
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
                    if (o.GetComponent<AgeTooltip>().Content == Util.GetName(f))
                    {
                        // This is the field that needs to be modified.
                        // It should be a bool, if the button is working as intended!
                        Debug.Log("Setting value of: " + Util.GetName(f) + " to: " + o.GetComponent<AgeControlSlider>().CurrentValue + " in settings type: " + mod.settingsType + " in mod: " + mod.name);
                        f.SetValue(mod.settings, o.GetComponent<AgeControlSlider>().CurrentValue);
                        return;
                    }
                }
            }
            Debug.Log("A slider exists but it doesn't have a matching field in all mods!");
        }
    }
}
