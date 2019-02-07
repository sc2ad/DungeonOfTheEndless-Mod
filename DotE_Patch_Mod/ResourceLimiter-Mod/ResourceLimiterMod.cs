using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceLimiter_Mod
{
    using Partiality.Modloader;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DustDevilFramework;

    namespace DotE_Combo_Mod
    {
        class ResourceLimiterMod : PartialityMod
        {
            ScadMod mod = new ScadMod("ResourceLimiter", typeof(ResourceLimiterSettings), typeof(ResourceLimiterMod));
            public override void Init()
            {
                mod.PartialityModReference = this;
                mod.Initialize();

                mod.settings.ReadSettings();

                mod.Log("Initialized!");
            }
            public override void OnLoad()
            {
                mod.Load();
                if (mod.settings.Enabled)
                {
                    On.Dungeon.GetFoodProd += Dungeon_GetFoodProd;
                    On.Dungeon.GetIndustryProd += Dungeon_GetIndustryProd;
                    On.Dungeon.GetScienceProd += Dungeon_GetScienceProd;
                }
            }
            public void UnLoad()
            {
                mod.UnLoad();
                On.Dungeon.GetFoodProd -= Dungeon_GetFoodProd;
                On.Dungeon.GetIndustryProd -= Dungeon_GetIndustryProd;
                On.Dungeon.GetScienceProd -= Dungeon_GetScienceProd;
            }

            private float CalculateNew(Dungeon self, float old)
            {
                if (Mob.ActiveMobs.Count >= 1)
                {
                    // Can't use GameState, as it is called AFTER the door is started to open, so it will always be Action Phase
                    // This should mean that there are mobs!
                    // Unless... Every time you open a door (before eco comes in) you are stuck in the Action phase...
                    mod.Log("It is the Action Phase! (Hopefully there are mobs on the screen!)");
                    if ((mod.settings as ResourceLimiterSettings).Use == "Percentage")
                    {
                        return (float)Math.Round(old * (mod.settings as ResourceLimiterSettings).Percentage, 1);
                    }
                    else if ((mod.settings as ResourceLimiterSettings).Use == "FlatRate")
                    {
                        return (float)(mod.settings as ResourceLimiterSettings).FlatRate;
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

}
