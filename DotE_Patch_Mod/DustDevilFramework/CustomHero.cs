using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Simulation;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustDevilFramework
{
    public abstract class CustomHero : ScadMod
    {
        // For Localization/GUI data

        public abstract Amplitude.StaticString GetName();
        public abstract Amplitude.StaticString GetRealName();
        public abstract Amplitude.StaticString GetIconName();
        public abstract Amplitude.StaticString GetAnimationName();
        public abstract string GetRealDescription();
        public abstract string GetRealFirstName();
        public abstract string GetIntro1();
        public abstract string GetIntro2();
        public abstract string GetIntro3();
        public abstract string GetArchetype();
        public abstract string GetDoorQuote1();
        public abstract string GetDoorQuote2();
        public abstract string GetDoorQuote3();
        public abstract string GetDoorQuote4();
        public abstract string GetWoundedQuote1();
        public abstract string GetWoundedQuote2();
        public abstract string GetCrystalQuote1();
        public abstract string GetCrystalQuote2();
        public abstract string GetRepairQuote1();
        public abstract string GetRepairQuote2();
        // Base Data

        public abstract float GetRecruitCost();
        public abstract AITargetType GetTargetType();
        public abstract string GetAttackType();
        public abstract int GetUnlockLevelCount();
        public abstract HeroConfig.HeroFaction GetFaction();
        public abstract EquipmentSlotConfig GetWeaponSlot();
        public abstract EquipmentSlotConfig GetSecondSlot();
        public abstract EquipmentSlotConfig GetThirdSlot();
        // Levelup Data

        // Start Screen Data
        public abstract bool IsKnown(); // Returns whether you start with the character unlocked or have to find the character through random encounters

        // Should definitely by 15+ long. Each one should have a unique name, among lots of other stuff...
        // Check xml for more details on how to structure these.
        // Keys are the level numbers, first string item should be food cost, remaining should be skills (if any)
        public abstract Dictionary<int, string[]> GetLevelUpData();
        // This simulation descriptor describes the actual base of the hero (the object itself, if you will)
        public abstract SimulationDescriptor GetSimulationDescriptor();
        // Need to return 15 different sim objects for the 15 different levels!
        // Some of them can actually be null, however!
        // This is where the stats for leveling up get calculated
        public abstract SimulationDescriptor[] GetLevelupSimulationDescriptors();

        private bool displayedData = false;

        public CustomHero()
        {
            name = GetRealName();
        }

        public void Initialize()
        {
            path = GetRealName() + "_log.txt";
            config = GetRealName() + "_config.txt";
            default_config = "# Modify this file to change various settings of the " + GetRealName() + " mod for DotE.\n" + default_config;
            base.Initialize();

            ReadConfig();
            if (Convert.ToBoolean(Values["Enabled"]))
            {
                Log("Attempting to create Localization changes!");
                CreateLocalizationChanges();
                Log("Attempting to create GUI changes!");
                CreateGUIChanges();
            }
            else
            {
                RemoveLocalizationChanges();
                RemoveGUIChanges();
                Log("Removed local changes!");
            }
            Log("Initialized!");
        }
        public void Load()
        {
            base.Load();
            if (Convert.ToBoolean(Values["Enabled"]))
            {
                On.SpriteAnimationRuntime2.OverrideClipsFromPath += SpriteAnimationRuntime2_OverrideClipsFromPath;
                On.Session.Update += Session_Update;
                On.UserProfile.Load += UserProfile_Load;
                On.UserProfile.GetSelectableHeroes += UserProfile_GetSelectableHeroes;
                On.Hero.ModifyLocalActiveHeroes += Hero_ModifyLocalActiveHeroes;
            }
        }

        private void Hero_ModifyLocalActiveHeroes(On.Hero.orig_ModifyLocalActiveHeroes orig, bool add, Hero hero)
        {
            Log("Attempting to add?: " + add + " hero: " + hero.LocalizedName + " to localActiveHeroes!");
            orig(add, hero);
        }

        private void UserProfile_Load(On.UserProfile.orig_Load orig)
        {
            try
            {
                AddToDatabase();
            }
            catch (ArgumentException e)
            {
                // It already exists!
            }
            catch (NullReferenceException e)
            {
                // Database doesn't exist yet!
            }
            Log("UserProfile Load Called! With IsKnown: " + IsKnown());
            Log("UserProfile post orig Call! With IsKnown: " + IsKnown());
            if (IsKnown())
            {
                Log("Attempting to set the character as Unlocked!");
                // Find the matching HeroGameStatsData
                GameConfig config = GameConfig.GetGameConfig();
                List<string> unlockedHeroes = config.InitUnlockedHeroes.ToList();
                unlockedHeroes.Add("Hero_" + GetName());
                new DynData<GameConfig>(config).Set("InitUnlockedHeroes", unlockedHeroes.ToArray());
                //HeroGameStatsData old = UserProfile.Data.HeroesGameStats[num];
                //old.Status = HeroStatus.Unlocked;
                //UserProfile.Data.HeroesGameStats[num] = old;
                //Log("Character at index: " + num + " with ConfigName: " + old.ConfigName + " should now be set to Unlocked!");
                Log("The Hero: " + GetName() + " has been added to the unlockedHeroes list!");
            }
            else
            {
                Log("The Hero: " + GetName() + " will be created as an unknown hero!");
            }
            orig();
        }

        private bool AddToDatabase()
        {
            SimulationDescriptor desc;
            Databases.GetDatabase<SimulationDescriptor>(false).TryGetValue(GetSimulationDescriptor().Name, out desc);
            if (desc != null)
            {
                return false;
            }
            Log("Attempting to add simulationdescriptors to database! With name: " + GetSimulationDescriptor().Name);
            Databases.GetDatabase<SimulationDescriptor>(false).Add(GetSimulationDescriptor());
            Log("Added main simdesc");
            foreach (HeroLevelConfig h in GetHeroLevelConfigs())
            {
                Databases.GetDatabase<HeroLevelConfig>(false).Add(h);
            }
            Log("Added LevelConfigs to DB");
            foreach (SimulationDescriptor d in GetLevelupSimulationDescriptors())
            {
                Databases.GetDatabase<SimulationDescriptor>(false).Add(d);
            }
            Log("Added LevelUp SimDescriptors to DB");
            Databases.GetDatabase<HeroConfig>(false).Add(GetHeroConfig());
            Log("Added HeroConfig to DB");
            Databases.GetDatabase<AIConfig>(false).Add(GetAIConfig());
            Log("Added AIConfig to DB");
            return true;
        }

        private HeroGameStatsData[] UserProfile_GetSelectableHeroes(On.UserProfile.orig_GetSelectableHeroes orig, bool hiddenHeroesOnly)
        {
            HeroConfig[] values = Databases.GetDatabase<HeroConfig>(false).GetValues();
            foreach (HeroConfig heroConfig in values)
            {
                Log("Loaded HeroConfig with name: " + heroConfig.Name + " while GettingSelectableHeroes");
                if (heroConfig.Name == "Hero_" + GetName() || heroConfig.Name == "Hero_H0001")
                {
                    Log("Found an important Hero!");
                    Log("Displaying Data: ");
                    Log("AITarget: " + heroConfig.AITargetType);
                    Log("AIConfig:");
                    var ai = Databases.GetDatabase<AIConfig>(false).GetValue(heroConfig.Name);
                    if (ai != null)
                    {
                        Log("- Name: " + ai.Name);
                        Log("- AI TargetInteractionConfigs:");
                        foreach (AITargetInteractionConfig c in ai.AITargetInteractionConfigs)
                        {
                            Log("-- TargetType: " + c.XmlSerializableTargetType + " Interaction: " + c.Interaction);
                        }
                    }
                    Log("Archetype: " + heroConfig.Archetype);
                    Log("AttackType: " + heroConfig.AttackType);
                    Log("Equipment Slots:");
                    foreach (EquipmentSlotConfig e in heroConfig.EquipmentSlots)
                    {
                        Log("- " + e.CategoryName + ", " + e.TypeName);
                    }
                    Log("Faction: " + heroConfig.Faction);
                    Log("Damages Stat: " + heroConfig.GetDamagesStat());
                    Log("SimObj: " + heroConfig.GetHeroSimObj());
                    IDatabase<SimulationDescriptor> simDescDatabase = SimMonoBehaviour.GetSimDescDatabase();
                    SimulationDescriptor hDesc = simDescDatabase.GetValue("Hero");
                    SimulationDescriptor desc = simDescDatabase.GetValue(heroConfig.Name);

                    //Log("Hero Descriptor:");
                    //Log("Modifiers:");
                    //foreach (SimulationModifierDescriptor d in hDesc.SimulationModifierDescriptors)
                    //{
                    //    Log("- " + d.TargetPropertyName + " With operation: " + d.Operation);
                    //}
                    //Log("Properties:");
                    //foreach (SimulationPropertyDescriptor p in hDesc.SimulationPropertyDescriptors)
                    //{
                    //    Log("- " + p.Name + ": " + p.BaseValue);
                    //}

                    Log("Personal Descriptor:");
                    Log("Name, Type: " + desc.Name + ", " + desc.Type);
                    Log("Modifiers:");
                    if (desc.SimulationModifierDescriptors != null)
                    {
                        foreach (SimulationModifierDescriptor d in desc.SimulationModifierDescriptors)
                        {
                            if (d == null)
                            {
                                continue;
                            }
                            Log("- " + d.TargetPropertyName + " With operation: " + d.Operation);
                        }
                    }
                    Log("Properties:");
                    if (desc.SimulationPropertyDescriptors != null)
                    {
                        foreach (SimulationPropertyDescriptor p in desc.SimulationPropertyDescriptors)
                        {
                            if (p == null)
                            {
                                continue;
                            }
                            Log("- " + p.Name + ": " + p.BaseValue);
                        }
                    }

                    Log("Life Stat: " + heroConfig.GetLifeStat());
                    Log("Random Weight: " + heroConfig.GetRandomSelectionWeight());
                    Log("Speed Stat: " + heroConfig.GetSpeedStat());
                    Log("Wit Stat: " + heroConfig.GetWitStat());
                    if (heroConfig.IntroDialogs != null)
                    {
                        Log("Intro Dialoges:");
                        foreach (DialogConfig c in heroConfig.IntroDialogs)
                        {
                            if (c == null)
                            {
                                continue;
                            }
                            Log("- " + c.Name + ": " + c.Text);
                        }
                    }
                    Log("Event Active: " + heroConfig.IsCommunityEventActive());
                    Log("Is Event Hero: " + heroConfig.IsCommunityEventHero());
                    Log("Is Hidden: " + heroConfig.IsHidden());
                    Log("Name: " + heroConfig.Name);
                    Log("Recruit Base Cost: " + heroConfig.RecruitmentFoodCost);
                    Log("Situation Dialogs:");
                    if (heroConfig.SituationDialogCount != null)
                    {
                        foreach (Amplitude.StaticString s in heroConfig.SituationDialogCount.Keys)
                        {
                            if (s == null)
                            {
                                continue;
                            }
                            Log("- " + s + ": " + heroConfig.SituationDialogCount[s]);
                        }
                    }
                    Log("Sprite animations path: " + heroConfig.SpriteAnimationsPath);
                    Log("Unlock Level Count: " + heroConfig.UnlockLevelCount);
                }
            }
            return orig(hiddenHeroesOnly);
        }

        private void SpriteAnimationRuntime2_OverrideClipsFromPath(On.SpriteAnimationRuntime2.orig_OverrideClipsFromPath orig, SpriteAnimationRuntime2 self, string clipsPath, bool restoreOriginalControllerFirst)
        {
            if (clipsPath.Contains(GetName()))
            {
                Log("Using SpriteName for animations!");
                // Need to use impersonation here.
                clipsPath = "SpriteAnimations/Hero/" + GetAnimationName();
            }
            orig(self, clipsPath, restoreOriginalControllerFirst);
        }

        private void Session_Update(On.Session.orig_Update orig, Session self)
        {
            if (!displayedData)
            {
                Log("Number of Active Heroes: " + Hero.LocalPlayerActiveRecruitedHeroes.Count);
                foreach (Hero h in Hero.LocalPlayerActiveRecruitedHeroes)
                {
                    try
                    {
                        Log("Entering a check with Name: " + h.LocalizedName);
                        SimulationDescriptor dbdescriptorByName = SimMonoBehaviour.GetDBDescriptorByName(h.Config.Name);
                        Log("DBDescriptorByName: " + dbdescriptorByName.Name);
                        var obj = h.GetSimDescriptorByType(SimulationProperties.SimDescTypeHero);
                        Log("Name: " + obj.Name + " Type: " + obj.Type);
                        Log("Modifiers:");
                        if (obj.SimulationModifierDescriptors != null)
                        {
                            foreach (SimulationModifierDescriptor m in obj.SimulationModifierDescriptors)
                            {
                                Log("- " + m.TargetPropertyName);
                            }
                        }
                        Log("Properties:");
                        if (obj.SimulationPropertyDescriptors != null)
                        {
                            foreach (SimulationPropertyDescriptor d in obj.SimulationPropertyDescriptors)
                            {
                                Log("- " + d.Name + ": " + d.BaseValue);
                            }
                        }
                        Log("Level 1 Name: " + h.GetLevelDescriptorName(1));
                        displayedData = true;
                    }
                    catch (NullReferenceException e)
                    {
                        // Thats ok for now
                    }
                }
            }
            orig(self);
        }
        public AIConfig GetAIConfig()
        {
            AIConfig config = Databases.GetDatabase<AIConfig>(false).GetValue("Hero_H0001");
            config.XmlSerializableName = "Hero_" + GetName();
            return config;
        }
        public HeroConfig GetHeroConfig()
        {
            HeroConfig config = new HeroConfig();
            DynData<HeroConfig> d = new DynData<HeroConfig>(config);
            d.Set<Amplitude.StaticString>("Name", "Hero_" + GetName());
            d.Set<float>("RecruitmentFoodCost", GetRecruitCost());
            d.Set<AITargetType>("AITargetType", GetTargetType());
            d.Set<string>("AttackType", GetAttackType());
            d.Set<string>("Archetype", "%Hero_" + GetName() + "_Archetype");
            d.Set<int>("UnlockLevelCount", GetUnlockLevelCount());
            d.Set<HeroConfig.HeroFaction>("Faction", GetFaction());
            d.Set<DialogConfig[]>("IntroDialogs", GetIntroDialogs());
            d.Set<EquipmentSlotConfig[]>("EquipmentSlots", GetEquipmentSlots());
            return config;
        }
        public DialogConfig[] GetIntroDialogs()
        {
            DialogConfig one = new DialogConfig();
            DynData<DialogConfig> d1 = new DynData<DialogConfig>(one);
            d1.Set<Amplitude.StaticString>("Name", "Intro1");
            d1.Set<string>("Text", "%Hero_" + GetName() + "_Intro1");
            DialogConfig two = new DialogConfig();
            DynData<DialogConfig> d2 = new DynData<DialogConfig>(two);
            d2.Set<Amplitude.StaticString>("Name", "Intro2");
            d2.Set<string>("Text", "%Hero_" + GetName() + "_Intro2");
            DialogConfig three = new DialogConfig();
            DynData<DialogConfig> d3 = new DynData<DialogConfig>(three);
            d3.Set<Amplitude.StaticString>("Name", "Intro3");
            d3.Set<string>("Text", "%Hero_" + GetName() + "_Intro3");
            return new DialogConfig[] { one, two, three };
        }
        public EquipmentSlotConfig[] GetEquipmentSlots()
        {
            List<EquipmentSlotConfig> lst = new List<EquipmentSlotConfig>();
            if (GetWeaponSlot() != null)
            {
                lst.Add(GetWeaponSlot());
            }
            if (GetSecondSlot() != null)
            {
                lst.Add(GetSecondSlot());
            }
            if (GetThirdSlot() != null)
            {
                lst.Add(GetThirdSlot());
            }
            return lst.ToArray();
        }

        public HeroLevelConfig[] GetHeroLevelConfigs()
        {
            List<HeroLevelConfig> lst = new List<HeroLevelConfig>();
            foreach (int level in GetLevelUpData().Keys)
            {
                HeroLevelConfig config = new HeroLevelConfig();
                DynData<HeroLevelConfig> d = new DynData<HeroLevelConfig>(config);
                d.Set<Amplitude.StaticString>("Name", "Hero_" + GetName() + "_LVL" + level);
                d.Set<float>("FoodCost", (float)Convert.ToDouble(GetLevelUpData()[level][0]));
                string[] skills = new string[GetLevelUpData()[level].Length - 1];
                for (int i = 1; i < GetLevelUpData()[level].Length; i++)
                {
                    skills[i - 1] = GetLevelUpData()[level][i];
                }
                d.Set<string[]>("Skills", skills);
                lst.Add(config);
            }
            return lst.ToArray();
        }

        public void CreateLocalizationChanges()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf("%Hero_" + GetName()) != -1)
                {
                    return;
                }
                if (line.IndexOf("%Hero_H0028_Archetype") != -1)
                {
                    linesLst.Insert(i + 1, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_Title\">" + GetRealName() + "</LocalizationPair>");
                    linesLst.Insert(i + 2, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_Biography\">" + GetRealDescription() + "</LocalizationPair>");
                    linesLst.Insert(i + 3, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_FirstName\">" + GetRealFirstName() + "</LocalizationPair>");
                    linesLst.Insert(i + 4, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_Intro1\">" + GetIntro1() + "</LocalizationPair>");
                    linesLst.Insert(i + 5, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_Intro2\">" + GetIntro2() + "</LocalizationPair>");
                    linesLst.Insert(i + 6, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_Intro3\">" + GetIntro3() + "</LocalizationPair>");
                    linesLst.Insert(i + 7, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_Archetype\">" + GetArchetype() + "</LocalizationPair>");
                    linesLst.Insert(i + 8, "  <LocalizationPair Name=\"%HeroName_" + GetName() + "\">" + GetRealName() + "</LocalizationPair>");
                    linesLst.Insert(i + 9, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_OpenDoor1\">" + GetDoorQuote1() + "</LocalizationPair>");
                    linesLst.Insert(i +10, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_OpenDoor2\">" + GetDoorQuote2() + "</LocalizationPair>");
                    linesLst.Insert(i +11, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_OpenDoor3\">" + GetDoorQuote3() + "</LocalizationPair>");
                    linesLst.Insert(i +12, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_OpenDoor4\">" + GetDoorQuote4() + "</LocalizationPair>");
                    linesLst.Insert(i +13, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_Wounded1\">" + GetWoundedQuote1() + "</LocalizationPair>");
                    linesLst.Insert(i +14, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_Wounded2\">" + GetWoundedQuote2() + "</LocalizationPair>");
                    linesLst.Insert(i +15, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_RemoveCrystal1\">" + GetCrystalQuote1() + "</LocalizationPair>");
                    linesLst.Insert(i +16, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_RemoveCrystal2\">" + GetCrystalQuote2() + "</LocalizationPair>");
                    linesLst.Insert(i +17, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_RepairModule1\">" + GetRepairQuote1() + "</LocalizationPair>");
                    linesLst.Insert(i +18, "  <LocalizationPair Name=\"%Hero_" + GetName() + "_SituationDialog_RepairModule2\">" + GetRepairQuote2() + "</LocalizationPair>");
                    linesLst.Insert(i +19, "  <LocalizationPair Name=\"%Hero_" + GetName() + "\">" + GetRealName() + "</LocalizationPair>");
                }
            }
            System.IO.File.WriteAllLines(@"Public\Localization\english\ED_Localization_Locales.xml", linesLst.ToArray());
        }

        public void RemoveLocalizationChanges()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < linesLst.Count; i++)
            {
                string s = lines[i];
                if (s.IndexOf("%Hero_" + GetName() + "_Title") != -1)
                {
                    int q = i;
                    for (int j = i; j < linesLst.Count; j++)
                    {
                        if (linesLst[j].IndexOf("%Hero_" + GetName() + "\"") != -1)
                        {
                            // This is the stopping point for deleting.
                            q = j;
                        }
                    }
                    for (int l = i; l <= q; l++)
                    {
                        linesLst.RemoveAt(i);
                    }
                }
            }
            System.IO.File.WriteAllLines(@"Public\Localization\english\ED_Localization_Locales.xml", linesLst.ToArray());
        }
        public void CreateGUIChanges()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Gui\GuiElements_Hero.xml");
            foreach (string s in lines)
            {
                if (s.IndexOf("%Hero_" + GetName()) != -1)
                {
                    return;
                }
            }
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf("%Hero_H0028_Biography") != -1)
                {
                    linesLst.Insert(i + 6, "  <GuiElement Name=\"Hero_" + GetName() + "\">");
                    linesLst.Insert(i + 7, "    <Title>%Hero_" + GetName() + "</Title>");
                    linesLst.Insert(i + 8, "    <Description>%Hero_" + GetName() + "_Biography</Description>");
                    linesLst.Insert(i + 9, "    <Icons>");
                    linesLst.Insert(i +10, "      <Icon Size=\"Small\" Path=\"GUI/DynamicBitmaps/Heroes/Hero_" + GetIconName() + "\"/>");
                    linesLst.Insert(i +11, "      <Icon Size=\"Large\" Path=\"GUI/DynamicBitmaps/Heroes/Hero_" + GetIconName() + "_Large\"/>");
                    linesLst.Insert(i +12, "    </Icons>");
                    linesLst.Insert(i +13, "  </GuiElement>");
                }
            }
            System.IO.File.WriteAllLines(@"Public\Gui\GuiElements_Hero.xml", linesLst.ToArray());
        }
        public void RemoveGUIChanges()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Gui\GuiElements_Hero.xml");
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < lines.Length; i++)
            {
                string s = lines[i];
                if (s.IndexOf("  <GuiElement Name=\"Hero_" + GetName() + "\">") != -1)
                {
                    int q = i;
                    for (int j = i; j < lines.Length; j++)
                    {
                        if (lines[j].IndexOf("  </GuiElement>") != -1)
                        {
                            // This is the stopping point for deleting.
                            q = j;
                        }
                    }
                    for (int l = i; l <= q; l++)
                    {
                        linesLst.RemoveAt(i);
                    }
                }
            }
            System.IO.File.WriteAllLines(@"Public\Gui\GuiElements_Hero.xml", linesLst.ToArray());
        }
    }
}
