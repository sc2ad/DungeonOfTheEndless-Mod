using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotE_Mod
{
    public class ModManager
    {
        public static bool Enabled { get; set; } = true;
        public static bool UseRandomRooms { get; set; } = false;

        public static int GetDoors(int low, int high)
        {
            if (Enabled)
            {
                // Do some calculation manually

            }
            return RandomGenerator.RangeInt(low, high);
        }
    }
}
