using DustDevilFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TASTools_Mod
{
    [BepInPlugin("com.sc2ad.TASTools", "TASTools", "1.0.0")]
    public class TASToolsMod : BaseUnityPlugin
    {
        private ConfigWrapper<string> tasFileExtensionWrapper;
        private ConfigWrapper<string> playKeyWrapper;
        private ConfigWrapper<string> recordKeyWrapper;
        private ConfigWrapper<string> pauseKeyWrapper;
        private ConfigWrapper<string> resetKeyWrapper;
        private ConfigWrapper<string> saveKeyWrapper;
        private ConfigWrapper<string> saveToFilesKeyWrapper;
        private ConfigWrapper<string> readFromFilesKeyWrapper;
        private ConfigWrapper<string> clearKeyWrapper;
        private ConfigWrapper<bool> isHumanWrapper;

        ScadMod mod;

        State state = State.None;
        string SaveKey;

        public void Awake()
        {
            mod = new ScadMod("TASToolsMod", this);

            tasFileExtensionWrapper = Config.Wrap("Settings", "TasFileExtension", "The file extension for all TAS files.", ".tas");
            playKeyWrapper = Config.Wrap("Settings", "PlayKey", "The UnityEngine.KeyCode to use for playing the TAS.", KeyCode.Quote.ToString());
            recordKeyWrapper = Config.Wrap("Settings", "RecordKey", "The UnityEngine.KeyCode to use for recording the TAS.", KeyCode.Semicolon.ToString());
            pauseKeyWrapper = Config.Wrap("Settings", "PauseKey", "The UnityEngine.KeyCode to use for pausing the TAS.", KeyCode.P.ToString());
            resetKeyWrapper = Config.Wrap("Settings", "ResetKey", "The UnityEngine.KeyCode to use for resetting the current state.", KeyCode.RightBracket.ToString());
            saveKeyWrapper = Config.Wrap("Settings", "SaveKey", "The UnityEngine.KeyCode to use for saving the current state.", KeyCode.LeftBracket.ToString());
            saveToFilesKeyWrapper = Config.Wrap("Settings", "SaveToFilesKey", "The UnityEngine.KeyCode to use for saving the TAS to files.", KeyCode.O.ToString());
            readFromFilesKeyWrapper = Config.Wrap("Settings", "ReadFromFilesKey", "The UnityEngine.KeyCode to use for reading the TAS from files.", KeyCode.Slash.ToString());
            clearKeyWrapper = Config.Wrap("Settings", "ClearKey", "The UnityEngine.KeyCode to use for clearing the TAS.", KeyCode.C.ToString());
            isHumanWrapper = Config.Wrap("Settings", "IsHuman", "Whether the TAS is only taking actions humans can, or if it is also setting the seed.", false);

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
            {
                On.InputManager.Update += InputManager_Update;
                On.Module.BuildingUpdate += Module_BuildingUpdate;
                On.MinorModule.Update += MinorModule_Update;
                On.Hero.Update += Hero_Update;
                On.Mob.Update += Mob_Update;
                On.Mover.Update += Mover_Update;
                On.Attacker.Update += Attacker_Update;
                // Precision Seed Control
                On.Dungeon.TriggerEvents += Dungeon_TriggerEvents;
                // Playback
                On.InputManager.Update_MouseEvents += InputManager_Update_MouseEvents;
                On.InputManager.GetControlStatus += InputManager_GetControlStatus;
                On.InputManager.TriggerClickDownEvent += InputManager_TriggerClickDownEvent;
                On.InputManager.TriggerClickUpEvent += InputManager_TriggerClickUpEvent;
                On.DungeonGenerator2.GenerateDungeonCoroutine += DungeonGenerator2_GenerateDungeonCoroutine;
            }
        }


        private IEnumerator Dungeon_TriggerEvents(On.Dungeon.orig_TriggerEvents orig, Dungeon self, Room openingRoom, HeroMobCommon opener, bool canTriggerDungeonEvent)
        {
            SeedCollection collection = SeedCollection.GetMostCurrentSeeds(self.ShipName, self.Level);
            if (collection == null || !collection.Enabled || isHumanWrapper.Value)
            {
                yield return self.StartCoroutine(orig(self, openingRoom, opener, canTriggerDungeonEvent));
                yield break;
            }
            else
            {
                // Load seeds for each door from files/somewhere
                // Restore the seed for this room openingRoom
                RandomGenerator.RestoreSeed();

                // Calls TriggerEvents the proper number of times. Hangs on this call.
                // Random deviation seems to appear while this Coroutine is running, possibly due to monster random actions?
                // Could fix this by storing all before/after seeds, but doesn't that seem lame?
                // Would like to find a way of only wrapping the Random calls with this so that there is less UnityEngine.Random.seed
                // noise from other sources that occur during the runtime.
                // The above will probably not work, so instead wrap everything EXCEPT for the wait in the Random Save/Restore
                // Possible error from SpawnWaves, SpawnMobs (cause they have dedicated Coroutines that run)
                yield return self.StartCoroutine(orig(self, openingRoom, opener, canTriggerDungeonEvent));

                // I'm going to cheat for now and SKIP the saving step - the same exact seed is ALWAYS used for RandomGenerator
                // When using RandomGenerator seeds.
                //mod.Log("Saving Seed!");
                //RandomGenerator.SaveSeed();

                yield break;
            }
        }

        private void InputManager_TriggerClickUpEvent(On.InputManager.orig_TriggerClickUpEvent orig, InputManager self, string eventName, ref List<ClickDownInfo> clickInfosContainer)
        {
            if (HasFlag(State.Playing) && !HasFlag(State.Pausing))
            {
                InputManagerHooks.TriggerClickUpEvent(self, eventName, ref clickInfosContainer);
                return;
            }
            orig(self, eventName, ref clickInfosContainer);
        }

        private void InputManager_TriggerClickDownEvent(On.InputManager.orig_TriggerClickDownEvent orig, InputManager self, string eventName, ref List<ClickDownInfo> clickInfosContainer)
        {
            if (HasFlag(State.Playing) && !HasFlag(State.Pausing))
            {
                InputManagerHooks.TriggerClickDownEvent(self, eventName, ref clickInfosContainer);
                return;
            }
            orig(self, eventName, ref clickInfosContainer);
        }

        private bool InputManager_GetControlStatus(On.InputManager.orig_GetControlStatus orig, InputManager self, Control control, ControlStatus controlStatus)
        {
            if (HasFlag(State.Playing) && !HasFlag(State.Pausing))
            {
                return TASInputPlayer.GetControlStatus(control, controlStatus);
            }
            return orig(self, control, controlStatus);
        }

        private void InputManager_Update_MouseEvents(On.InputManager.orig_Update_MouseEvents orig, InputManager self)
        {
            if (HasFlag(State.Playing) && !HasFlag(State.Pausing))
            {
                InputManagerHooks.Update_MouseEvents(self);
            }
            orig(self);
        }

        private void Attacker_Update(On.Attacker.orig_Update orig, Attacker self)
        {
            if (HasFlag(State.Pausing))
            {
                return;
            }
            orig(self);
        }

        private void Mover_Update(On.Mover.orig_Update orig, Mover self)
        {
            if (HasFlag(State.Pausing))
            {
                return;
            }
            orig(self);
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

        private IEnumerator DungeonGenerator2_GenerateDungeonCoroutine(On.DungeonGenerator2.orig_GenerateDungeonCoroutine orig, DungeonGenerator2 self, int level, Amplitude.StaticString shipName)
        {
            SeedData seeds = TASInput.seeds.GetSeedForShipLevel(shipName, level);
            if (seeds != null)
            {
                // Then we need to set the seed accordingly!
                seeds.SetSeedData();
                mod.Log("Set seed to: " + seeds + " for level: " + level);
            }
            else
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
                // That would allow for TAS in menus (without a Dungeon)
                orig(self);
                return;
            }
            int level = SingletonManager.Get<Dungeon>(false).Level;
            if (HasFlag(State.Pausing))
            {
                // Need to find some way of doing standard processing except for actually updating the game
            }
            if (HasFlag(State.Playing) && !HasFlag(State.Pausing))
            {
                // Read Inputs from file/RAM and perform them
                if (TASInput.HasNext(level))
                {
                    TASInputPlayer.Update(TASInput.GetNext(level));
                    mod.Log("Playing input: " + TASInputPlayer.Current);
                } else
                {
                    mod.Log("Completed input playback!");
                    ToggleState(State.Playing);
                }
                
            }
            if (HasFlag(State.Recording))
            {
                // Read Inputs from real action into file/RAM
                // Assumes the current level exists.
                TASInput.CreateAndAdd(level);
                // Also make sure that the seed is correctly set
                mod.Log("Recording input: " + TASInput.inputs[level][TASInput.inputs[level].Count - 1].ToString());
                // TODO: REMOVE LINE BELOW
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
                mod.Log("Writing files with *" + tasFileExtensionWrapper.Value);
                TASIO.WriteTAS(TASInput.inputs, tasFileExtensionWrapper.Value);
                ToggleState(State.SavingToFile);
            }
            if (HasFlag(State.ReadingFromFiles))
            {
                // Don't assume that the Dungeon exists, but read the many information things from the TAS file.
                // The trick with this is that the seed also needs to be read.
                TASInput.Clear();
                mod.Log("Reading from *" + tasFileExtensionWrapper.Value + " files...");
                foreach (string file in System.IO.Directory.GetFiles(".", "*" + tasFileExtensionWrapper.Value))
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
            On.Mover.Update -= Mover_Update;
            On.Attacker.Update -= Attacker_Update;

            On.Dungeon.TriggerEvents -= Dungeon_TriggerEvents;

            On.InputManager.Update_MouseEvents -= InputManager_Update_MouseEvents;
            On.InputManager.GetControlStatus -= InputManager_GetControlStatus;
            On.InputManager.TriggerClickDownEvent -= InputManager_TriggerClickDownEvent;
            On.InputManager.TriggerClickUpEvent -= InputManager_TriggerClickUpEvent;

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
            if (TestKeyPress(playKeyWrapper.Value))
            {
                ToggleState(State.Playing);
            }
            else if (TestKeyPress(pauseKeyWrapper.Value))
            {
                ToggleState(State.Pausing);
            }
            else if (TestKeyPress(recordKeyWrapper.Value))
            {
                ToggleState(State.Recording);
            }
            else if (TestKeyPress(resetKeyWrapper.Value))
            {
                ToggleState(State.Resetting);
            }
            else if (TestKeyPress(saveKeyWrapper.Value))
            {
                ToggleState(State.Saving);
            }
            else if (TestKeyPress(readFromFilesKeyWrapper.Value))
            {
                ToggleState(State.ReadingFromFiles);
            }
            else if (TestKeyPress(saveToFilesKeyWrapper.Value))
            {
                ToggleState(State.SavingToFile);
            }
            else if (TestKeyPress(clearKeyWrapper.Value))
            {
                ToggleState(State.Clearing);
            }
        }
    }
}
