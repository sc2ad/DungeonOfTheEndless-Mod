using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TASTools_Mod
{
    public class Util
    {
        public static Room GetExitRoom()
        {
            Room r = SingletonManager.Get<Dungeon>(false).ExitRoom;
            if (r != null)
            {
                return r;
            }
            return GetRoomList().Find((Room room) => { return room.IsExitRoom; });
        }
        public static List<Room> GetRoomList()
        {
            List<Room> rooms = new List<Room>();
            // Recursively add all adjacent rooms that don't already exist within the list until all rooms have been added
            AddRoomRecursive(SingletonManager.Get<Dungeon>(false).StartRoom, rooms);
            return rooms;
        }
        public static void AddRoomRecursive(Room r, List<Room> rooms)
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
