using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DustDevilFramework
{
    // Really, I should make a CustomPopupMenuPanel class and use that for everything
    // This is like a MonoBehavior or a Component, so it needs to be added to a GameObject to properly function!
    public class ModSettingsPopupMenuPanel : PopupMenuPanel
    {
        private OptionsPanel optionsPanel;
        private GameObject frame;
        private AgeTransform modSettingsTable;
        private GameObject modSettingsScroll;
        private List<AgeControlSlider> sliders;
        private Dictionary<ScadMod, Dictionary<FieldInfo, object>> defaultFields;
        List<ScadMod> mods;
        List<ScadMod> modsToModify;
        List<FieldInfo> VisibleFields;

        private bool saveSettings = false;

        private readonly float settingSpacing = 3;

        public ModSettingsPopupMenuPanel()
        {
            Debug.Log("Constructing a ModSettings Popup Menu Panel for a GameObject!");
            VisibleFields = new List<FieldInfo>();
            modsToModify = new List<ScadMod>();
            sliders = new List<AgeControlSlider>();
            defaultFields = new Dictionary<ScadMod, Dictionary<FieldInfo, object>>();
            Debug.Log("Construction of Component for GO Complete!");
        }
        private void SetupModlist(List<ScadMod> m)
        {
            Debug.Log("Setting up ModList!");
            mods = m;
            Debug.Log("Creating Frame!");
            CreateFrame();
            Debug.Log("Creating ScrollBar!");
            CreateScrollArea();
            Debug.Log("Creating ModSettingsTable!");
            CreateModSettingsTable();
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

            GameObject o = (GameObject)GameObject.Instantiate(obj);
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
            instanceForGO.transform.SetParent(obj.transform.parent);
            instanceForGO.transform.GetComponent<AgeTransform>().Position = old.AgeTransform.Position;

            instanceForGO.SetupOptionsPanel(old);
            instanceForGO.SetupModlist(m);

            return instanceForGO;
        }
        protected override void Awake()
        {
            base.Awake();
            saveSettings = false;
            Hide();
        }
        private void ResetDefaultSettings()
        {
            Debug.Log("Setting up DefaultSettings Dict!");
            foreach (ScadMod m in mods)
            {
                foreach (FieldInfo f in VisibleFields)
                {
                    //if (!defaultFields.ContainsKey(m))
                    //{
                    //    defaultFields.Add(m, new Dictionary<FieldInfo, object>());
                    //}
                    //if (!defaultFields[m].ContainsKey(f))
                    //{
                    //    defaultFields[m].Add(f, f.GetValue(m.settings));
                    //    continue;
                    //}
                    try
                    {
                        defaultFields[m][f] = f.GetValue(m.settings);
                    } catch (ArgumentException e)
                    {
                        // Writing to the wrong mod.settings
                        continue;
                    }
                }
            }
        }
        private void AddDefaultSetting(ScadMod m, FieldInfo f, object current)
        {
            if (!defaultFields.ContainsKey(m))
            {
                defaultFields.Add(m, new Dictionary<FieldInfo, object>());
            }
            defaultFields[m].Add(f, current);
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

                    fields.AddRange(basicType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
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

                    object[] allCustomAttributes = f.GetCustomAttributes(true);
                    //object[] allCustomAttributes = new object[0];
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
                            try
                            {
                                Debug.Log("Trying as a float...");
                                float v = (float)f.GetValue(m.settings);
                                CreateSlider(m, f, range.Low, range.High, v, range.Increment);
                                AddDefaultSetting(m, f, v);
                            } catch (Exception _)
                            {
                                Debug.Log("Trying as an int...");
                                try
                                {
                                    int v = (int)f.GetValue(m.settings);
                                    CreateSlider(m, f, range.Low, range.High, v, range.Increment);
                                    AddDefaultSetting(m, f, v);
                                } catch (Exception __)
                                {
                                    Debug.Log("Trying as a double...");
                                    double v = (double)f.GetValue(m.settings);
                                    CreateSlider(m, f, range.Low, range.High, (float)v, range.Increment);
                                    AddDefaultSetting(m, f, v);
                                }
                            }
                            break;
                        }
                    }
                    if (show && f.FieldType.Equals(typeof(bool)))
                    {
                        Debug.Log("Showing bool with name: " + Util.GetName(m, f));
                        // Let's create a toggle!
                        bool v = (bool)f.GetValue(m.settings);
                        CreateToggle(m, f, v);
                        AddDefaultSetting(m, f, v);
                    }
                }
                Debug.Log("==========================================");
            }
        }
        public override void RefreshContent()
        {
            base.RefreshContent();
            modSettingsScroll.GetComponent<AgeControlScrollView>().OnPositionRecomputed();

        }
        protected override IEnumerator OnLoad()
        {
            UseRefreshLoop = true;
            modSettingsTable.Height = 0f;
            modSettingsTable.Y = 0;
            modSettingsTable.Width = 612;
            // I need to make a prefab that is usable for all types of visible settings
            // It should be a lot like the toggle prefab, but it needs to not have the toggle in it because sliders are a thing

            //Transform PREFAB_FOR_SETTINGS = new DynData<OptionsPanel>(optionsPanel).Get<Transform>("controlBindingLinePrefab");
            //modSettingsTable.ReserveChildren(VisibleFields.Count, PREFAB_FOR_SETTINGS, "ModSettings");
            //modSettingsTable.RefreshChildrenIList();
            modSettingsTable.ArrangeChildren();
            return base.OnLoad();
        }
        public override void Show(params object[] parameters)
        {
            Debug.Log("Showing ModSettings Panel!");
            ResetDefaultSettings();
            foreach (AgeControlSlider s in sliders)
            {
                ResetSlider(s);
            }

            base.Show(parameters);
            NeedRefresh = true;
        }
        protected override IEnumerator OnShow(params object[] parameters)
        {
            Debug.Log("OnShow!");
            saveSettings = false;
            modsToModify = new List<ScadMod>();
            yield return base.OnShow(parameters);
            Debug.Log("AFTER SHOW!");
            yield break;
        }
        public override void Hide(bool instant = false)
        {
            Debug.Log("Hiding ModSettings Panel!");
            if (saveSettings)
            {
                Debug.Log("Attempting to write settings to files!");
                foreach (AgeControlSlider slider in sliders)
                {
                    // Check all of the sliders
                    UpdateSlider(slider);
                }
                foreach (ScadMod m in mods)
                {
                    m.settings.WriteSettings();
                }
                foreach (ScadMod m in modsToModify)
                {
                    if (m.settings.Enabled)
                    {
                        Debug.Log("Mod: " + m.PartialityModType.GetMethod("ToString").Invoke(m.PartialityModReference, new object[0]) + " is being Loaded!");
                        m.PartialityModType.GetMethod("OnLoad").Invoke(m.PartialityModReference, new object[0]);
                    } else
                    {
                        Debug.Log("Mod: " + m.PartialityModType.GetMethod("ToString").Invoke(m.PartialityModReference, new object[0]) + " is being UnLoaded!");
                        m.PartialityModType.GetMethod("UnLoad").Invoke(m.PartialityModReference, new object[0]);
                    }
                }
                Debug.Log("Wrote settings to files!");
            } else
            {
                OnResetButtonClick();
            }
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
            saveSettings = false;
            Hide();
        }
        private void OnResetButtonClick()
        {
            //TODO Add!
            Debug.Log("Resetting Settings!");
            foreach (ScadMod m in defaultFields.Keys)
            {
                foreach (FieldInfo f in defaultFields[m].Keys)
                {
                    f.SetValue(m.settings, defaultFields[m][f]);
                }
            }
            Debug.Log("Reset Settings!");
            foreach (AgeControlSlider s in sliders)
            {
                ResetSlider(s);
            }
            Debug.Log("Reset sliders!");
            //requesterPanel.Display("%ResetControlsConfirmMessage", new global::RequesterPanel.ResultEventHandler(this.OnSettingsResetConfrim), global::RequesterPanel.ButtonsMode.YesNo, "%ResetControlsConfirmTitle", -1f, true);
        }
        private void OnConfirmButtonClick(GameObject obj)
        {
            saveSettings = true;
            Hide();
        }
        public void CreateFrame()
        {
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlToggle>("vSyncToggle").transform.parent.parent.parent.parent.gameObject;
            // oldObj in this case is the "Frame" object that we would like to duplicate.
            GameObject frame = (GameObject)GameObject.Instantiate(oldObj);
            // The frame is a child of the current display.
            frame.transform.SetParent(transform);
            frame.name = "ModSettingsFrame";
            Rect te = optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>().Position;
            frame.GetComponent<AgeTransform>().Position = te;
            frame.GetComponent<AgeTransform>().PixelOffsetLeft = optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>().PixelOffsetLeft;
            frame.GetComponent<AgeTransform>().PixelOffsetTop = optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>().PixelOffsetTop;

            // Delete the Frame from the GUI control, there can only be one frame (otherwise weird things happen)
            GameObject.DestroyImmediate(transform.FindChild("1-Frame").gameObject);
            GameObject bg = (GameObject)GameObject.Instantiate(optionsPanel.transform.FindChild("0-Bg").gameObject);

            bg.transform.SetParent(transform);
            //transform.SetParent(optionsPanel.AgeTransform.GetParent().transform);

            bg.name = "ModSettingsBackground";

            // Destroys the useless RightSide
            // Instead, we will expand the leftSide to have everything!
            // We need to delete all of its children too, and its children's children, etc.

            frame.transform.FindChild("2-LeftPart").name = "CentralPart";

            Util.DeleteChildrenExclusive(frame.transform.FindChild("CentralPart").gameObject);
            AgeTransform left = frame.transform.FindChild("CentralPart").GetComponent<AgeTransform>();
            left.Init();
            left.Position = optionsPanel.transform.FindChild("1-Frame").FindChild("2-LeftPart").GetComponent<AgeTransform>().Position;
            left.Width = frame.GetComponent<AgeTransform>().Width;
            left.PixelMarginTop = 20;
            left.Height = left.Height + (optionsPanel.transform.FindChild("1-Frame").FindChild("2-LeftPart").GetComponent<AgeTransform>().PixelMarginTop - left.PixelMarginTop);
            left.Height -= 10;

            Util.DeleteChildrenInclusive(frame.transform.FindChild("3-RightPart").gameObject);

            //GameObject.DestroyImmediate(frame.transform.FindChild("3-RightPart"));
            AgeControlButton cancel = frame.transform.FindChild("7-CancelButton").GetComponent<AgeControlButton>();
            cancel.AgeTransform.Position = optionsPanel.transform.FindChild("1-Frame").FindChild("7-CancelButton").GetComponent<AgeControlButton>().AgeTransform.Position; // Fixes a strange bug
            AgeControlButton confirm = frame.transform.FindChild("9-ConfirmButton").GetComponent<AgeControlButton>();
            confirm.AgeTransform.Position = optionsPanel.transform.FindChild("1-Frame").FindChild("9-ConfirmButton").GetComponent<AgeControlButton>().AgeTransform.Position; // Fixes a strange bug
            AgeControlButton reset = frame.transform.FindChild("8-ResetButton").GetComponent<AgeControlButton>();
            reset.AgeTransform.Position = optionsPanel.transform.FindChild("1-Frame").FindChild("8-ResetButton").GetComponent<AgeControlButton>().AgeTransform.Position; // Fixes a strange bug

            // Sets the buttons to match the OptionsPanel buttons
            cancel.AgeTransform.PixelMarginBottom = 3;
            cancel.AgeTransform.PixelMarginLeft = 3;
            cancel.AgeTransform.PixelMarginRight = 4;

            confirm.AgeTransform.PixelMarginBottom = 3;
            confirm.AgeTransform.PixelMarginLeft = 4;
            confirm.AgeTransform.PixelMarginRight = 3;

            reset.AgeTransform.PixelMarginBottom = 3;
            reset.AgeTransform.PixelMarginLeft = 3;
            reset.AgeTransform.PixelMarginRight = 4;
            reset.AgeTransform.PixelOffsetLeft = 309;
            reset.transform.FindChild("2-Label").GetComponent<AgePrimitiveLabel>().Text = "Reset Settings";

            cancel.OnActivateObject = gameObject;
            confirm.OnActivateObject = gameObject;
            reset.OnActivateObject = gameObject;

            this.frame = frame;
        }
        public void CreateModSettingsTable()
        {
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject old = d.Get<AgeTransform>("controlBindingsTable").gameObject;

            GameObject table = (GameObject)GameObject.Instantiate(old);
            table.name = "ModSettingsTable";
            table.transform.SetParent(modSettingsScroll.GetComponent<AgeControlScrollView>().Viewport.transform);

            AgeTransform oldTable = old.GetComponent<AgeTransform>();
            AgeTransform newTable = table.GetComponent<AgeTransform>();

            modSettingsScroll.GetComponent<AgeControlScrollView>().VirtualArea = newTable;

            newTable.Init();
            newTable.Position = oldTable.Position;
            newTable.VerticalSpacing = oldTable.VerticalSpacing;

            modSettingsTable = newTable;
        }
        public void CreateSlider(ScadMod m, FieldInfo f, float low, float high, float current, float increment)
        {
            Debug.Log("Creating Slider with name: " + Util.GetName(m, f));
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlSlider>("masterVolSlider").transform.parent.parent.gameObject;
            GameObject sliderGroup = (GameObject)GameObject.Instantiate(oldObj);
            sliderGroup.name = Util.GetName(m, f);
            sliderGroup.transform.SetParent(modSettingsTable.transform);
            Debug.Log("Slider Group Object Created!");

            Debug.Log("OLD:");
            Debug.Log("Slider: " + d.Get<AgeControlSlider>("masterVolSlider").transform);
            Debug.Log("Parent: " + d.Get<AgeControlSlider>("masterVolSlider").transform.parent);
            Debug.Log("Parent's Parent: " + d.Get<AgeControlSlider>("masterVolSlider").transform.parent.parent);
            Debug.Log("Children");
            foreach (Transform t in d.Get<AgeControlSlider>("masterVolSlider").transform.parent.parent)
            {
                Debug.Log("Components:");
                foreach (Component _ in t.GetComponents(typeof(Component)))
                {
                    Debug.Log("- " + _);
                }
            }
            Transform slider = sliderGroup.transform.FindChild("20-SliderContainer").FindChild("10-Slider");
            slider.gameObject.name = Util.GetName(m, f) + "_Slider";
            AgeControlSlider sliderControl = slider.GetComponent<AgeControlSlider>();
            Debug.Log("Created Slider Control!");

            //slider.GetComponent<AgeTooltip>().Content = Util.GetName(m, f);

            Transform label = sliderGroup.transform.FindChild("0-Title");
            label.gameObject.name = Util.GetName(m, f) + "_Label";
            label.GetComponent<AgePrimitiveLabel>().Text = Util.GetName(m, f);

            sliderGroup.GetComponent<AgeTransform>().Position = d.Get<AgeControlSlider>("masterVolSlider").transform.parent.parent.GetComponent<AgeTransform>().Position;
            sliderGroup.GetComponent<AgeTransform>().PixelMarginTop = VisibleFields.Count * (settingSpacing + sliderGroup.GetComponent<AgeTransform>().Height);
            Debug.Log("Setup Slider Location!");

            sliderControl.MinValue = low;
            sliderControl.MaxValue = high;
            sliderControl.Increment = increment;
            sliderControl.CurrentValue = current;
            Debug.Log("Setup Slider Control!");

            sliderControl.AgeTransform.Position = d.Get<AgeControlSlider>("masterVolSlider").AgeTransform.Position;
            // Set to have same stats as old sliders
            sliderGroup.transform.FindChild("20-SliderContainer").GetComponent<AgeTransform>().PixelMarginLeft = 36;
            sliderGroup.transform.FindChild("20-SliderContainer").GetComponent<AgeTransform>().PixelMarginRight = 36;
            sliderGroup.transform.FindChild("20-SliderContainer").GetComponent<AgeTransform>().PixelMarginBottom = 6;
            //sliderGroup.transform.FindChild("20-SliderContainer").GetComponent<AgeTransform>().Width = 612;
            sliderGroup.transform.FindChild("10-SliderBg").GetComponent<AgeTransform>().PixelMarginBottom = 6;
            sliderGroup.transform.FindChild("30-Value").GetComponent<AgePrimitiveLabel>().Text = sliderControl.CurrentValue.ToString();
            label.GetComponent<AgePrimitiveLabel>().AgeTransform.Height = 42;
            label.GetComponent<AgePrimitiveLabel>().AgeTransform.PixelMarginLeft = 8;
            sliderControl.AgeTransform.GetParent().X = 36;
            sliderControl.AgeTransform.GetParent().Y = 36;
            sliderControl.AgeTransform.GetParent().Width = d.Get<AgeControlSlider>("masterVolSlider").AgeTransform.GetParent().Width;
            sliderGroup.transform.FindChild("30-Value").GetComponent<AgeTransform>().Height = 42;


            sliderControl.OnDragMethod = "OnSliderDragged";
            sliderControl.OnDragObject = transform.gameObject;
            Debug.Log("Arm - Method: " + sliderControl.OnArmMethod + " GameObject: " + sliderControl.OnArmObject);
            Debug.Log("Drag - Method: " + sliderControl.OnDragMethod + " GameObject: " + sliderControl.OnDragObject);
            Debug.Log("Release - Method: " + sliderControl.OnReleaseMethod + " GameObject: " + sliderControl.OnReleaseObject);


            VisibleFields.Add(f);

            sliders.Add(sliderControl);
        }
        public void CreateToggle(ScadMod m, FieldInfo f, bool current)
        {
            // I believe the Old Object's parent is a special display component that contains both text and the toggle button
            Debug.Log("Creating Toggle with name: " + Util.GetName(m, f));
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlToggle>("vSyncToggle").transform.parent.gameObject;
            // oldObj is now the group object!

            GameObject toggleGroup = (GameObject)GameObject.Instantiate(oldObj);
            toggleGroup.name = Util.GetName(m, f);
            toggleGroup.transform.SetParent(modSettingsTable.transform);
            Debug.Log("Toggle Group Object Created!");

            Transform toggle = toggleGroup.transform.GetChild(2);
            toggle.gameObject.name = Util.GetName(m,f) + "_Toggle";
            AgeControlToggle toggleControl = toggle.GetComponent<AgeControlToggle>();
            toggleControl.State = current;
            toggle.GetComponent<AgeTooltip>().Content = Util.GetName(m, f);

            Transform label = toggleGroup.transform.GetChild(1);
            label.gameObject.name = Util.GetName(m, f) + "_Label";
            label.GetComponent<AgePrimitiveLabel>().Text = Util.GetName(m, f);

            toggleGroup.GetComponent<AgeTransform>().PixelMarginTop = VisibleFields.Count * (settingSpacing + toggleGroup.GetComponent<AgeTransform>().Height);
            Debug.Log("VisibleFields Count: " + VisibleFields.Count);
            toggleControl.OnSwitchMethod = "OnTogglePressed";
            toggleControl.OnSwitchObject = transform.gameObject;
            Debug.Log("Switch - Method: " + toggleControl.OnSwitchMethod + " GameObject: " + toggleControl.OnSwitchObject);


            VisibleFields.Add(f);
        }
        public void CreateScrollArea()
        {
            AgeControlScrollView scroll = new DynData<OptionsPanel>(optionsPanel).Get<AgeControlScrollView>("controlBindingsScrollView");
            // Clones the parent of the scroll (1-ControlsConfig)
            // ScrollBar is child: 2-ControlBindingsScrollView
            GameObject o = (GameObject)GameObject.Instantiate(scroll.gameObject);
            o.transform.SetParent(frame.transform.FindChild("CentralPart"));
            o.SetActive(true);

            AgeControlScrollView newScroll = o.GetComponent<AgeControlScrollView>();
            o.name = "ModSettingsScrollView";
            newScroll.Init();
            newScroll.AgeTransform.Position = scroll.AgeTransform.Position;
            newScroll.AgeTransform.PixelMarginTop = scroll.AgeTransform.PixelMarginTop;
            newScroll.AgeTransform.PixelMarginLeft = scroll.AgeTransform.PixelMarginLeft;
            newScroll.AgeTransform.PixelMarginRight = scroll.AgeTransform.PixelMarginRight;

            o.transform.FindChild("3Viewport").name = "ModSettingsViewPort";
            o.transform.FindChild("1VerticalScrollBar").name = "ModSettingsScrollBar";
            newScroll.Viewport = o.transform.FindChild("ModSettingsViewPort").GetComponent<AgeTransform>();
            newScroll.Viewport.Init();
            newScroll.Viewport.Position = scroll.Viewport.Position;
            Util.DeleteChildrenInclusive(newScroll.Viewport.transform.FindChild("2-ControlBindingsTable").gameObject);
            newScroll.VerticalScrollBar = o.transform.FindChild("ModSettingsScrollBar").GetComponent<AgeControlScrollBar>();
            newScroll.VerticalScrollBar.Init();
            newScroll.VerticalScrollBar.AgeTransform.Position = scroll.VerticalScrollBar.AgeTransform.Position;
            newScroll.DisplayVertical = AgeScrollbarDisplay.ALWAYS;

            modSettingsScroll = o;
        }
        private void OnTogglePressed(GameObject o)
        {
            Debug.Log("Registered a Toggle Press! GO: " + o);
            foreach (ScadMod mod in mods)
            {
                foreach (FieldInfo f in VisibleFields)
                {
                    // We need to set the field that corresponds to this toggle to the toggle value
                    // We can cheat by checking the name of the AgeTooltip attatched to 'o'
                    if (o.GetComponent<AgeTooltip>().Content.Equals(Util.GetName(mod, f)))
                    {
                        // This is the field that needs to be modified.
                        // It should be a bool, if the button is working as intended!
                        Debug.Log("Setting value of: " + Util.GetName(mod, f) + " to: " + o.GetComponent<AgeControlToggle>().State + " in settings type: " + mod.settingsType + " in mod: " + mod.name);
                        bool prevEnabled = mod.settings.Enabled;
                        f.SetValue(mod.settings, o.GetComponent<AgeControlToggle>().State);
                        if (prevEnabled != mod.settings.Enabled)
                        {
                            modsToModify.Add(mod);
                        }
                        return;
                    }
                }
            }
            Debug.Log("A toggle button exists but it doesn't have a matching field in all mods!");
        }
        private void UpdateSlider(AgeControlSlider o)
        {
            foreach (ScadMod mod in mods)
            {
                foreach (FieldInfo f in VisibleFields)
                {
                    if (o.transform.parent.parent.name.Equals(Util.GetName(mod, f)))
                    {
                        Debug.Log("Setting value of: " + Util.GetName(mod, f) + " to: " + o.CurrentValue + " in settings type: " + mod.settingsType + " in mod: " + mod.name);
                        try
                        {
                            f.SetValue(mod.settings, o.CurrentValue);
                        } catch (Exception e)
                        {
                            try
                            {
                                f.SetValue(mod.settings, (int)o.CurrentValue);
                            }
                            catch (Exception e2)
                            {
                                f.SetValue(mod.settings, (double)o.CurrentValue);
                            }
                        } 
                        return;
                    }
                }
            }
            Debug.Log("A slider exists but it doesn't have a matching field in all mods!");
        }
        private void OnSliderDragged()
        {
            // Find the slider that was dragged and update it.
            foreach (AgeControlSlider s in sliders)
            {
                AgePrimitiveLabel valueLabel = s.transform.parent.parent.FindChild("30-Value").GetComponent<AgePrimitiveLabel>();
                if (!s.CurrentValue.ToString().Equals(valueLabel.Text))
                {
                    valueLabel.Text = s.CurrentValue.ToString();
                }
            }
        }
        private void ResetSlider(AgeControlSlider o)
        {
            foreach (ScadMod m in mods)
            {
                foreach (FieldInfo f in VisibleFields)
                {
                    if (o.transform.parent.parent.name.Equals(Util.GetName(m, f)))
                    {
                        object q = defaultFields[m][f];
                        Debug.Log("Reseting slider: " + o + " to: " + q);
                        try
                        {
                            o.CurrentValue = (float)q;
                        }
                        catch (InvalidCastException e)
                        {
                            try
                            {
                                o.CurrentValue = (int)q;
                            }
                            catch (InvalidCastException e2)
                            {
                                o.CurrentValue = (float)(double)q;
                            }
                        }
                        o.transform.parent.parent.FindChild("30-Value").GetComponent<AgePrimitiveLabel>().Text = o.CurrentValue.ToString();
                    }
                }
            }
        }
    }
}
