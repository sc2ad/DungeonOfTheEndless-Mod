using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotE_Patch_Mod
{
    class ResourceLimiterMod : PartialityMod
    {
        ScadMod mod = new ScadMod();
        public override void Init()
        {
            mod.path = @"ResourceLimiter_log.txt";
            mod.config = @"ResourceLimiter_config.txt";
            mod.default_config = "# Modify this file to change various settings of the DustGenerator Mod for DotE.\nYou must match a value exactly when changing Use? (Percentage, FlatRate)\n" + mod.default_config;
            mod.Initalize();

            // Setup default values for config
            mod.Values.Add("Use?", "Percentage");
            mod.Values.Add("Percentage", "0.75");
            mod.Values.Add("FlatRate", "3");

            mod.ReadConfig();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                On.Dungeon.GetFoodProd += Dungeon_GetFoodProd;
                On.Dungeon.GetIndustryProd += Dungeon_GetIndustryProd;
                On.Dungeon.GetScienceProd += Dungeon_GetScienceProd;
            }
        }

        private float CalculateNew(Dungeon self, float old)
        {
            if (Mob.ActiveMobs.Count >= 1)
            {
                // Can't use GameState, as it is called AFTER the door is started to open, so it will always be Action Phase
                // This should mean that there are mobs!
                // Unless... Every time you open a door (before eco comes in) you are stuck in the Action phase...
                mod.Log("It is the Action Phase! (Hopefully there are mobs on the screen!)");
                if (mod.Values["Use?"] == "Percentage")
                {
                    return (float)Math.Round(old * Convert.ToDouble(mod.Values["Percentage"]), 1);
                }
                else if (mod.Values["Use?"] == "FlatRate")
                {
                    return (float)Convert.ToDouble(mod.Values["FlatRate"]);
                }
            }
            return old;
        }

        private float Dungeon_GetScienceProd(On.Dungeon.orig_GetScienceProd orig, Dungeon self)
        {
            return CalculateNew(self, orig(self));
        }

        private float Dungeon_GetIndustryProd(On.Dungeon.orig_GetIndustryProd orig, Dungeon self)
        {
            return CalculateNew(self, orig(self));
        }

        private float Dungeon_GetFoodProd(On.Dungeon.orig_GetFoodProd orig, Dungeon self)
        {
            return CalculateNew(self, orig(self));
        }
    }
}
