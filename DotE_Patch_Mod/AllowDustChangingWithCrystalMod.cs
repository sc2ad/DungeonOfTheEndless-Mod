using DustDevilFramework;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotE_Combo_Mod
{
    class AllowDustChangingWithCrystalMod : PartialityMod
    {
        ScadMod mod = new ScadMod();
        public override void Init()
        {
            mod.path = @"DustAfterCrystal_log.txt";
            mod.config = @"DustAfterCrystal_config.txt";
            mod.default_config = "# Modify this file to change various settings of the DustAfterCrystal Mod for DotE.\n" + mod.default_config;
            mod.Initialize();

            // Setup default values for config
            

            mod.ReadConfig();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                On.Room.CanBePowered_refString_bool_bool_bool_bool_bool += Room_CanBePowered;
                On.Room.CanBeUnpowered += Room_CanBeUnpowered;
            }
        }

        private bool Room_CanBeUnpowered(On.Room.orig_CanBeUnpowered orig, Room self, bool checkCrystalState, bool checkPoweringPlayer, bool checkPowerChangeCooldown, bool ignoreShipConfig, bool displayError)
        {
            if (checkCrystalState)
            {
                mod.Log("Making sure checkCrystalState is false...");
                return orig(self, false, checkPoweringPlayer, checkPowerChangeCooldown, ignoreShipConfig, displayError);
            }
            return orig(self, checkCrystalState, checkPoweringPlayer, checkPowerChangeCooldown, ignoreShipConfig, displayError);
        }

        private bool Room_CanBePowered(On.Room.orig_CanBePowered_refString_bool_bool_bool_bool_bool orig, Room self, out string errorNotif, bool displayErrorNotif, bool checkCrystalState, bool checkInitialization, bool ignoreOpeningDoorsForPowerChainCheck, bool checkPowerChain)
        {
            errorNotif = null;
            if (checkCrystalState)
            {
                mod.Log("Making sure checkCrystalState is false...");
                return orig(self, out errorNotif, displayErrorNotif, false, checkInitialization, ignoreOpeningDoorsForPowerChainCheck, checkPowerChain);
            }
            return orig(self, out errorNotif, displayErrorNotif, checkCrystalState, checkInitialization, ignoreOpeningDoorsForPowerChainCheck, checkPowerChain);
        }
    }
}
