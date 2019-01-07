using Amplitude.Unity.Framework;
using Amplitude.Unity.Simulation;
using Amplitude.Unity.Simulation.SimulationModifierDescriptors;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustDevilFramework
{
    public abstract class CustomItem : ScadMod
    {
        // Impersonation Config
        public abstract Amplitude.StaticString GetSpriteName();
        // Direct Configurations for ItemHeroConfig
        public abstract string GetName();
        public abstract int GetMaxCount();
        public abstract string GetAttackType();
        public abstract bool GetCannotBeUnequipped();
        public abstract string[] GetSkills();
        public abstract bool GetFlatSprite();
        public abstract bool GetDestroyOnDeath();
        public abstract int GetMaxOccurenceCount();
        // Category Parameters
        public abstract string GetCategoryName();
        public abstract string GetCategoryTypeName();
        // Finding the Item/Probability Parameters
        public abstract int GetMinLevel();
        public abstract int GetMaxLevel();
        public abstract float GetStartingProbabilityWeight();
        // Rarity Configs
        // These dictionaries MUST contain: Start, LevelMin, LevelMax, StartValue, DepthBonus, MaxValue
        public abstract Dictionary<string, int> GetCommonRarity();
        public abstract Dictionary<string, int> GetRarity0Rarity();
        public abstract Dictionary<string, int> GetRarity1Rarity();
        public abstract Dictionary<string, int> GetRarity2Rarity();
        // Localization Configs
        public abstract string GetRealName();
        public abstract string GetRealDescription();

        public CustomItem(Type settingsType)
        {
            name = GetRealName();
            settings = (CustomItemSettings)settingsType.TypeInitializer.Invoke(new object[] { name });
            this.settingsType = settingsType;
            // NEED TO FIGURE OUT A WAY OF PASSING IN THE SETTINGS TYPE
            // THIS IS SO THE SETTINGS IS PROPERLY CONSTRUCTED INTO SCADMOD
            /*
             * IDEAS:
             * 1. MAKE A CustomItemsSettings CLASS AND HAVE THIS USE THAT AS A CONSTRUCTOR
             * - THIS WOULD INCLUDE THE BASELINE SETTINGS DATA FOR CUSTOMITEMS (WHICH IS JUST ENABLED)
             * 2. USE EXISTING ModSettings CLASS, BUT NEED TO HAVE A BETTER WAY OF DEALING WITH DEFAULTS
             * - THIS IS BECAUSE DEFAULT SETTINGS ARE PAINFULLY POOR
             * - FOR EXAMPLE: PODMOD NEEDS TO HAVE DEFAULT CONFIG OF CONTAINING ASSUMEDPOD AND GENERATIONPOD
             * 
             * HOW DO I WANT TO DO THIS?
             */
        }

        public void Initialize()
        {
            base.Initialize();

            settings.ReadSettings();

            if (settings.Enabled)
            {
                Log("Attempting to create Localization changes!");
                CreateLocalizationChanges();
                Log("Attempting to create GUI changes!");
                CreateGUIChanges();
                // Should also attempt to change GuiElements_ItemHero.xml
            } else
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
            if (settings.Enabled)
            {
                On.Session.Update += Session_Update;
            }
        }
        public void UnLoad()
        {
            base.UnLoad();
            On.Session.Update -= Session_Update;
        }

        private void Session_Update(On.Session.orig_Update orig, Session self)
        {
            // Continue attempting to add the ItemHero until it has been added!
            try
            {
                SimulationDescriptor desc;
                Databases.GetDatabase<SimulationDescriptor>(false).TryGetValue(GetBaseDescriptor().Name, out desc);
                if (desc != null)
                {
                    orig(self);
                    return;
                }
                ItemHeroConfig itemHeroConfig = GetItemHeroConfig();
                Databases.GetDatabase<ItemConfig>(false).Add(itemHeroConfig);
                Log("Added the item (ItemHeroConfig) to the database!");
                Log("Attempting to make sim descriptors");
                SimulationDescriptor descriptor = GetBaseDescriptor();
                Log("Successfully retrieved the overall descriptor!");
                SimulationDescriptor common = GetCommonDescriptor();
                Log("Successfully retrieved the common descriptor!");
                SimulationDescriptor rarity0 = GetRarity0Descriptor();
                Log("Successfully retrieved the rarity0 descriptor!");
                SimulationDescriptor rarity1 = GetRarity1Descriptor();
                Log("Successfully retrieved the rarity1 descriptor!");
                SimulationDescriptor rarity2 = GetRarity2Descriptor();
                Log("Successfully retrieved the rarity2 descriptor!");

                Log("Attempting to add descriptors to database!");

                Databases.GetDatabase<SimulationDescriptor>(false).Add(descriptor);
                Log("Added Base!");
                Databases.GetDatabase<SimulationDescriptor>(false).Add(common);
                Log("Added Common!");
                Databases.GetDatabase<SimulationDescriptor>(false).Add(rarity0);
                Log("Added Rarity0!");
                Databases.GetDatabase<SimulationDescriptor>(false).Add(rarity1);
                Log("Added Rarity1!");
                Databases.GetDatabase<SimulationDescriptor>(false).Add(rarity2);
                Log("Added Rarity2!");
                Log("Added all SimDescriptors to the database!");
            }
            catch (ArgumentException e)
            {
                // It already exists!
            } catch (NullReferenceException e)
            {
                // Database doesn't exist yet!
            }
            orig(self);
        }
        
        public abstract SimulationDescriptor GetBaseDescriptor();
        public abstract SimulationDescriptor GetCommonDescriptor();
        public abstract SimulationDescriptor GetRarity0Descriptor();
        public abstract SimulationDescriptor GetRarity1Descriptor();
        public abstract SimulationDescriptor GetRarity2Descriptor();

        public ItemHeroConfig GetItemHeroConfig()
        {
            ItemHeroConfig config = new ItemHeroConfig();
            DynData<ItemHeroConfig> d = new DynData<ItemHeroConfig>(config);
            d.Set<Amplitude.StaticString>("Name", GetName());
            d.Set<int>("MaxCount", GetMaxCount());
            d.Set<Amplitude.StaticString>("AttackTypeConfigName", GetAttackType());
            d.Set("CannotBeUnequipped", GetCannotBeUnequipped());
            d.Set("Skills", GetSkills());
            d.Set("FlatSprite", GetFlatSprite());
            d.Set("DestroyOnDeath", GetDestroyOnDeath());
            d.Set("MaxOccurenceCount", GetMaxOccurenceCount());

            d.Set("CategoryParameters", GetCategoryParameters());
            d.Set("RarityParameters", GetRarityConfigs());
            d.Set("DepthRanges", GetDepthRangeConfigs());
            return config;
        }
        public ItemHeroConfig.ItemHeroCategoryParameters GetCategoryParameters()
        {
            ItemHeroConfig.ItemHeroCategoryParameters config = new ItemHeroConfig.ItemHeroCategoryParameters();
            DynData<ItemHeroConfig.ItemHeroCategoryParameters> d = new DynData<ItemHeroConfig.ItemHeroCategoryParameters>(config);

            d.Set<Amplitude.StaticString>("CategoryName", GetCategoryName());
            d.Set<Amplitude.StaticString>("TypeName", GetCategoryTypeName());
            return config;
        }
        public RarityConfig[] GetRarityConfigs()
        {
            RarityConfig[] configs = new RarityConfig[] { GetCommonConfig(), GetRarity0Config(), GetRarity1Config(), GetRarity2Config() };
            return configs;
        }
        public RarityConfig GetConfig(string name, Dictionary<string, int> dictOfData)
        {
            RarityConfig config = new RarityConfig();
            ProbabilityWeightDepthRangeConfig prob = new ProbabilityWeightDepthRangeConfig();
            EvolutiveValueConfig prob2 = new EvolutiveValueConfig();

            DynData<RarityConfig> d = new DynData<RarityConfig>(config);
            DynData<ProbabilityWeightDepthRangeConfig> d2 = new DynData<ProbabilityWeightDepthRangeConfig>(prob);
            DynData<EvolutiveValueConfig> d3 = new DynData<EvolutiveValueConfig>(prob2);
            config.XmlSerializableName = name;
            d2.Set("LevelMin", dictOfData["LevelMin"]);
            d2.Set("LevelMax", dictOfData["LevelMax"]);
            d2.Set("Start", dictOfData["Start"]);
            d3.Set("StartValue", (float)dictOfData["StartValue"]);
            d3.Set("DepthBonus", (float)dictOfData["DepthBonus"]);
            d3.Set("MaxValue", (float)dictOfData["MaxValue"]);
            d2.Set("ProbabilityWeight", prob2);
            d.Set("DepthRanges", new ProbabilityWeightDepthRangeConfig[] { prob });
            return config;
        }
        public RarityConfig GetCommonConfig()
        {
            return GetConfig("Common", GetCommonRarity());
        }
        public RarityConfig GetRarity0Config()
        {
            return GetConfig("Rarity0", GetRarity0Rarity());
        }
        public RarityConfig GetRarity1Config()
        {
            return GetConfig("Rarity1", GetRarity1Rarity());
        }
        public RarityConfig GetRarity2Config()
        {
            return GetConfig("Rarity2", GetRarity2Rarity());
        }
        public ProbabilityWeightDepthRangeConfig[] GetDepthRangeConfigs()
        {
            ProbabilityWeightDepthRangeConfig config = new ProbabilityWeightDepthRangeConfig();
            EvolutiveValueConfig prob = new EvolutiveValueConfig();

            DynData<ProbabilityWeightDepthRangeConfig> d = new DynData<ProbabilityWeightDepthRangeConfig>(config);
            DynData<EvolutiveValueConfig> d2 = new DynData<EvolutiveValueConfig>(prob);

            d.Set("LevelMin", GetMinLevel());
            d.Set("LevelMax", GetMaxLevel());
            d.Set("Start", 0);
            d2.Set("StartValue", GetStartingProbabilityWeight());
            d2.Set("DepthBonus", (float)0);
            d2.Set("MaxValue", (float)-1);
            d.Set("ProbabilityWeight", prob);

            return new ProbabilityWeightDepthRangeConfig[] { config };
        }
        public void CreateLocalizationChanges()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            foreach (string s in lines)
            {
                if (s.IndexOf("%Item_" + GetName()) != -1)
                {
                    return;
                }
            }
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf("%Item_Special054_Description") != -1)
                {
                    linesLst.Insert(i + 1, "  <LocalizationPair Name=\"%Item_" + GetName() + "\">" + GetRealName() + "</LocalizationPair>");
                    linesLst.Insert(i + 2, "  <LocalizationPair Name=\"%Item_" + GetName() + "_Description\">" + GetRealDescription() + "</LocalizationPair>");
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
                string s = linesLst[i];
                if (s.IndexOf("%Item_" + GetName()) != -1)
                {
                    linesLst.RemoveAt(i);
                }
            }
            System.IO.File.WriteAllLines(@"Public\Localization\english\ED_Localization_Locales.xml", linesLst.ToArray());
        }
        public void CreateGUIChanges()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Gui\GuiElements_ItemHero.xml");
            foreach (string s in lines)
            {
                if (s.IndexOf("%Item_" + GetName()) != -1)
                {
                    return;
                }
            }
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf("%Item_Special054_Description") != -1)
                {
                    linesLst.Insert(i + 5, "  <GuiElement Name=\"" + GetName() + "\">");
                    linesLst.Insert(i + 6, "    <Title>%Item_" + GetName() + "</Title>");
                    linesLst.Insert(i + 7, "    <Description>%Item_" + GetName() + "_Description</Description>");
                    linesLst.Insert(i + 8, "    <Icons>");
                    linesLst.Insert(i + 9, "      <Icon Size=\"Small\" Path=\"GUI/DynamicBitmaps/Items/" + GetSpriteName() + "\"/>");
                    linesLst.Insert(i +10, "    </Icons>");
                    linesLst.Insert(i +11, "  </GuiElement>");
                }
            }
            System.IO.File.WriteAllLines(@"Public\Gui\GuiElements_ItemHero.xml", linesLst.ToArray());
        }
        public void RemoveGUIChanges()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Gui\GuiElements_ItemHero.xml");
            foreach (string s in lines)
            {
                if (s.IndexOf("%Item_" + GetName()) != -1)
                {
                    return;
                }
            }
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < lines.Length; i++)
            {
                string s = linesLst[i];
                if (s.IndexOf("  <GuiElement Name=\"" + GetName() + "\">") != -1)
                {
                    int q = i;
                    for (int j = i; j < linesLst.Count; j++)
                    {
                        if (linesLst[j].IndexOf("  </GuiElement>") != -1)
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
            System.IO.File.WriteAllLines(@"Public\Gui\GuiElements_ItemHero.xml", linesLst.ToArray());
        }
    }
}
