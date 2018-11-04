using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Gui;

namespace DotE_Mod
{
    // All of the methods in this class are placed within the DungeonGenerator2 class in "Assembly-CSharp".dll
    // This means that when a Floor (Dungeon) is loaded (Keyword: LOADED!) These methods are called
    // These methods are only called when the Floor (Dungeon) gets loaded and never again after that
    public class DungeonGenerator2_Modded
    {
        public static bool Enabled { get; set; } = true;
        public static bool UseRandomRooms { get; set; } = true;
        public static bool UseRandomDust { get; set; } = false;
        public static bool UseRandomRoomDustProbability { get; set; } = true;

        private static Dungeon InitializedDungeon;
        private static bool ExitMessageSent = false;

        // Manually injected into DungeonGenerator2: GenerateDungeonCoroutine(int, StaticString)
        // Returns how many rooms the dungeon will contain
        public static int GetRoomCount(int low, int high)
        {
            if (Enabled)
            {
                // Do some calculation to determine the new number of rooms that can be generated
                if (!UseRandomRooms)
                {
                    return 4;
                }
            }
            return RandomGenerator.RangeInt(low, high);
        }

        // Returns how much dust the entire floor will contain
        public static int GetGlobalDust(float low, float high)
        {
            if (Enabled)
            {
                // Do some calculation to determine the new amount of dust contained by the entire floor
                if (!UseRandomDust)
                {
                    return 100;
                }
            }
            return GenericUtilities.RoundHalfAwayFromZeroToInt(RandomGenerator.RangeFloat(low, high));
        }

        // Returns the weighted probability of a given room having dust
        public static float GetRoomDustLootProbWeight(GameConfig gameCfg, Room room)
        {
            
            if (Enabled)
            {
                // Do some calculation to determine the new probability of dust contained in the given room
                if (!UseRandomRoomDustProbability)
                {
                    return 1;
                }
            }
            return gameCfg.RoomDustLootProbWeight.GetValue(room);
        }

        // Need to find a good way of doing events here
        // Returns the RoomEvent that will be considered to be added to a room
        public static RoomEvent GetRoomEvent(StaticRoomEventConfig eventCfg)
        {
            if (Enabled)
            {
                // Return a new RoomEvent
            }
            return eventCfg.Name.ToEnum<RoomEvent>();
        }

        // Returns the number of static events that will occur this floor
        public static int GetRemainingEventCount(float low, float high)
        {
            if (Enabled)
            {
                // Return the total number of static events that will occur on the floor
            }
            return GenericUtilities.RoundHalfAwayFromZeroToInt(RandomGenerator.RangeFloat(low, high));
        }

        // Returns bool where true means that waves will not spawn, and false indicates they might
        // Ex: Exit never spawns waves
        public static bool GetForbidDungeonEvent(StaticRoomEventConfig eventCfg, RoomEvent roomEvent)
        {
            if (Enabled)
            {
                // Return a new bool determining if waves should be blocked
            }
            return eventCfg.ForbidDungeonEvent;
        }

        // Called when the Dungeon is fully "Filled" (Proper number of rooms, with static events)
        public static void FinishedLoading(Dungeon dungeon)
        {
            if (Enabled)
            {
                
            }
            InitializedDungeon = dungeon;
        }

        // Called when the Dungeon is finished loading the given level and the UI is visible to the player.
        public static void FinishedLoadingLevel(Dungeon dungeon)
        {
            if (Enabled)
            {
                //dungeon.EnqueueNotification(GetExit(dungeon));
            }
        }

        // Called immediately after the Dungeon is Displayed
        public static void DungeonStart(Dungeon dungeon)
        {
            if (Enabled)
            {
                // You are allowed to do things with the GUI here!
                if (dungeon != null)
                {
                    //SingletonManager.Get<LogPanel>(true).AddLog(GetExit(dungeon), NotificationType.Message, new StaticString("eventType"));
                    //dungeon.EnqueueNotification(GetExit(dungeon), null, new StaticString("eventType"), false, "notificationInformation");
                } 
            }
        }

