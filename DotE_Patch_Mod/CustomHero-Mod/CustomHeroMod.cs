using Amplitude.Unity.Framework;
using Amplitude.Unity.Gui;
using Amplitude.Unity.Simulation;
using DustDevilFramework;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomHero_Mod
{
    class CustomHeroMod : PartialityMod
    {
        ScadMod mod = new ScadMod();

        Dictionary<string, string> Ranks = new Dictionary<string, string>();

        public override void Init()
        {
            mod.path = @"CustomHero_log.txt";
            mod.config = @"CustomHero_config.txt";
            mod.default_config = "# Modify this file to change various settings of the CustomHero Mod for DotE.\n" + mod.default_config;
            mod.Initialize();

            // Setup default values for config
            mod.Values.Add("HeroToReplace", "Skroig");
            mod.Values.Add("HeroName", "Skrog");
            mod.Values.Add("Hero HP", "1000");
            mod.Values.Add("Speed", "80");
            mod.Values.Add("LogHeroData", "True");
            mod.Values.Add("Path to Ranks.txt", "Ranks.txt");

            mod.ReadConfig();
            ReadRanks();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                On.Hero.Init += Hero_Init;
                On.Hero.ModifyLocalActiveHeroes += Hero_ModifyLocalActiveHeroes;
            }
        }

        private void Hero_ModifyLocalActiveHeroes(On.Hero.orig_ModifyLocalActiveHeroes orig, bool add, Hero hero)
        {
            // This should be called every time a hero is given/taken
            orig(add, hero);
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                if (mod.Values["HeroToReplace"] == hero.LocalizedName)
                {
                    // We are matching Heroes! Start to change data!
                    SetHeroStats(hero);
                    //self.HealthCpnt.SetHealth((float)Convert.ToDouble(mod.Values["Hero HP"]));
                }
                SetHeroRankName(hero);
            }
        }

        private void ReadRanks()
        {
            if (System.IO.File.Exists(mod.Values["Path to Ranks.txt"]))
            {
                mod.Log("Reading Ranks.txt...");
                string[] lines = System.IO.File.ReadAllLines(mod.Values["Path to Ranks.txt"]);

                foreach (string line in lines)
                {
                    if (line.StartsWith("#") || line.IndexOf(": ") == -1)
                    {
                        continue;
                    }
                    string[] spl = line.Split(new string[] { ": " }, StringSplitOptions.None);
                    string value = spl[1].Trim();
                    Ranks[spl[0]] = value;
                    mod.Log("Read hero: " + spl[0] + " with rank: " + value + " from " + mod.Values["Path to Ranks.txt"]);
                }
            } else
            {
                mod.Log("Could not find Ranks.txt... Continuing!");
            }
        }

        private void Hero_Init(On.Hero.orig_Init orig, Hero self, ulong ownerPlayerID, Amplitude.StaticString heroDescName, Room spawnRoom, bool isRecruited, bool registerRecruitment, int initLevel, int unlockLevel, bool hasOperatingBonus, Dictionary<Amplitude.StaticString, List<ItemPersistentData>> initItemsByCategory, bool displayRecruitmentDialog, bool consumeLevelUpFood, bool updateDiscoverableHeroPool, int floorRecruited, Amplitude.StaticString[] permanentDescriptors, bool isStartingHero, int currentRespawnRoomCount, bool recruitable)
        {
            orig(self, ownerPlayerID, heroDescName, spawnRoom, isRecruited, registerRecruitment, initLevel, unlockLevel, hasOperatingBonus, initItemsByCategory, displayRecruitmentDialog, consumeLevelUpFood, updateDiscoverableHeroPool, floorRecruited, permanentDescriptors, isStartingHero, currentRespawnRoomCount, recruitable);
            if (Convert.ToBoolean(mod.Values["LogHeroData"]))
            {
                LogHeroData(self, heroDescName);
            }
            if (mod.Values["HeroToReplace"] == self.LocalizedName)
            {
                // We are matching Heroes! Start to change data!
                SetHeroStats(self);
                //self.HealthCpnt.SetHealth((float)Convert.ToDouble(mod.Values["Hero HP"]));
            }
            SetHeroRankName(self);
        }

        private void SetHeroStats(Hero self)
        {
            mod.Log("Found hero with name: " + self.LocalizedName + " Attempting to change data!");
            new DynData<Hero>(self).Set<string>("LocalizedName", mod.Values["HeroName"]);
            //self.HealthCpnt.PermanantHealthMalus = (float)Convert.ToDouble(mod.Values["Hero HP"]);
            self.GetSimObj().SetPropertyBaseValue("MaxHealth", (float)Convert.ToDouble(mod.Values["Hero HP"]) - self.GetSimObj().GetPropertyValue("MaxHealth"));
            self.GetSimObj().SetPropertyBaseValue("MoveSpeed", (float)Convert.ToDouble(mod.Values["Speed"]) - self.GetSimObj().GetPropertyValue("MoveSpeed"));
            //self.RemoveSimDescriptor(SimMonoBehaviour.GetDBDescriptorByName(self.Config.Name), true);
            // Okay so now that I removed the actual SMB, I need to create a new one where the max health is mine
            self.HealthCpnt.SetHealth((float)Convert.ToDouble(mod.Values["Hero HP"]));
            mod.Log("New Name: " + self.LocalizedName + " and HP: " + self.HealthCpnt.GetHealth());
        }

        private void SetHeroRankName(Hero self)
        {
            if (Ranks.Keys.Count > 0)
            {
                foreach (string hero in Ranks.Keys)
                {
                    if (self.LocalizedName == hero)
                    {
                        new DynData<Hero>(self).Set<string>("LocalizedName", self.LocalizedName + " Rank: " + Ranks[hero]);
                        mod.Log("Changed LocalizedName to: " + self.LocalizedName);
                    }
                }
            }
        }

        private string GetSkillNameFromConfig(SkillConfig s)
        {
            IGuiService service = Services.GetService<IGuiService>();
            GuiElement guiElement;
            if (!service.GuiPanelHelper.TryGetGuiElement(s.BaseName, out guiElement))
            {
                mod.Log("Attempted to find a GuiElement, but couldn't find one with name: " + s.BaseName);
                return null;
            }
            return AgeLocalizer.Instance.LocalizeString(guiElement.Title);
        }

        private string GetSkillNameFromSkill(Skill s)
        {
            return GetSkillNameFromConfig(s.Config);
        }

        private string GetSkillNameFromSkillKey(string skillKey)
        {
            SkillConfig skc = Databases.GetDatabase<SkillConfig>(false).GetValue(skillKey);
            skc.Init();
            return GetSkillNameFromConfig(skc);
        }

        private void LogHeroData(Hero self, Amplitude.StaticString heroDescName)
        {
            mod.Log("HeroDescName: " + heroDescName + " with real name: " + self.LocalizedName);
            mod.Log("Current Level: " + self.Level);
            // Possibly important information below:
            /*
             * activeSkills, passiveSkills: Lists
             * FilteredActiveSkills: List of ActiveSkills
             * FilteredPassiveSkills: List of PassiveSkills
             * Config: Database of HeroConfig, found by using heroDescName
             * SituationDialogCount seems like dialogs that the hero says during the game
             * spriteAnim, tacticalMapElementAnim, crystalSprite: All seem to be animations/graphics
             * self.transform.position: Real position
             * heroAttackTargetCpnt: The attacker componenet, init is called on this with Config.AITargetType passed in (if it is recruited by a player)
             * mobsAggroAttackTargetCpnt: Seems to be something that determines aggro of mobs
             * levelConfigs: List<HeroLevelConfig>
             * levelUpModifiersByLevel: List<Dictionary<StaticString, float>>()
             * GetLevelDescriptorName: returns a name for a given level, used to populate the above dict
             * levelConfigs contains a list of data (including passive/active skills with unique naming)
             * leveUpModifiersByLevel contains Dictionaries created by:
             * 1. Finding a SimulationDescriptor from a SimMonoBehavior (found by using levelDescriptorName)
             * 2. Finding a simObj (self.GetSimObj())
             * 3. Iterating over each SimulationModifierDescriptor in the SimulationDescriptor.SimulationModifierDescriptors list
             * - And adding to the dictionary: TargetPropertyName of the current SimulationModifierDescriptor, current.ComputeValue(simObj, simObj, SimulationPropertyRefreshContext.GetContext(-1))
             * permanentDescriptors contain SimulationDescriptors
             * 
            */
            mod.Log("Active Skills:");
            foreach (ActiveSkill a in self.FilteredActiveSkills)
            {
                mod.Log("- " + GetSkillNameFromSkill(a) + " with desc name: " + a.name);

            }
            mod.Log("Passive Skills:");
            foreach (PassiveSkill p in self.FilteredPassiveSkills)
            {
                string label = GetSkillNameFromSkill(p);
                mod.Log("GUI Label: " + label + " with desc name: " + p.name);
                if (p.OwnerSimDesc == null)
                {
                    continue;
                }
                if (p.OwnerSimDesc.SimulationModifierDescriptors != null)
                {
                    mod.Log("- All ModifierDescriptors:");
                    foreach (SimulationModifierDescriptor d in p.OwnerSimDesc.SimulationModifierDescriptors)
                    {
                        if (d != null)
                            mod.Log("-- " + d.TargetPropertyName);
                    }
                }
                if (p.OwnerSimDesc.SimulationPropertyDescriptors != null)
                {
                    mod.Log("- All PropertyDescriptors:");
                    foreach (SimulationPropertyDescriptor d in p.OwnerSimDesc.SimulationPropertyDescriptors)
                    {
                        if (d != null)
                            mod.Log("-- " + d.Name);
                    }
                }
            }
            mod.Log("Level Configs:");
            foreach (HeroLevelConfig h in new DynData<Hero>(self).Get<List<HeroLevelConfig>>("levelConfigs"))
            {
                if (h == null)
                {
                    continue;
                }
                mod.Log("Food Cost: " + h.FoodCost + " Skills:");
                try
                {
                    foreach (string s in h.Skills)
                    {
                        //SkillConfig skc = Databases.GetDatabase<SkillConfig>(false).GetValue(s);
                        //skc.Init();
                        string label = GetSkillNameFromSkillKey(s);
                        mod.Log("- " + label + " with desc name: " + s);
                    }
                }
                catch (NullReferenceException e)
                {
                    // There are no Skills from this levelConfig
                }
            }
            mod.Log("Level Up Modifiers By Level:");
            List<Dictionary<Amplitude.StaticString, float>> list = new DynData<Hero>(self).Get<List<Dictionary<Amplitude.StaticString, float>>>("levelUpModifiersByLevel");
            foreach (Dictionary<Amplitude.StaticString, float> d in list)
            {
                if (d == null)
                {
                    continue;
                }
                mod.Log("Dictionary:");
                foreach (Amplitude.StaticString s in d.Keys)
                {
                    mod.Log("- Key: " + s + ", Value: " + d[s]);
                }
            }
            mod.Log("=======================================================================");
        }
    }
}
