using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedFinder_Mod
{
    public class SeedData
    {
        public int DungeonSeed;
        public int RandomGeneratorSeed;
        public int UnityEngineSeed;
        public SeedData(int d, int r, int u)
        {
            DungeonSeed = d;
            RandomGeneratorSeed = r;
            UnityEngineSeed = u;
        }
    }
    public class Util
    {
        public static void SetSeed(SeedData data)
        {
            new DynData<DungeonGenerator2>(SingletonManager.Get<DungeonGenerator2>(false)).Set("randomSeed", data.DungeonSeed);
            RandomGenerator.SetSeed(data.RandomGeneratorSeed);
            UnityEngine.Random.seed = data.UnityEngineSeed;
        }
        public static int GetExitDepth()
        {
            Room exitRoom = SingletonManager.Get<Dungeon>(false).ExitRoom;
            if (exitRoom != null && exitRoom.Depth != 0)
            {
                // Possibly invalid, need to test to see what happens with exitRoom.Depth = 0
                return exitRoom.Depth;
            }
            return GetRoomList().Find((Room r) => { return r.IsExitRoom; }).Depth;
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