        // Called in the Session.Update function
        public static void Update()
        {
            if (Enabled)
            {
                if (InitializedDungeon != null && !ExitMessageSent)
                {
                    Log("Entering Try-Catch: " + ExitMessageSent + " Dungeon: " + InitializedDungeon);
                    try
                    {
                        Log("Attempting to write a message to the GUI!");
                        string s = GetExit(InitializedDungeon);
                        // The EnqueueNotification is always null BECAUSE! the reference is to the dll that never has anything setup.
                        // I need to find a way to get around this by referencing the actual .dll, but then i need a way of building the new .dll
                        // From dnSpy
                        InitializedDungeon.EnqueueNotification(s, null, "MajorModule_Major0001_LVL1", true, "MajorModule_Major0001_LVL1");
                        ExitMessageSent = true;
                        Log("Wrote the message to the GUI!");
                    }
                    catch (Exception e)
                    {
                        Log("Ohno! GUI Showed before Dungeon was Initialized!");
                    }
                }
            }
        }

        private static string GetExit(Dungeon dungeon)
        {
            string s = "Path to exit:\n";
            //s = "Exit has depth: " + dungeon.ExitRoom.Depth;
            System.IO.File.WriteAllText(@"exit.txt", "Attempting to find exit!\n");
            //Room[] pathToExit = GetPathToExit(new Room[dungeon.RoomCount], 0, dungeon.RoomCount, dungeon.StartRoom);
            //foreach (Room r in pathToExit)
            //{
            //    s += "R: " + r.OpeningIndex+", ";
            //}
            s += GetPathToExit("", dungeon.StartRoom)+"\n";
            System.IO.File.AppendAllText(@"exit.txt", s);
            return s;
        }

        private static void Log(string str)
        {
            System.IO.File.AppendAllText(@"exit.txt", str+"\n");
        }

        private static string GetPathToExit(string outp, Room current)
        {
            if (current.StaticRoomEvent == RoomEvent.Exit)
            {
                Log("Found exit! " + outp);
                return outp + "ExitR: [" + current.Depth + ", " + current.CenterPosition + "]";
            }
            string o = outp + "R: [" + current.Depth + ", " + current.CenterPosition + "] ";
            //Log(o);
            foreach (Room r in current.AdjacentRooms)
            {
                if (o.IndexOf("R: [" + r.Depth + ", " + r.CenterPosition + "]") != -1)
                {
                    // Already have this within the string
                    continue;
                }
                string o2 = GetPathToExit(o, r);
                if (o2.IndexOf("ExitR") != -1)
                {
                    return o2;
                }
            }
            return o;
        }

        // Starting room: Check all of the adjacent rooms, check if exit then check all of their adjacent rooms
        private static Room[] GetPathToExit(Room[] pathSoFar, int ind, int maxCount, Room current)
        {
            Log("Checking room: " + current.ToString() + " with depth=" + current.Depth + " AdjacentRooms Count: "+current.AdjacentRooms.Count);
            if (current.StaticRoomEvent == RoomEvent.Exit)
            {
                Log("Exit found! Depth="+current.Depth);
                pathSoFar[ind] = current;
                return pathSoFar;
            }
            Room[] outp = new Room[maxCount];
            for (int i = 0; i <= ind; i++)
            {
                outp[i] = pathSoFar[i];
            }
            outp[ind] = current;
            Log("Path so Far till index:");
            for (int i = 0; i < outp.Length; i++)
            {
                if (outp[i] == null)
                {
                    continue;
                }
                Log("Item "+i+": "+outp[i].ToString());
            }
            for (int i = 0; i < current.AdjacentRooms.Count; i++)
            {
                Room r = current.AdjacentRooms[i];
                Log("Adjacent Room Depth: " + r.Depth);
                if (pathSoFar.Contains(r))
                {
                    continue;
                }
                
                Room[] temp = GetPathToExit(outp, ind+1, maxCount, r);
                for (int j = 0; j < ind+1; j++)
                {
                    outp[ind + j + ind + 1 + i] = temp[j];
                }
            }
            return outp;
        }
        // Add handles for RandomTemplate, StartSlot, Doors, etc.
    }
}
