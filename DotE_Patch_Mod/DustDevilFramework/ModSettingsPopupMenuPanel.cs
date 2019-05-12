using BepInEx.Configuration;
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
        private List<AgeControlToggle> toggles;
        private Dictionary<ScadMod, Dictionary<BepInExSettingsPacket, object>> DefaultWrappers;
        protected List<ScadMod> mods;
        protected List<ScadMod> modsToModify;
        internal List<BepInExSettingsPacket> VisibleWrappers;

        private bool saveSettings = false;

        private readonly float settingSpacing = 3;

        public ModSettingsPopupMenuPanel()
        {
            Debug.Log("Constructing a ModSettings Popup Menu Panel for a GameObject!");
            VisibleWrappers = new List<BepInExSettingsPacket>();
            modsToModify = new List<ScadMod>();
            sliders = new List<AgeControlSlider>();
            toggles = new List<AgeControlToggle>();
            DefaultWrappers = new Dictionary<ScadMod, Dictionary<BepInExSettingsPacket, object>>();
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
                foreach (BepInExSettingsPacket p in VisibleWrappers)
                {
                    try
                    {
                        if (p.Wrapper.GetType().Equals(typeof(ConfigWrapper<bool>)))
                        {
                            DefaultWrappers[m][p] = ((ConfigWrapper<bool>)p.Wrapper).Value;
                        } else if (p.Wrapper.GetType().Equals(typeof(int)))
                        {
                            DefaultWrappers[m][p] = ((ConfigWrapper<int>)p.Wrapper).Value;
                        }
                        else if (p.Wrapper.GetType().Equals(typeof(ConfigWrapper<double>)))
                        {
                            DefaultWrappers[m][p] = ((ConfigWrapper<double>)p.Wrapper).Value;
                        } else if (p.Wrapper.GetType().Equals(typeof(ConfigWrapper<float>)))
                        {
                            DefaultWrappers[m][p] = ((ConfigWrapper<float>)p.Wrapper).Value;
                        } else
                        {
                            // Defaults to string. Potentially dangerous!
                            DefaultWrappers[m][p] = ((ConfigWrapper<string>)p.Wrapper).Value;
                        }
                    } catch (ArgumentException)
                    {
                        // Writing to the wrong mod.settings
                        continue;
                    }
                }
            }
        }
        private void AddDefaultSetting(ScadMod m, BepInExSettingsPacket p, object current)
        {
            if (!DefaultWrappers.ContainsKey(m))
            {
                DefaultWrappers.Add(m, new Dictionary<BepInExSettingsPacket, object>());
            }
            DefaultWrappers[m].Add(p, current);
        }
        // This method creates all of the AgeControl items that correspond to mod settings
        // They are placed within the optionsPanel instance variable
        private void CreateSettings()
        {
            Debug.Log("Creating Settings!");
            foreach (ScadMod m in mods)
            {
                Debug.Log("Attempting to find ConfigFile for mod: " + m.name);
                ConfigFile file = Util.GetConfigFile(m);

                Debug.Log("Mod: " + m.name + " with: " + file.ConfigDefinitions.Count + " fields.");

                foreach (ConfigDefinition d in file.ConfigDefinitions)
                {
                    // If it exists in the config file, it is meant to be there.

                    //TODO DEBUG!
                    // NEED TO REDO ATTRIBUTES FOR RANGE HERE NOW THAT I REMOVED MOST OF THAT STUFF WITH BEPINEX
                    if (d.Section.Equals(DustDevil.IGNORED_SETTINGS_SECTION_NAME))
                    {
                        Debug.Log("Ignoring Definition: " + d.Key);
                        continue;
                    }
                    Debug.Log("Attempting Definition: " + d.Key);

                    //ConfigWrapper<object> wrapper = file.Wrap<object>(d);

                    //Debug.Log("Definition: " + d + " has value: " + wrapper.Value);

                    //if (wrapper.Value == null)
                    //{
                    //    Debug.Log("Wrapper value is null!");
                    //    continue;
                    //}

                    try
                    {
                        Debug.Log("Attempting with type: bool");
                        ConfigWrapper<bool> wrapper = file.Wrap<bool>(d);
                        Debug.Log("File path: " + file.ConfigFilePath);

                        bool throwAway = wrapper.Value;

                        var packet = new BepInExSettingsPacket(wrapper, wrapper.Definition.Key, typeof(bool));

                        Debug.Log("Showing bool with name: " + Util.GetName(m, packet));
                        CreateToggle(m, packet);
                        AddDefaultSetting(m, packet, throwAway);
                    } catch (Exception)
                    {
                        try
                        {
                            Debug.Log("Attempting with type: float");
                            ConfigWrapper<float> wrapper = file.Wrap<float>(d);

                            float throwAway = wrapper.Value;

                            var packet = new BepInExSettingsPacket(wrapper, wrapper.Definition.Key, typeof(float));

                            Debug.Log("Showing slider with name: " + Util.GetName(m, packet));

                            // Get SliderRange of this definition.
                            SliderRange sliderRange = new SliderRange();
                            try
                            {
                                Debug.Log("Attempting to find slider range for slider: " + d.Key);

                                // Attempt to get the lower and upper settings...
                                ConfigWrapper<float> lowWrapper = file.Wrap<float>(DustDevil.IGNORED_SETTINGS_SECTION_NAME, d.Key + "Min", "The minimum value for " + d.Key + ".", sliderRange.Min);
                                ConfigWrapper<float> highWrapper = file.Wrap<float>(DustDevil.IGNORED_SETTINGS_SECTION_NAME, d.Key + "Max", "The maximum value for " + d.Key + ".", sliderRange.Max);
                                sliderRange = new SliderRange(lowWrapper.Value, highWrapper.Value);
                                // Attempt to get the increment...
                                ConfigWrapper<float> incrementWrapper = file.Wrap<float>(DustDevil.IGNORED_SETTINGS_SECTION_NAME, d.Key + "Increment", "How much to increment " + d.Key + " by.", sliderRange.Increment);
                                sliderRange.SetIncrement(incrementWrapper.Value);
                            }
                            catch (Exception)
                            {
                                Debug.Log("Error when attempting to get a slider range object. Will use most recent functional slider range.");
                            }

                            Debug.Log("Creating a slider with SliderRange: " + sliderRange);
                            CreateSlider(m, packet, sliderRange);
                            AddDefaultSetting(m, packet, throwAway);
                        } catch (Exception)
                        {
                            Debug.Log("Skipping Item with Name: " + d.Key + " as it was unparseable for settings!");
                        }
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
                    Util.GetConfigFile(m).Save();
                    Util.GetConfigFile(m).Reload();
                }
                foreach (ScadMod m in modsToModify)
                {
                    if (m.EnabledWrapper.Value)
                    {
                        Debug.Log("Mod: " + m.BepinExPluginType.GetMethod("ToString").Invoke(m.BepinPluginReference, new object[0]) + " is being Loaded!");
                        m.BepinExPluginType.GetMethod("OnLoad").Invoke(m.BepinPluginReference, new object[0]);
                    } else
                    {
                        Debug.Log("Mod: " + m.BepinExPluginType.GetMethod("ToString").Invoke(m.BepinPluginReference, new object[0]) + " is being UnLoaded!");
                        m.BepinExPluginType.GetMethod("UnLoad").Invoke(m.BepinPluginReference, new object[0]);
                    }
                }
                foreach (ScadMod m in mods)
                {
                    Util.GetConfigFile(m).Save();
                    Util.GetConfigFile(m).Reload();
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
            foreach (ScadMod m in DefaultWrappers.Keys)
            {
                foreach (BepInExSettingsPacket p in DefaultWrappers[m].Keys)
                {
                    if (p.Wrapper.GetType().Equals(typeof(ConfigWrapper<bool>)))
                    {
                        ((ConfigWrapper<bool>)p.Wrapper).Value = (bool)DefaultWrappers[m][p];
                    }
                    else if (p.Wrapper.GetType().Equals(typeof(int)))
                    {
                        ((ConfigWrapper<int>)p.Wrapper).Value = (int)DefaultWrappers[m][p];
                    }
                    else if (p.Wrapper.GetType().Equals(typeof(ConfigWrapper<double>)))
                    {
                        ((ConfigWrapper<double>)p.Wrapper).Value = (double)DefaultWrappers[m][p];
                    }
                    else if (p.Wrapper.GetType().Equals(typeof(ConfigWrapper<float>)))
                    {
                        ((ConfigWrapper<float>)p.Wrapper).Value = (float)DefaultWrappers[m][p];
                    }
                    else
                    {
                        // Defaults to string. Potentially dangerous!
                        ((ConfigWrapper<string>)p.Wrapper).Value = (string)DefaultWrappers[m][p];
                    }
                }
            }
            Debug.Log("Reset Settings!");
            foreach (AgeControlSlider s in sliders)
            {
                ResetSlider(s);
            }
            Debug.Log("Reset sliders!");
            foreach (AgeControlToggle t in toggles)
            {
                ResetToggle(t);
            }
            Debug.Log("Reset toggles!");
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
        internal void CreateSlider(ScadMod m, BepInExSettingsPacket w, SliderRange range)
        {
            Debug.Log("Creating Slider with name: " + Util.GetName(m, w));
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlSlider>("masterVolSlider").transform.parent.parent.gameObject;
            GameObject sliderGroup = (GameObject)GameObject.Instantiate(oldObj);
            sliderGroup.name = Util.GetName(m, w);
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
            slider.gameObject.name = Util.GetName(m, w) + "_Slider";
            AgeControlSlider sliderControl = slider.GetComponent<AgeControlSlider>();
            Debug.Log("Created Slider Control!");

            //slider.GetComponent<AgeTooltip>().Content = Util.GetName(m, f);

            Transform label = sliderGroup.transform.FindChild("0-Title");
            label.gameObject.name = Util.GetName(m, w) + "_Label";
            label.GetComponent<AgePrimitiveLabel>().Text = Util.GetName(m, w);

            sliderGroup.GetComponent<AgeTransform>().Position = d.Get<AgeControlSlider>("masterVolSlider").transform.parent.parent.GetComponent<AgeTransform>().Position;
            sliderGroup.GetComponent<AgeTransform>().PixelMarginTop = VisibleWrappers.Count * (settingSpacing + sliderGroup.GetComponent<AgeTransform>().Height);
            Debug.Log("Setup Slider Location!");

            sliderControl.MinValue = range.Min;
            sliderControl.MaxValue = range.Max;
            sliderControl.Increment = range.Increment;
            if (w.Type.Equals(typeof(int)))
            {
                sliderControl.CurrentValue = ((ConfigWrapper<int>)w.Wrapper).Value;
            } else if (w.Type.Equals(typeof(float)))
            {
                sliderControl.CurrentValue = ((ConfigWrapper<float>)w.Wrapper).Value;
            } else if (w.Type.Equals(typeof(double)))
            {
                sliderControl.CurrentValue = (float)((ConfigWrapper<double>)w.Wrapper).Value;
            }
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


            VisibleWrappers.Add(w);

            sliders.Add(sliderControl);
        }
        internal void CreateToggle(ScadMod m, BepInExSettingsPacket w)
        {
            // I believe the Old Object's parent is a special display component that contains both text and the toggle button
            Debug.Log("Creating Toggle with name: " + Util.GetName(m, w));
            DynData<OptionsPanel> d = new DynData<OptionsPanel>(optionsPanel);
            GameObject oldObj = d.Get<AgeControlToggle>("vSyncToggle").transform.parent.gameObject;
            // oldObj is now the group object!

            GameObject toggleGroup = (GameObject)GameObject.Instantiate(oldObj);
            toggleGroup.name = Util.GetName(m, w);
            toggleGroup.transform.SetParent(modSettingsTable.transform);
            Debug.Log("Toggle Group Object Created!");

            Transform toggle = toggleGroup.transform.GetChild(2);
            toggle.gameObject.name = Util.GetName(m, w) + "_Toggle";
            AgeControlToggle toggleControl = toggle.GetComponent<AgeControlToggle>();
            toggleControl.State = ((ConfigWrapper<bool>)w.Wrapper).Value;
            toggle.GetComponent<AgeTooltip>().Content = Util.GetName(m, w);

            Transform label = toggleGroup.transform.GetChild(1);
            label.gameObject.name = Util.GetName(m, w) + "_Label";
            label.GetComponent<AgePrimitiveLabel>().Text = Util.GetName(m, w);

            toggleGroup.GetComponent<AgeTransform>().PixelMarginTop = VisibleWrappers.Count * (settingSpacing + toggleGroup.GetComponent<AgeTransform>().Height);
            Debug.Log("VisibleFields Count: " + VisibleWrappers.Count);
            toggleControl.OnSwitchMethod = "OnTogglePressed";
            toggleControl.OnSwitchObject = transform.gameObject;
            Debug.Log("Switch - Method: " + toggleControl.OnSwitchMethod + " GameObject: " + toggleControl.OnSwitchObject);


            VisibleWrappers.Add(w);
            toggles.Add(toggleControl);
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
                foreach (BepInExSettingsPacket p in VisibleWrappers)
                {
                    // We need to set the field that corresponds to this toggle to the toggle value
                    // We can cheat by checking the name of the AgeTooltip attatched to 'o'
                    if (o.GetComponent<AgeTooltip>().Content.Equals(Util.GetName(mod, p)))
                    {
                        // This is the field that needs to be modified.
                        // It should be a bool, if the button is working as intended!
                        Debug.Log("Setting value of: " + Util.GetName(mod, p) + " to: " + o.GetComponent<AgeControlToggle>().State + " in mod: " + mod.name);
                        bool prevEnabled = mod.EnabledWrapper.Value;
                        ((ConfigWrapper<bool>)p.Wrapper).Value = o.GetComponent<AgeControlToggle>().State;
                        if (prevEnabled != mod.EnabledWrapper.Value || Util.GetName(mod, p).EndsWith("- Enabled"))
                        {
                            Debug.Log("Changed Enabled State of: " + mod.name);
                            // Just to make sure... Not sure (entirely) why this is nescessary...
                            mod.EnabledWrapper.Value = o.GetComponent<AgeControlToggle>().State;
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
                foreach (BepInExSettingsPacket p in VisibleWrappers)
                {
                    if (o.transform.parent.parent.name.Equals(Util.GetName(mod, p)))
                    {
                        Debug.Log("Setting value of: " + Util.GetName(mod, p) + " to: " + o.CurrentValue + " in mod: " + mod.name);
                        try
                        {
                            ((ConfigWrapper<float>)p.Wrapper).Value = o.CurrentValue;
                        } catch (Exception)
                        {
                            try
                            {
                                ((ConfigWrapper<int>)p.Wrapper).Value = (int)o.CurrentValue;
                            }
                            catch (Exception)
                            {
                                ((ConfigWrapper<double>)p.Wrapper).Value = (double)o.CurrentValue;
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
                foreach (BepInExSettingsPacket p in VisibleWrappers)
                {
                    if (o.transform.parent.parent.name.Equals(Util.GetName(m, p)))
                    {
                        object q = DefaultWrappers[m][p];
                        Debug.Log("Reseting slider: " + o + " to: " + q);
                        try
                        {
                            o.CurrentValue = (float)q;
                        }
                        catch (InvalidCastException)
                        {
                            try
                            {
                                o.CurrentValue = (int)q;
                            }
                            catch (InvalidCastException)
                            {
                                o.CurrentValue = (float)(double)q;
                            }
                        }
                        o.transform.parent.parent.FindChild("30-Value").GetComponent<AgePrimitiveLabel>().Text = o.CurrentValue.ToString();
                    }
                }
            }
        }
        private void ResetToggle(AgeControlToggle t)
        {
            foreach (ScadMod m in mods)
            {
                foreach (BepInExSettingsPacket p in VisibleWrappers)
                {
                    if (t.name.Equals(Util.GetName(m, p) + "_Toggle"))
                    {
                        t.State = (bool)DefaultWrappers[m][p];
                    }
                }
            }
        }
    }
}
