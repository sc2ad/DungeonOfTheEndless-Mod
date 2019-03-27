using DustDevilFramework;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TASTools_Mod
{
    public class TASToolsMod : PartialityMod
    {
        ScadMod mod = new ScadMod("TASToolsMod", typeof(TASToolsSettings), typeof(TASToolsMod));

        State state = State.None;
        string SaveKey;

        private TASToolsSettings settings;

        public override void Init()
        {
            mod.PartialityModReference = this;
            mod.Initialize();

            mod.settings.ReadSettings();

            settings = (mod.settings as TASToolsSettings);

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                On.InputManager.Update += InputManager_Update;
                On.Module.BuildingUpdate += Module_BuildingUpdate;
                On.MinorModule.Update += MinorModule_Update;
                On.Hero.Update += Hero_Update;
                On.Mob.Update += Mob_Update;
                On.DungeonGenerator2.GenerateDungeonCoroutine += DungeonGenerator2_GenerateDungeonCoroutine;
            }
        }

        private void Mob_Update(On.Mob.orig_Update orig, Mob self)
        {
            if (HasFlag(State.Pausing))
            {
                return;
            }
            orig(self);
        }

        private void Hero_Update(On.Hero.orig_Update orig, Hero self)
        {
            if (HasFlag(State.Pausing))
            {
                return;
            }
            orig(self);
        }

        private void MinorModule_Update(On.MinorModule.orig_Update orig, MinorModule self)
        {
            if (HasFlag(State.Pausing))
            {
                return;
            }
            orig(self);
        }

        private void Module_BuildingUpdate(On.Module.orig_BuildingUpdate orig, Module self, float deltaTime)
        {
            if (HasFlag(State.Pausing))
            {
                return;
            }
            orig(self, deltaTime);
        }

        private System.Collections.IEnumerator DungeonGenerator2_GenerateDungeonCoroutine(On.DungeonGenerator2.orig_GenerateDungeonCoroutine orig, DungeonGenerator2 self, int level, Amplitude.StaticString shipName)
        {
            
            if (HasFlag(State.Playing) && TASInput.seeds.ContainsKey(level))
            {
                // Then we need to set the seed accordingly!
                Util.SetSeedData(TASInput.seeds[level]);
                mod.Log("Set seed to: " + TASInput.seeds[level] + " for level: " + level);
            }
            if (!TASInput.seeds.ContainsKey(level))
            {
                // Add the seed!
                SeedData data = new SeedData();
                mod.Log("Creating new SeedData for level: " + level + " with data: " + data);
                TASInput.AddSeed(level, data);
            }
            yield return orig(self, level, shipName);
        }

        private void InputManager_Update(On.InputManager.orig_Update orig, InputManager self)
        {
            // Do the recording, pausing, resetting, and playing here.
            // Updates the state
            UpdateState();
            // Performs an action depending on the state
            if (SingletonManager.Get<Dungeon>(false) == null)
            {
                // We don't have a Dungeon to work with yet!
                // This should be removed soon and instead of having levels, have time stamps or something else.
                orig(self);
                return;
            }
            int level = SingletonManager.Get<Dungeon>(false).Level;
            if (HasFlag(State.Pausing))
            {
                // Need to find some way of doing standard processing except for actually updating the game
            }
            if (HasFlag(State.Playing))
            {
                // Read Inputs from file/RAM and perform them
                mod.Log("Playing input...");
                // HEY WAIT I GOTTA DO THIS PART
            }
            if (HasFlag(State.Recording))
            {
                // Read Inputs from real action into file/RAM
                // Assumes the current level exists.
                TASInput.CreateAndAdd(level);
                mod.Log("Recording input: " + TASInput.inputs[level][TASInput.inputs[level].Count - 1].ToString());
                ToggleState(State.Recording);
            }
            if (HasFlag(State.Saving))
            {
                // Save the current floor as a start point (somehow)
                // Probably just save the SeedData and do a "generateForLevel" or something
                SaveKey = GameSave.GenerateSaveKey();
                SingletonManager.Get<Dungeon>(false).SaveGameData(SaveKey);
                mod.Log("Saved Key: " + SaveKey + " saved successfully!");
                ToggleState(State.Saving);
            }
            if (HasFlag(State.SavingToFile))
            {
                // Assume the Dungeon exists, use it to determine the level to write the TAS for.
                mod.Log("Writing files with *" + settings.TasFileExtension);
                TASIO.WriteTAS(TASInput.inputs, settings.TasFileExtension);
                ToggleState(State.SavingToFile);
            }
            if (HasFlag(State.ReadingFromFiles))
            {
                // Don't assume that the Dungeon exists, but read the many information things from the TAS file.
                // The trick with this is that the seed also needs to be read.
                TASInput.Clear();
                mod.Log("Reading from *" + settings.TasFileExtension + " files...");
                foreach (string file in System.IO.Directory.GetFiles(".", "*" + settings.TasFileExtension))
                {
                    // Let's just load all of the info into the TASInput RAM
                    mod.Log("Reading file: " + file);
                    TASIO.ReadTAS(file);
                }
                ToggleState(State.ReadingFromFiles);
            }
            if (HasFlag(State.Resetting))
            {
                // Reset to the current save point (whatever floor it is)
                // Probably just do a "generateForLevel" or something (make the game think the data was saved in a certain way)
                mod.Log("Attempting to load SaveKey: " + SaveKey);
                if (SaveKey.Length > 0)
                {
                    // Only now can we load valid data.
                    mod.Log("Preparing Dungeon for SaveKey!");
                    Dungeon.PrepareForSavedGame(SaveKey, false);
                }
                ToggleState(State.Resetting);
            }
            if (HasFlag(State.Clearing))
            {
                // Clear the data for this level
                TASInput.ClearForLevel(level);
                mod.Log("Cleared all TAS data for level: " + level);
                ToggleState(State.Clearing);
            }
            orig(self);
        }

        public void UnLoad()
        {
            mod.UnLoad();
            On.InputManager.Update -= InputManager_Update;
            On.Module.BuildingUpdate -= Module_BuildingUpdate;
            On.MinorModule.Update -= MinorModule_Update;
            On.Hero.Update -= Hero_Update;
            On.Mob.Update -= Mob_Update;
            On.DungeonGenerator2.GenerateDungeonCoroutine -= DungeonGenerator2_GenerateDungeonCoroutine;
        }

        private void ToggleState(State s)
        {
            if (HasFlag(s))
            {
                state &= ~s;
            }
            else
            {
                state |= s;
            }
        }

        private bool TestKeyPress(string key)
        {
            UnityEngine.KeyCode code = (UnityEngine.KeyCode) Enum.Parse(typeof(UnityEngine.KeyCode), key);
            return UnityEngine.Input.GetKeyUp(code);
        }

        private bool HasFlag(State s)
        {
            return (state & s) != 0;
        }

        private void UpdateState()
        {
            if (TestKeyPress(settings.PlayKey))
            {
                ToggleState(State.Playing);
            }
            else if (TestKeyPress(settings.PauseKey))
            {
                ToggleState(State.Pausing);
            }
            else if (TestKeyPress(settings.RecordKey))
            {
                ToggleState(State.Recording);
            }
            else if (TestKeyPress(settings.ResetKey))
            {
                ToggleState(State.Resetting);
            }
            else if (TestKeyPress(settings.SaveKey))
            {
                ToggleState(State.Saving);
            }
            else if (TestKeyPress(settings.ReadFromFilesKey))
            {
                ToggleState(State.ReadingFromFiles);
            }
            else if (TestKeyPress(settings.SaveToFilesKey))
            {
                ToggleState(State.SavingToFile);
            }
            else if (TestKeyPress(settings.ClearKey))
            {
                ToggleState(State.Clearing);
            }
        }
    }
}
