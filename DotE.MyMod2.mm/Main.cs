using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonoftheEndless.MyMod2.mm
{
    public static class Main
    {
        public static string logPath = @"log.txt";
        public static void Log(string s)
        {
            System.IO.File.AppendAllText(logPath, s);
        }
    }
    public class patch_EndingPanel : EndingPanel
    {
        public extern void orig_Show(params object[] parameters);
        public override void Show(params object[] parameters)
        {
            orig_Show();
        }
    }
    //[MonoModPatch("global::Session")]
    //public class patch_Session : Session
    //{
    //    //Dungeon Dun;
    //    //bool ExitSent = false;
    //    public extern void orig_Update();
    //    public override void Update()
    //    {
    //        Main.Log("Wowee!");
    //        orig_Update();
    //        // Add handling for keys and such!
    //        //if (Dun == null && !ExitSent)
    //        //{
    //        //    try
    //        //    {
    //        //        Main.Log("Attempting to find a Dungeon...");
    //        //        Dun = SingletonManager.Get<Dungeon>(true);
    //        //        Main.Log("Found a Dungeon! Attempting to find the exit...");
    //        //        string s = GetPathToExit("", Dun.StartRoom);
    //        //        Main.Log("Attempting to place message on display...");
    //        //        Dun.EnqueueNotification(s, null, "MajorModule_Major0001_LVL1", true, "MajorModule_Major0001_LVL1");
    //        //        Main.Log("Success!");
    //        //        ExitSent = true;
    //        //    }
    //        //    catch (NullReferenceException e)
    //        //    {
    //        //        Main.Log("A NullReference Happened - Try entering a game or waiting a little while for it to load.");
    //        //    }
    //        //}
    //    }
    //    private static string GetPathToExit(string outp, Room current)
    //    {
    //        if (current.StaticRoomEvent == RoomEvent.Exit)
    //        {
    //            Main.Log("Found exit! " + outp);
    //            return outp + "ExitR: [" + current.Depth + ", " + current.CenterPosition + "]";
    //        }
    //        string o = outp + "R: [" + current.Depth + ", " + current.CenterPosition + "] ";
    //        //Log(o);
    //        foreach (Room r in current.AdjacentRooms)
    //        {
    //            if (o.IndexOf("R: [" + r.Depth + ", " + r.CenterPosition + "]") != -1)
    //            {
    //                // Already have this within the string
    //                continue;
    //            }
    //            string o2 = GetPathToExit(o, r);
    //            if (o2.IndexOf("ExitR") != -1)
    //            {
    //                return o2;
    //            }
    //        }
    //        return o;
    //    }
    //}
}
