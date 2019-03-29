using Amplitude.Unity.Framework;
using DustDevilFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeededDungeon_Mod
{
    public class SeededDungeonMod : PartialityMod
    {
        ScadMod mod = new ScadMod("SeededDungeon", typeof(SeededDungeonSettings), typeof(SeededDungeonMod));
        Dictionary<Amplitude.StaticString, Dictionary<int, SeedData>> shipSeeds = new Dictionary<Amplitude.StaticString, Dictionary<int, SeedData>>();
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
                // When the level loads, need to instead load a seed from config, for example
                // So the only things that seem to exist in the same locations are the layouts, layout sizes, and exits
                // Other static/dynamic events are unknown based off of solely Seed
                // However... What if I used recursive logging to find all of the data i need, then use recursive reading/setting in the same order.
                On.DungeonGenerator2.GenerateDungeonCoroutine += DungeonGenerator2_GenerateDungeonCoroutine;
                On.Dungeon.TriggerEvents += Dungeon_TriggerEvents;
                On.ShipConfig.GetLocalizedDescription += ShipConfig_GetLocalizedDescription;
                //IL.Dungeon.TriggerEvents += Dungeon_TriggerEvents1;
            }
            if ((mod.settings as SeededDungeonSettings).ReadFromSeedLog)
            {
                ReadSeeds();
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
            On.DungeonGenerator2.GenerateDungeonCoroutine -= DungeonGenerator2_GenerateDungeonCoroutine;
            On.ShipConfig.GetLocalizedDescription -= ShipConfig_GetLocalizedDescription;
            On.Dungeon.TriggerEvents -= Dungeon_TriggerEvents;
            //IL.Dungeon.TriggerEvents -= Dungeon_TriggerEvents1;
        }

        private System.Collections.IEnumerator Dungeon_TriggerEvents(On.Dungeon.orig_TriggerEvents orig, Dungeon self, Room openingRoom, HeroMobCommon opener, bool canTriggerDungeonEvent)
        {
            if (!shipSeeds.ContainsKey(self.ShipName) || !shipSeeds[self.ShipName].ContainsKey(self.Level))
            {
                yield return self.StartCoroutine(orig(self, openingRoom, opener, canTriggerDungeonEvent));
                yield break;
            }
            else
            {
                mod.Log("Triggering Events!");
                RandomGenerator.RestoreSeed();
                mod.Log("Restoring Seed!");
                int temp = RandomGenerator.Seed;
                RandomGenerator.RestoreSeed();
                int saved = RandomGenerator.Seed;
                RandomGenerator.SetSeed(temp);

                mod.Log("Before SeedData: " + new SeedData() + " saved: " + saved);

                // Calls TriggerEvents the proper number of times. Hangs on this call.
                // Random deviation seems to appear while this Coroutine is running, possibly due to monster random actions?
                // Could fix this by storing all before/after seeds, but doesn't that seem lame?
                // Would like to find a way of only wrapping the Random calls with this so that there is less UnityEngine.Random.seed
                // noise from other sources that occur during the runtime.
                // The above will probably not work, so instead wrap everything EXCEPT for the wait in the Random Save/Restore
                // Possible error from SpawnWaves, SpawnMobs (cause they have dedicated Coroutines that run)
                yield return self.StartCoroutine(orig(self, openingRoom, opener, canTriggerDungeonEvent));
                temp = RandomGenerator.Seed;
                RandomGenerator.RestoreSeed();
                saved = RandomGenerator.Seed;
                RandomGenerator.SetSeed(temp);
                mod.Log("After SeedData: " + new SeedData() + " saved: " + saved);

                // I'm going to cheat for now and SKIP the saving step - the same exact seed is ALWAYS used for RandomGenerator
                // When using RandomGenerator seeds.
                //mod.Log("Saving Seed!");
                //RandomGenerator.SaveSeed();

                yield break;
            }
        }

        private string ShipConfig_GetLocalizedDescription(On.ShipConfig.orig_GetLocalizedDescription orig, ShipConfig self)
        {
            if (shipSeeds.ContainsKey(self.Name) && shipSeeds[self.Name].ContainsKey(1))
            {
                // If the seed for the first level exists, display the seed at the bottom of the description.
                string temp = orig(self) + "\n\n#98FB98#Seeds: " + shipSeeds[self.Name][1] + "#98FB98#";
                mod.Log("Attempting to update description to include seeds!");
                return temp;
            }
            return orig(self);
        }

        private System.Collections.IEnumerator DungeonGenerator2_GenerateDungeonCoroutine(On.DungeonGenerator2.orig_GenerateDungeonCoroutine orig, DungeonGenerator2 self, int level, Amplitude.StaticString shipName)
        {
            if (shipSeeds.ContainsKey(shipName) && shipSeeds[shipName].ContainsKey(level))
            {
                // And if I actually want to apply the seed change...
                mod.Log("Creating the Dungeon with the provided seed from the dictionary: " + shipSeeds[shipName][level]);
                SingletonManager.Get<Dungeon>(false).EnqueueNotification("Using Seeds: " + shipSeeds[shipName][level]);
                shipSeeds[shipName][level].SetSeedData();
            }
            yield return orig(self, level, shipName);
            if (!shipSeeds.ContainsKey(shipName))
            {
                shipSeeds.Add(shipName, new Dictionary<int, SeedData>());
            }
            if (!shipSeeds[shipName].ContainsKey(level))
            {
                // And if I actually want to overwrite the existing seed...
                int seed = new DynData<DungeonGenerator2>(self).Get<int>("randomSeed");
                mod.Log("Adding a seed to the seeds dictionary: " + seed);
                shipSeeds[shipName].Add(level, new SeedData(seed, RandomGenerator.Seed, UnityEngine.Random.seed));
                SingletonManager.Get<Dungeon>(false).EnqueueNotification("Added Seeds: " + shipSeeds[shipName][level]);
                if ((mod.settings as SeededDungeonSettings).LogSeeds)
                {
                    LogSeeds();
                }
            }
            LogRoomData(GetRoomList(SingletonManager.Get<Dungeon>(false)));
            yield break;
        }
        private void LogSeeds()
        {
            string text = "Seeds for each level in most recently played Pod\n";
            foreach (Amplitude.StaticString name in shipSeeds.Keys)
            {
                Dictionary<int, SeedData> seeds = shipSeeds[name];
                foreach (int level in seeds.Keys)
                {
                    text += "N:" + name + ":L:" + level + ":" + seeds[level] + "\n";
                }
            }
            System.IO.File.WriteAllText((mod.settings as SeededDungeonSettings).SeedLogPath, text);
        }
        private void ReadSeeds()
        {
            mod.Log("Reading Seeds from: " + (mod.settings as SeededDungeonSettings).SeedLogPath);
            string[] lines = System.IO.File.ReadAllLines((mod.settings as SeededDungeonSettings).SeedLogPath);
            foreach (string line in lines)
            {
                if (line.IndexOf("N:") == -1)
                {
                    continue;
                }
                string[] spl = line.Split(new string[] { ":" }, StringSplitOptions.None);
                Amplitude.StaticString name = spl[1];
                string level = spl[3];
                string data = spl[spl.Length - 1];
                mod.Log("Adding Seed for ship: " + name + " with level: " + level + " with data: " + data);
                if (!shipSeeds.ContainsKey(name))
                {
                    shipSeeds.Add(name, new Dictionary<int, SeedData>());
                }
                shipSeeds[name].Add(Convert.ToInt32(level), new SeedData(data));
            }
        }
        private void LogRoomData(List<Room> rooms)
        {
            mod.Log("Logging room data:");
            string text = "Room Count: " + rooms.Count + "\n";
            foreach (Room r in rooms)
            {
                mod.Log("Name: " + r.name + " event: " + r.StaticRoomEvent + " UID: " + r.UniqueID + " Dust: " + r.DustLootAmount);
                text += "Name: " + r.name + " event: " + r.StaticRoomEvent + " UID: " + r.UniqueID + " Dust: " + r.DustLootAmount + "\n";
            }
            System.IO.File.WriteAllText((mod.settings as SeededDungeonSettings).RoomDataPath, text);

        }
        private void ReadRoomData()
        {
            mod.Log("Reading room data...");
            string[] lines = System.IO.File.ReadAllLines((mod.settings as SeededDungeonSettings).RoomDataPath);
            // Need to somehow set various rooms' data to what is provided in this data
        }
        private List<Room> GetRoomList(Dungeon d)
        {
            List<Room> rooms = new List<Room>();
            // Recursively add all adjacent rooms that don't already exist within the list until all rooms have been added
            AddRoomRecursive(d.StartRoom, rooms);
            mod.Log("RoomList Count: " + rooms.Count);
            return rooms;
        }
        private void AddRoomRecursive(Room r, List<Room> rooms)
        {
            if (rooms.Contains(r))
            {
                return;
            }
            rooms.Add(r);
            foreach (Room r2 in r.AdjacentRooms)
            {
                AddRoomRecursive(r2, rooms);
            }
        }
    }
}
