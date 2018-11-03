using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amplitude;

namespace DotE_Mod
{
    public class DungeonGenerator2_Modded
    {
        public static bool Enabled { get; set; } = true;
        public static bool UseRandomRooms { get; set; } = false;
        public static bool UseRandomDust { get; set; } = true;
        public static bool UseRandomRoomDustProbability { get; set; } = true;

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

        // Add handles for RandomTemplate, StartSlot, Doors, etc.
    }
}
