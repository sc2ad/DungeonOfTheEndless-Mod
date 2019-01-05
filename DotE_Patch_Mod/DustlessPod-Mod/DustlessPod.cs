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

            mod.settings.ReadSettings();
        }
        public override void OnLoad()
        {
            mod.Load();

            if (mod.settings.Enabled)
            {
                On.Mob.OnDeath += Mob_OnDeath;
                On.Room.Open += Room_Open;
            }
        }
        public void UnLoad()
        {
            mod.UnLoad();
            On.Mob.OnDeath -= Mob_OnDeath;
            On.Room.Open -= Room_Open;
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
                self.SetSimPropertyBaseValue(SimulationProperties.DustLootProbability, (mod.settings as DustlessPodSettings).DustLootProbability);
                self.SetSimPropertyBaseValue(SimulationProperties.DustLootAmountMin, (mod.settings as DustlessPodSettings).MinDustLoot);
                self.SetSimPropertyBaseValue(SimulationProperties.DustLootAmountMax, (mod.settings as DustlessPodSettings).MaxDustLoot);
            }
            orig(self, attackerOwnerPlayerID);
        }
    }
}
