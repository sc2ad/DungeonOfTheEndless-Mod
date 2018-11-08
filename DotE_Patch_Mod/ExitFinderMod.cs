using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UnityEngine;

namespace DotE_Patch_Mod
{
    public class ExitFinderMod : PartialityMod
    {
        private static string path = @"C:\Program Files (x86)\Steam\steamapps\common\Dungeon of the Endless\Patched_log.txt";
        private static bool DisplayExit = true;

        private static Room ExitRoom;

        private static void Log(string s)
        {
            System.IO.File.AppendAllText(path, s + "\n");
        }
        private static void ClearLog()
        {
            System.IO.File.WriteAllText(path, "");
        }
        public override void Init()
        {
            author = "Sc2ad";
            ClearLog();
            Log("===============================================");
            Log("DATETIME: " + DateTime.Today.ToLongDateString());
            Log("===============================================");
            Log("Initialized!");
        }
        public override void OnLoad()
        {
            Log("Loaded!");
            On.Session.Update += Session_Update;
            On.Dungeon.SpawnExit += Dungeon_SpawnExit;
        }

        private void Dungeon_SpawnExit(On.Dungeon.orig_SpawnExit orig, Dungeon self, Room spawnRoom)
        {
            orig(self, spawnRoom);
            Log("Called a SpawnExit function!");
        }

        private void Session_Update(On.Session.orig_Update orig, Session self)
        {
            orig(self);
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                try
                {
                    Dungeon d = SingletonManager.Get<Dungeon>(false);
                    Log("Attempting to find exit in Dungeon...");
                    d.EnqueueNotification(GetExit(d));
                    if (DisplayExit)
                    {
                        Log("Attemping to display exit...");
                        //OffscreenMarker.OffscreenMarkerData data = default(OffscreenMarker.OffscreenMarkerData);
                        ExitRoom.AddCrystalSlot(true);
                        d.DisplayCrystalAndExitOffscreenMarkers(ExitRoom.CrystalModuleSlots[0].transform).Show(true);
                        //List<CrystalModuleSlot> slots = d.ExitRoom.CrystalModuleSlots;
                        //Log("Crystal Slot: "+slots[0].ToString());
                        //slots[0].Activate();
                    }
                } catch (NullReferenceException e)
                {
                    // The Dungeon/GUI has yet to be setup!
                    // Let's just wait a bit and log it.
                    Log("Dungeon/GUI Not yet initialized!");
                }
            }
        }

        private string GetExit(Dungeon d)
        {

            //List<Room> path = FindPathToExit(new List<Room>(), d.StartRoom, d.ExitRoom);
            string pa = GetPathToExit("", d.StartRoom);
            List<Vector3> path = new List<Vector3>();
            Log("Path to exit (before instructions): " + pa);
            string[] splits = pa.Split(new string[] { "R: [(" }, StringSplitOptions.None);
            foreach (string s in splits)
            {
                if (s.IndexOf(")]") == -1)
                {
                    continue;
                }
                string q = s.Substring(0, s.IndexOf(")]") - 1);
                string[] vec = q.Split(new string[] { ", " }, StringSplitOptions.None);
                path.Add(new Vector3((float)Convert.ToDouble(vec[0]), (float)Convert.ToDouble(vec[1]), (float)Convert.ToDouble(vec[2])));
            }
            Log("Exit Vectors:\n============================");
            foreach (Vector3 v in path)
            {
                Log("Vector: " + v);
            }
            Log("============================");
            string instructions = "Path to exit: ";
            for (int i = 1; i < path.Count; i++)
            {
                Vector3 delta = path[i] - path[i - 1];
                if (Math.Abs(delta.z) > Math.Abs(delta.x))
                {
                    // The next step should be either up or down
                    if (delta.z < 0)
                    {
                        instructions += "U ";
                    } else if (delta.z > 0)
                    {
                        instructions += "D ";
                    }
                } else if (Math.Abs(delta.x) > Math.Abs(delta.z))
                {
                    // The next step should be either left or right
                    if (delta.x < 0)
                    {
                        // Yes, this is backwards. It is just how the coordinate system works...
                        instructions += "R ";
                    } else if (delta.x > 0)
                    {
                        instructions += "L ";
                    }
                }
            }
            Log("Instructions: " + instructions);
            return instructions;
        }

        private List<Room> FindPathToExit(List<Room> pathSoFar, Room Current, Room Exit)
        {
            if (Current == Exit)
            {
                pathSoFar.Add(Current);
                return pathSoFar;
            }
            List<Room> outp = new List<Room>();
            outp.AddRange(pathSoFar);
            outp.Add(Current);
            foreach (Room r in Current.AdjacentRooms)
            {
                if (pathSoFar.Contains(r))
                {
                    continue;
                }
                List<Room> p = FindPathToExit(outp, r, Exit);
                if (p.Contains(Exit))
                {
                    return p;
                }
            }
            return outp;
        }

        private static string GetPathToExit(string outp, Room current)
        {
            if (current.StaticRoomEvent == RoomEvent.Exit)
            {
                ExitRoom = current;
                return outp + "ExitR: [" + current.CenterPosition + "]";
            }
            string o = outp + "R: [" + current.CenterPosition + "] ";
            //Log(o);
            foreach (Room r in current.AdjacentRooms)
            {
                if (o.IndexOf("R: [" + r.CenterPosition + "]") != -1)
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

        public override void OnEnable()
        {
            Log("Enabled!");
        }
        public override void OnDisable()
        {
            Log("Disabled!");
        }
        
    }
}
