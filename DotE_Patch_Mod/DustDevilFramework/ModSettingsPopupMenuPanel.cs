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
        private AgeTransform modSettingsTable;
        private GameObject modSettingsScroll;
        List<ScadMod> mods;
        List<ScadMod> modsToModify;
        List<FieldInfo> Fields;
        List<FieldInfo> VisibleFields;

        private bool saveSettings = false;

        private readonly float settingSpacing = 3;

        public ModSettingsPopupMenuPanel()
        {
            Debug.Log("Constructing a ModSettings Popup Menu Panel for a GameObject!");
            Fields = new List<FieldInfo>();
            VisibleFields = new List<FieldInfo>();
            modsToModify = new List<ScadMod>();
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
            modSettingsScroll.GetComponent<AgeControlScrollView>().OnPositionRecomputed();
        }
        protected override IEnumerator OnLoad()
        {
            UseRefreshLoop = true;
            modSettingsTable.Height = 0f;
            modSettingsTable.Y = 0;
            modSettingsTable.Width = 612;
            //modSettingsTable.ChildWidth = 612;
            // I need to make a prefab that is usable for all types of visible settings
            // It should be a lot like the toggle prefab, but it needs to not have the toggle in it because sliders are a thing
            Transform PREFAB_FOR_SETTINGS = new DynData<OptionsPanel>(optionsPanel).Get<Transform>("controlBindingLinePrefab");
            modSettingsTable.ReserveChildren(VisibleFields.Count, PREFAB_FOR_SETTINGS, "ModSettings");
            //modSettingsTable.RefreshChildrenIList();
            modSettingsTable.ArrangeChildren();
            return base.OnLoad();
        }
        public override void Show(params object[] parameters)
        {
            Debug.Log("Showing ModSettings Panel!");

            Util.LogVariousAgeTransformInfo(frame.transform.FindChild("2-LeftPart").GetComponent<AgeTransform>());
            Util.LogVariousAgeTransformInfo(optionsPanel.transform.FindChild("1-Frame").FindChild("2-LeftPart").GetComponent<AgeTransform>());
            Util.LogVariousAgeTransformInfo(modSettingsTable);
            Util.LogVariousAgeTransformInfo(new DynData<OptionsPanel>(optionsPanel).Get<AgeTransform>("controlBindingsTable"));
            Util.LogVariousAgeTransformInfo(modSettingsScroll.transform.FindChild("ModSettingsViewPort").GetComponent<AgeTransform>());
            Util.LogVariousAgeTransformInfo(new DynData<OptionsPanel>(optionsPanel).Get<AgeControlScrollView>("controlBindingsScrollView").transform.FindChild("3Viewport").GetComponent<AgeTransform>());
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

            GameObject modSettingsConfig = (GameObject)GameObject.Instantiate(optionsPanel.transform.FindChild("1-Frame").FindChild("3-RightPart").FindChild("2-ResolutionConfig").gameObject);

            bg.transform.SetParent(transform);
            //transform.SetParent(optionsPanel.AgeTransform.GetParent().transform);

            bg.name = "ModSettingsBackground";
            modSettingsConfig.name = "ModSettingsConfig";

            // Destroys the useless LeftSide (which holds the controls)
            // Instead, we could put stuff like labels there, or something
            // We need to delete all of its children too, and its children's children, etc.

            Util.DeleteChildrenExclusive(frame.transform.FindChild("2-LeftPart").gameObject);
            AgeTransform left = frame.transform.FindChild("2-LeftPart").GetComponent<AgeTransform>();
            left.Init();
            left.Position = optionsPanel.transform.FindChild("1-Frame").FindChild("2-LeftPart").GetComponent<AgeTransform>().Position;
            left.PixelMarginTop = optionsPanel.transform.FindChild("1-Frame").FindChild("2-LeftPart").GetComponent<AgeTransform>().PixelMarginTop;

            Util.DeleteChildrenExclusive(frame.transform.FindChild("3-RightPart").gameObject);
            Util.DeleteChildrenExclusive(modSettingsConfig);

            modSettingsConfig.transform.SetParent(frame.transform.FindChild("3-RightPart"));
            modSettingsConfig.GetComponent<AgeTransform>().Position = optionsPanel.transform.FindChild("1-Frame").FindChild("3-RightPart").FindChild("2-ResolutionConfig").GetComponent<AgeTransform>().Position;

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

            Debug.Log("Old ControlBindingsTable Parent: " + old.transform.parent);

            newTable.Init();
            newTable.Position = oldTable.Position;
            newTable.Y -= 42;
            newTable.VerticalSpacing = oldTable.VerticalSpacing;

            modSettingsTable = newTable;
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

            Fields.Add(f);
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
            //toggleGroup.transform.SetParent(frame.transform.FindChild("3-RightPart").FindChild("ModSettingsConfig"));
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
            toggleGroup.GetComponent<AgeTransform>().Position = oldObj.GetComponent<AgeTransform>().Position;
            toggle.GetComponent<AgeTransform>().Position = oldObj.transform.GetChild(2).GetComponent<AgeTransform>().Position;
            //toggleGroup.GetComponent<AgeTransform>().PixelMarginTop = VisibleFields.Count * (settingSpacing + toggleGroup.GetComponent<AgeTransform>().Height);
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
            o.transform.SetParent(frame.transform.FindChild("2-LeftPart"));
            o.SetActive(true);
            Debug.Log("Children of O:");
            foreach (Transform t in o.transform)
            {
                Debug.Log("- " + t);
            }
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

            AgeControlScrollView newScroll = o.GetComponent<AgeControlScrollView>();
            o.name = "ModSettingsScrollView";
            newScroll.Init();
            newScroll.AgeTransform.Position = scroll.AgeTransform.Position;
            newScroll.AgeTransform.PixelMarginTop = scroll.AgeTransform.PixelMarginTop;
            newScroll.AgeTransform.PixelMarginLeft = scroll.AgeTransform.PixelMarginLeft;
            newScroll.AgeTransform.PixelMarginRight = scroll.AgeTransform.PixelMarginRight;

            Debug.Log("New VirtualArea Parent: " + newScroll.VirtualArea.transform.parent);
            Debug.Log("Old VirtualArea Parent: " + scroll.VirtualArea.transform.parent);
            Debug.Log("New VirtualArea PP: " + newScroll.VirtualArea.transform.parent.parent);
            Debug.Log("Old VirtualArea PP: " + scroll.VirtualArea.transform.parent.parent);

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

            Util.LogVariousAgeTransformInfo(newScroll.AgeTransform);
            Util.LogVariousAgeTransformInfo(scroll.AgeTransform);

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
                    if (o.GetComponent<AgeTooltip>().Content == Util.GetName(mod, f))
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
        private void OnSliderDragged(GameObject o)
        {
            Debug.Log("Registered a Drag Event Press! GO: " + o);
            foreach (ScadMod mod in mods)
            {
                foreach (FieldInfo f in VisibleFields)
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
