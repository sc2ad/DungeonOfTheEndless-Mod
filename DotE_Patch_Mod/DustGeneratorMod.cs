using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotE_Patch_Mod
{
    class DustGeneratorMod : PartialityMod
    {
        ScadMod mod = new ScadMod();
        public override void Init()
        {
            mod.path = @"DustGenerator_log.txt";
            mod.config = @"DustGenerator_config.txt";
            mod.default_config = "# Modify this file to change various settings of the DustGenerator Mod for DotE.\n" + mod.default_config;
            mod.Initalize();

            // Setup default values for config
            mod.Values.Add("DustPerDoor", "10.0");
            mod.Values.Add("DustFromProducing", "False");
            mod.Values.Add("DustFromRoom", "False");

            mod.ReadConfig();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                On.Dungeon.GetDustProd += Dungeon_GetDustProd;
                On.Room.Open += Room_Open;
            }
        }

        private void Room_Open(On.Room.orig_Open orig, Room self, Door openingDoor, bool ignoreVisibility)
        {
            if (Convert.ToBoolean(mod.Values["DustFromRoom"]))
            {
                // You can reuse a DynData wrapper for multiple get / set operations on the same object
                new DynData<Room>(self).Set<int>("DustLootAmount", (int)Convert.ToDouble(mod.Values["DustPerDoor"])); // Sets the dust value of this room to 10
                mod.Log("Attempting to spawn: " + self.DustLootAmount + " dust in room!");
                orig(self, openingDoor, ignoreVisibility);
                return;
            }
            orig(self, openingDoor, ignoreVisibility);
            mod.Log("Using default room collection: " + self.DustLootAmount);
        }

        private float Dungeon_GetDustProd(On.Dungeon.orig_GetDustProd orig, Dungeon self)
        {
            if (Convert.ToBoolean(mod.Values["DustFromProducing"]))
            {
                mod.Log("Attempting to Produce: " + Convert.ToDouble(mod.Values["DustPerDoor"]) + " dust!");
                orig(self);
                return (float)Convert.ToDouble(mod.Values["DustPerDoor"]);
            }
            mod.Log("Using default dust production..." + orig(self));
            return orig(self);
        }
    }
}
