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
        List<FieldInfo> Fields;
        List<FieldInfo> VisibleFields;

        private bool saveSettings = false;

        private readonly float settingSpacing = 3;

        public ModSettingsPopupMenuPanel()
        {
            Debug.Log("Constructing a ModSettings Popup Menu Panel for a GameObject!");
            Fields = new List<FieldInfo>();
            VisibleFields = new List<FieldInfo>();
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
        private void LogVariousAgeTransformInfo(AgeTransform trans)
        {
            Debug.Log("========================================== LOG FOR: " + trans.name + " ==========================================");
            Debug.Log("Anchored: " + trans.Anchored);
            Debug.Log("Attach Bottom: " + trans.AttachBottom);
            Debug.Log("Attach Left: " + trans.AttachLeft);
            Debug.Log("Attach Right: " + trans.AttachRight);
            Debug.Log("Attach Top: " + trans.AttachTop);
            Debug.Log("AutoResizeHeight: " + trans.AutoResizeHeight);
            Debug.Log("AutoResizeWidth: " + trans.AutoResizeWidth);
            Debug.Log("ChildHeight: " + trans.ChildHeight);
            Debug.Log("ChildWidth: " + trans.ChildWidth);
            Debug.Log("ChildComparer: " + trans.ChildrenComparer);
            Debug.Log("FixedSize: " + trans.FixedSize);
            trans.ComputeGlobalPosition(out Rect temp);
            Debug.Log("GlobalPosition: " + temp);
            Debug.Log("DirtyPosition: " + trans.DirtyPosition);
            Debug.Log("RenderedPosition: " + trans.GetRenderedPosition());
            Debug.Log("HasModifiers: " + trans.HasModifiers);
            Debug.Log("Height: " + trans.Height);
            Debug.Log("HorizontalMargin: " + trans.HorizontalMargin);
            Debug.Log("HorizontalSpacing: " + trans.HorizontalSpacing);
            Debug.Log("ModifiersRunning: " + trans.ModifiersRunning);
            Debug.Log("NoOverroll: " + trans.NoOverroll);
            Debug.Log("Percent Bottom: " + trans.PercentBottom);
            Debug.Log("Percent Left: " + trans.PercentLeft);
            Debug.Log("Percent Right: " + trans.PercentRight);
            Debug.Log("Percent Top: " + trans.PercentTop);
            Debug.Log("PivotMode: " + trans.PivotMode);
            Debug.Log("PivotOffset: " + trans.PivotOffset);
            Debug.Log("PixelMarginBottom: " + trans.PixelMarginBottom);
            Debug.Log("PixelMarginLeft: " + trans.PixelMarginLeft);
            Debug.Log("PixelMarginRight: " + trans.PixelMarginRight);
            Debug.Log("PixelMarginTop: " + trans.PixelMarginTop);
            Debug.Log("PixelOffsetBottom: " + trans.PixelOffsetBottom);
            Debug.Log("PixelOffsetLeft: " + trans.PixelOffsetLeft);
            Debug.Log("PixelOffsetRight: " + trans.PixelOffsetRight);
            Debug.Log("PixelOffsetTop: " + trans.PixelOffsetTop);
            Debug.Log("Position: " + trans.Position);
            Debug.Log("PropagateDirty: " + trans.PropagateDirty);
            Debug.Log("TableArrangement: " + trans.TableArrangement);
            Debug.Log("Tag: " + trans.tag);
            Debug.Log("TiltAngle: " + trans.TiltAngle);
            Debug.Log("UniformScale: " + trans.UniformScale);
            Debug.Log("VerticalMargin: " + trans.VerticalMargin);
            Debug.Log("VerticalSpacing: " + trans.VerticalSpacing);
            Debug.Log("Visible: " + trans.Visible);
            Debug.Log("Width: " + trans.Width);
            Debug.Log("X: " + trans.X);
            Debug.Log("Y: " + trans.Y);
            Debug.Log("Z: " + trans.Z);
            Debug.Log("====================================================================================");
        }
        private void RecalculatePositions()
        {
            //frame.transform.GetComponent<AgeTransform>().Position = optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>().Position;
            LogVariousAgeTransformInfo(frame.GetComponent<AgeTransform>());
            LogVariousAgeTransformInfo(optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>());
            frame.transform.FindChild("3-RightPart").GetComponent<AgeTransform>().Position = optionsPanel.transform.FindChild("1-Frame").FindChild("3-RightPart").GetComponent<AgeTransform>().Position;
            frame.transform.FindChild("3-RightPart").FindChild("ModSettingsConfig").GetComponent<AgeTransform>().Position = optionsPanel.transform.FindChild("1-Frame").FindChild("3-RightPart").FindChild("2-ResolutionConfig").GetComponent<AgeTransform>().Position;

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
                foreach (FieldInfo f in VisibleFields)
                {
                    for (int i = 0; i < fields.Count; i++)
                    {
                        FieldInfo f2 = fields[i];
                        if (f.Equals(f2))
                        {
                            // The field exists as a button, recalculate its position.
                            Transform t = frame.transform.FindChild("3-RightPart").FindChild("ModSettingsConfig").FindChild(m.name + " - " + f.Name);

                            //t.GetComponent<AgeTransform>().PixelMarginTop = i * (settingSpacing + t.GetComponent<AgeTransform>().Height);
                            LogVariousAgeTransformInfo(t.GetComponent<AgeTransform>());
                            LogVariousAgeTransformInfo(optionsPanel.transform.FindChild("1-Frame").FindChild("3-RightPart").FindChild("2-ResolutionConfig").FindChild("24-VSyncGroup").GetComponent<AgeTransform>());
                            //t.GetComponent<AgeTransform>().Position = temp;
                        }
                    }
                }
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
            //RecalculatePositions();

            //GameObject temp = frame.transform.FindChild("3-RightPart").FindChild("ModSettingsConfig").FindChild("TrueIGT - Enabled").gameObject;
            //if (temp)
            //{
            //    GameObject.DestroyImmediate(temp);
            //}
            //frame.transform.FindChild("3-RightPart").FindChild("ModSettingsConfig").FindChild("TrueIGT - Enabled").transform.Translate(new Vector3(0, 0.0001f, 0));
            RecalculatePositions();
            base.Show(parameters);
            // TEMP

            //optionsPanel.Show(parameters);
            //GameResolutionAgeScaler scaler = optionsPanel.transform.parent.GetComponent<GameResolutionAgeScaler>();
            //// Hopefully fixes the scaling problem!
            //typeof(GameResolutionAgeScaler).GetMethod("ApplyCurrentGameResolution", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(scaler, new object[0]);
        }
        protected override IEnumerator OnShow(params object[] parameters)
        {
            Debug.Log("OnShow!");
            saveSettings = false;
            LogFrame();
            yield return base.OnShow(parameters);
            Debug.Log("AFTER SHOW!");
            LogFrame();
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
        private void LogFrame()
        {
            Debug.Log("============================== FRAME LOG");
            Debug.Log("Frame: " + frame + " AgeTfm: " + frame.GetComponent<AgeTransform>().Position);
            Debug.Log("Children:");
            foreach (AgeTransform a in frame.GetComponent<AgeTransform>().GetChildren())
            {
                Debug.Log("-" + a.name + " " + a.Position);
                Debug.Log("-Children:");
                foreach (AgeTransform q in a.GetChildren())
                {
                    Debug.Log("--" + q.name + " " + q.Position);
                    if (q.name == "ModSettingsConfig")
                    {
                        Debug.Log("--Children:");
                        foreach (AgeTransform z in q.GetChildren())
                        {
                            Debug.Log("---" + z.name + " " + z.Position);
                        }
                    }
                }
            }
            Debug.Log("Frame Anchor: " + frame.GetComponent<AgeTransform>().Anchored);
            Debug.Log("SettingsPanel Anchor: " + GetComponent<AgeTransform>().Anchored);
            Debug.Log("Options Frame: " + optionsPanel.transform.FindChild("1-Frame") + " AgeTfm: " + optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>().Position);
            Debug.Log("Children:");
            foreach (AgeTransform a in optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>().GetChildren())
            {
                Debug.Log("-" + a.name + " " + a.Position);
                Debug.Log("-Children:");
                foreach (AgeTransform q in a.GetChildren())
                {
                    Debug.Log("--" + q.name + " " + q.Position);
                    if (q.name == "2-ResolutionConfig")
                    {
                        Debug.Log("--Children:");
                        foreach (AgeTransform z in q.GetChildren())
                        {
                            Debug.Log("---" + z.name + " " + z.Position);
                        }
                    }
                }
            }
            Debug.Log("Options Frame Anchor: " + optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>().Anchored);
            Debug.Log("OptionsPanel Anchor: " + optionsPanel.GetComponent<AgeTransform>().Anchored);
            Debug.Log("============================== FRAME LOG");
        }
        public void CreateFrame()
        {
            Debug.Log("Creating the Frame!");
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

            Util.DeleteChildrenInclusive(frame.transform.FindChild("2-LeftPart").gameObject);
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
            

            // Should set up each of the children to have identical positioning as the options panel
            foreach (AgeTransform a in optionsPanel.transform.FindChild("1-Frame").GetComponent<AgeTransform>().GetChildren())
            {
                if (frame.transform.FindChild(a.name))
                {
                    frame.transform.FindChild(a.name).GetComponent<AgeTransform>().Position = a.Position;
                }
            }

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
            toggleGroup.transform.SetParent(frame.transform.FindChild("3-RightPart").FindChild("ModSettingsConfig"));
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
                foreach (FieldInfo f in VisibleFields)
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
