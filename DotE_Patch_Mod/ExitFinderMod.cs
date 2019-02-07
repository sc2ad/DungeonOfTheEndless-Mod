using DustDevilFramework;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UnityEngine;

namespace ExitFinder_Mod
{
    public class ExitFinderMod : PartialityMod
    {
        ScadMod mod = new ScadMod("ExitFinder", typeof(ExitFinderSettings), typeof(ExitFinderMod));
        private static bool DisplayExit = true;

        private static Room ExitRoom;

        public override void Init()
        {
            mod.Initialize();

            mod.settings.ReadSettings();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                On.Session.Update += Session_Update;
                On.Dungeon.SpawnExit += Dungeon_SpawnExit;
            }
        }
        public void UnLoad()
        {
            mod.UnLoad();
            On.Session.Update -= Session_Update;
            On.Dungeon.SpawnExit -= Dungeon_SpawnExit;
        }

        private void Dungeon_SpawnExit(On.Dungeon.orig_SpawnExit orig, Dungeon self, Room spawnRoom)
        {
            orig(self, spawnRoom);
            mod.Log("Called a SpawnExit function!");
        }

        private void Session_Update(On.Session.orig_Update orig, Session self)
        {
            orig(self);
            KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), (mod.settings as ExitFinderSettings).Key);
            if (Input.GetKeyDown(key))
            {
                try
                {
                    Dungeon d = SingletonManager.Get<Dungeon>(false);
                    mod.Log("Attempting to find exit in Dungeon...");
                    d.EnqueueNotification(GetExit(d));
                    if (ExitRoom.CrystalModuleSlots.Count > 0)
                    {
                        // The exit already exists, don't make another one!
                        return;
                    }
                    if (DisplayExit)
                    {
                        mod.Log("Attemping to display exit...");
                        ExitRoom.AddCrystalSlot(true);
                        d.DisplayCrystalAndExitOffscreenMarkers(ExitRoom.CrystalModuleSlots[0].transform).Show(true);
                    }
                } catch (NullReferenceException e)
                {
                    // The Dungeon/GUI has yet to be setup!
                    // Let's just wait a bit and log it.
                    mod.Log("Dungeon/GUI Not yet initialized!");
                }
            }
        }

        private string GetExit(Dungeon d)
        {

            //List<Room> path = FindPathToExit(new List<Room>(), d.StartRoom, d.ExitRoom);
            string pa = GetPathToExit("", d.StartRoom);
            List<Vector3> path = new List<Vector3>();
            mod.Log("Path to exit (before instructions): " + pa);
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
            mod.Log("Exit Vectors:\n============================");
            foreach (Vector3 v in path)
            {
                mod.Log("Vector: " + v);
            }
            mod.Log("============================");
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
            mod.Log("Instructions: " + instructions);
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
    }
}
