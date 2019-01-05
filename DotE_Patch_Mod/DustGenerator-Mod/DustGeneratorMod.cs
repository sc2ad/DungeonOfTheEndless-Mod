using DustDevilFramework;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustGenerator_Mod
{
    class DustGeneratorMod : PartialityMod
    {
        ScadMod mod = new ScadMod("DustGenerator", typeof(DustGeneratorSettings));
        public override void Init()
        {
            mod.Initialize();

            // Setup default values for config
            if (!mod.settings.Exists())
            {
                DustGeneratorSettings temp = mod.settings as DustGeneratorSettings;
                temp.DustPerDoor = 10;
                temp.DustFromProducing = true;
                temp.DustFromRoom = false;
            }

            mod.settings.ReadSettings();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                On.Dungeon.GetDustProd += Dungeon_GetDustProd;
                On.Room.Open += Room_Open;
            }
        }
        public void UnLoad()
        {
            // Need to have a way of unloading entirety of the mod from only ScadMod (also deals with unloading partialitymod)
            mod.UnLoad();
            On.Dungeon.GetDustProd -= Dungeon_GetDustProd;
            On.Room.Open -= Room_Open;
        }

        private void Room_Open(On.Room.orig_Open orig, Room self, Door openingDoor, bool ignoreVisibility)
        {
            if ((mod.settings as DustGeneratorSettings).DustFromRoom)
            {
                // You can reuse a DynData wrapper for multiple get / set operations on the same object
                new DynData<Room>(self).Set<int>("DustLootAmount", (int)(mod.settings as DustGeneratorSettings).DustPerDoor); // Sets the dust value of this room to 10
                mod.Log("Attempting to spawn: " + self.DustLootAmount + " dust in room!");
                orig(self, openingDoor, ignoreVisibility);
                return;
            }
            orig(self, openingDoor, ignoreVisibility);
            mod.Log("Using default room collection: " + self.DustLootAmount);
        }

        private float Dungeon_GetDustProd(On.Dungeon.orig_GetDustProd orig, Dungeon self)
        {
            if ((mod.settings as DustGeneratorSettings).DustFromProducing)
            {
                mod.Log("Attempting to Produce: " + (mod.settings as DustGeneratorSettings).DustPerDoor + " dust!");
                orig(self);
                return (mod.settings as DustGeneratorSettings).DustPerDoor;
            }
            mod.Log("Using default dust production..." + orig(self));
            return orig(self);
        }
    }
}
