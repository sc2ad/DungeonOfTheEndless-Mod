using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustlessPod_Mod
{
    class DustlessPod : PartialityMod
    {
        DustlessPodConfig mod = new DustlessPodConfig();

        public override void Init()
        {
            mod.Initialize();

            mod.Values.Add("MinDustLoot", "1");
            mod.Values.Add("MaxDustLoot", "8");
            mod.Values.Add("DustLootProbability", "0.5");

            mod.ReadConfig();
        }
        public override void OnLoad()
        {
            mod.Load();

            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                On.Mob.OnDeath += Mob_OnDeath;
                On.Room.Open += Room_Open;
            }
        }

        private void Room_Open(On.Room.orig_Open orig, Room self, Door openingDoor, bool ignoreVisibility)
        {
            Dungeon d = SingletonManager.Get<Dungeon>(false);

            if (d.ShipName == mod.GetName())
            {
                new DynData<Room>(self).Set<int>("DustLootAmount", 0); // Sets the dust value of this room to 0
                mod.Log("Set the dust value of this room to 0!");
                orig(self, openingDoor, ignoreVisibility);
                return;
            }
            orig(self, openingDoor, ignoreVisibility);
        }

        private void Mob_OnDeath(On.Mob.orig_OnDeath orig, Mob self, ulong attackerOwnerPlayerID)
        {
            Dungeon d = SingletonManager.Get<Dungeon>(false);

            if (d.ShipName == mod.GetName())
            {
                self.SetSimPropertyBaseValue(SimulationProperties.DustLootProbability, (float)Convert.ToDouble(mod.Values["DustLootProbability"]));
                self.SetSimPropertyBaseValue(SimulationProperties.DustLootAmountMin, (float)Convert.ToDouble(mod.Values["MinDustLoot"]));
                self.SetSimPropertyBaseValue(SimulationProperties.DustLootAmountMax, (float)Convert.ToDouble(mod.Values["MaxDustLoot"]));
            }
            orig(self, attackerOwnerPlayerID);
        }
    }
}
