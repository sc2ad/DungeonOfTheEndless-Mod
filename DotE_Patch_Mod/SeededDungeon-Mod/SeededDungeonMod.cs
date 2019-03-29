using DustDevilFramework;
using Partiality.Modloader;
using System;
using UnityEngine;

namespace SeededDungeon_Mod
{
    public class SeededDungeonMod : PartialityMod
    {
        ScadMod mod = new ScadMod("SeededDungeon", typeof(SeededDungeonSettings), typeof(SeededDungeonMod));
        public override void Init()
        {
            mod.PartialityModReference = this;
            mod.Initialize();

            mod.settings.ReadSettings();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                // This reads all of the seeds that we have in local files into RAM so that we can use it.
                SeedCollection.ReadAll();
                // When the level loads, need to instead load a seed from config, for example
                // So the only things that seem to exist in the same locations are the layouts, layout sizes, and exits
                // Other static/dynamic events are unknown based off of solely Seed
                // However... What if I used recursive logging to find all of the data i need, then use recursive reading/setting in the same order.
                //IL.Dungeon.TriggerEvents += Dungeon_TriggerEvents1;
                On.InputManager.Update += InputManager_Update;
            }
        }

        private void InputManager_Update(On.InputManager.orig_Update orig, InputManager self)
        {
            orig(self);
            Dungeon d = SingletonManager.Get<Dungeon>(false);
            if (d == null)
            {
                return;
            }
            if (Input.GetKeyUp((KeyCode) Enum.Parse(typeof(KeyCode), (mod.settings as SeededDungeonSettings).SaveKey)))
            {
                mod.Log("Saving SeedData to SeedCollection!");
                SeedCollection best = SeedCollection.GetMostCurrentSeeds(d.ShipName, d.Level);
                if (best == null)
                {
                    mod.Log("Creating new SeedCollection because there were no matching SeedCollections!");
                    best = SeedCollection.Create();
                }
                best.Add(d.ShipName, d.Level, new SeedData());
                SeedCollection.WriteAll();
                mod.Log("Wrote SeedCollection to: " + best.ReadFrom);
            }
            if (Input.GetKeyUp((KeyCode) Enum.Parse(typeof(KeyCode), (mod.settings as SeededDungeonSettings).CreateNewSeedKey)))
            {
                mod.Log("Created new SeedCollection!");
                SeedCollection best = SeedCollection.Create();
                best.Add(d.ShipName, d.Level, new SeedData());
                SeedCollection.WriteAll();
                mod.Log("Wrote SeedCollection to: " + best.ReadFrom);
            }
        }

        // NEED TO UPDATE PARTIALITY TO USE LATEST MONOMOD IN ORDER FOR THIS TO WORK! (USES MONO.CECIL OF A TOO ADVANCED VERSION)
        // SEE: https://github.com/0xd4d/dnSpy/issues/173
        // CODE ORIGINALLY BASED OFF OF: https://gist.github.com/0x0ade/1d1013d6ae1ff450aa76f252b0f3b62c

        //private void Dungeon_TriggerEvents1(HookIL il)
        //{
        //    mod.Log("Module Definition for Dungeon.TriggerEvents: " + il.Module);
        //    HookILCursor cursor = il.At(0);
        //    while (cursor.TryGotoNext(
        //        i => i.MatchCall<RandomGenerator>("RandomRange")
        //    ))
        //    {
        //        int rangeIndex = cursor.Index;
        //        mod.Log("Found IL for RandomRange at IL Index: " + rangeIndex);
        //        // This probably won't work, because if there are calls inside the method call bad things will happen
        //        cursor.Index -= 2;
        //        mod.Log("Shifted cursor.Index to: " + cursor.Index);

        //        TypeDefinition generator = null;
        //        MethodDefinition save = null;
        //        MethodDefinition restore = null;
        //        foreach (ModuleDefinition m in il.Module.Assembly.Modules)
        //        {
        //            foreach (TypeDefinition d in m.GetTypes()) {
        //                if (d.IsClass && d.Name.Equals("RandomGenerator"))
        //                {
        //                    mod.Log("Found RandomGenerator type definition: " + d);
        //                    generator = d;
        //                    foreach (MethodDefinition method in d.Methods)
        //                    {
        //                        if (method.Name.Equals("SaveSeed"))
        //                        {
        //                            mod.Log("Found SaveSeed method definition: " + method);
        //                            save = method;
        //                        }
        //                        if (method.Name.Equals("RestoreSeed"))
        //                        {
        //                            mod.Log("Found RestoreSeed method definition: " + method);
        //                            restore = method;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (generator == null)
        //        {
        //            mod.Log("Could not find RandomGenerator!");
        //        } else
        //        {
        //            if (save == null || restore == null)
        //            {
        //                mod.Log("Could not find either save/restore method definitions!");
        //            } else
        //            {
        //                mod.Log("Attempting to write Save to index: " + cursor.Index);
        //                cursor.Emit(OpCodes.Call, save);
        //                // This should at least dodge the stloc or stfld that is happening when this method is called
        //                // But it probably won't dodge the possibility of there being other calls and whatnot
        //                cursor.Index = rangeIndex + 2;
        //                mod.Log("Attempting to write Restore to index: " + cursor.Index);
        //                cursor.Emit(OpCodes.Call, restore);
        //            }
        //        }
        //    }
        //}

        public void UnLoad()
        {
            mod.UnLoad();
            On.InputManager.Update -= InputManager_Update;
            //IL.Dungeon.TriggerEvents -= Dungeon_TriggerEvents1;
        }
    }
}
