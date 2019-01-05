using Amplitude.Unity.Framework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DustDevilFramework
{
    public abstract class PodMod : ScadMod
    {
        public abstract string GetName();
        public abstract string GetDescription();
        public abstract string GetSpecialText();
        public abstract string GetAnimationPod();
        public abstract string GetDungeonPod();

        public abstract int GetIndex();
        public abstract int GetLevelCount();
        public abstract string[] GetInitialBlueprints();

        public abstract string[] GetUnavailableBlueprints();
        public abstract string[] GetUnavailableItems();

        public PodMod(Type settingsType)
        {
            name = GetName();
            PodSettings temp = (PodSettings)settingsType.TypeInitializer.Invoke(new object[] { name });
            this.settingsType = settingsType;
            settings = temp;
            // Setup default values for config
            if (!settings.Exists())
            {
                temp.AssumedPod = GetAnimationPod();
                temp.DungeonPod = GetDungeonPod();
            }
        }

        public void Initialize()
        {
            path = GetName() + "_log.txt";
            base.Initialize();

            settings.ReadSettings();
            if (settings.Enabled)
            {
                Log("Attempting to create Localization changes!");
                List<string> linesLst = new List<string>();
                linesLst.Add("  <LocalizationPair Name=\"%ShipTitle_" + GetName() + "\">" + GetName() + "</LocalizationPair>");
                linesLst.Add("  <LocalizationPair Name=\"%ShipDescription_" + GetName() + "\">" + GetDescription());
                linesLst.Add("");
                linesLst.Add(GetSpecialText() + "</LocalizationPair>");
                Util.ApplyLocalizationChange("%ShipTitle_Locked_Infirmary", 0, linesLst.ToArray());
            } else
            {
                Log("Attempting to remove Localization changes!");
                Util.RemoveLocalizationChangeInclusive("%ShipTitle_" + GetName(), "</LocalizationPair>");
                //RemoveLocalizationChanges();
            }
            Log("Initialized!");
        }

        public void Load()
        {
            base.Load();

            if (settings.Enabled)
            {
                On.GameSelectionPanel.Display += GameSelectionPanel_Display;

                On.GameSelectionPanel.GetShipAnimation += GameSelectionPanel_GetShipAnimation;
                On.GameSelectionPanel.LaunchSelectedShipInAnimation += GameSelectionPanel_LaunchSelectedShipInAnimation;
                On.GameSelectionPanel.LaunchSelectedShipOutAnimation += GameSelectionPanel_LaunchSelectedShipOutAnimation;
                On.GameSelectionPanel.LaunchSelectedShipConfirmAnimation += GameSelectionPanel_LaunchSelectedShipConfirmAnimation;

                On.GameSelectionPanel.UpdateShip += GameSelectionPanel_UpdateShip;
                On.DungeonGenerator2.GenerateDungeonUsingSeedCoroutine += DungeonGenerator2_GenerateDungeonUsingSeedCoroutine;
            }
        }

        public void UnLoad()
        {
            base.UnLoad();

            On.GameSelectionPanel.Display -= GameSelectionPanel_Display;

            On.GameSelectionPanel.GetShipAnimation -= GameSelectionPanel_GetShipAnimation;
            On.GameSelectionPanel.LaunchSelectedShipInAnimation -= GameSelectionPanel_LaunchSelectedShipInAnimation;
            On.GameSelectionPanel.LaunchSelectedShipOutAnimation -= GameSelectionPanel_LaunchSelectedShipOutAnimation;
            On.GameSelectionPanel.LaunchSelectedShipConfirmAnimation -= GameSelectionPanel_LaunchSelectedShipConfirmAnimation;

            On.GameSelectionPanel.UpdateShip -= GameSelectionPanel_UpdateShip;
            On.DungeonGenerator2.GenerateDungeonUsingSeedCoroutine -= DungeonGenerator2_GenerateDungeonUsingSeedCoroutine;
        }

        private System.Collections.IEnumerator DungeonGenerator2_GenerateDungeonUsingSeedCoroutine(On.DungeonGenerator2.orig_GenerateDungeonUsingSeedCoroutine orig, DungeonGenerator2 self, int level, int randomSeed, Amplitude.StaticString shipName)
        {
            if (shipName == GetName())
            {
                DynData<DungeonGenerator2> d = new DynData<DungeonGenerator2>(self);
                d.Set<int>("randomSeed", randomSeed);
                Log("Calling GenerateDungeonUsingSeedCoroutine!");

                yield return typeof(DungeonGenerator2).GetMethod("GenerateDungeonCoroutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(self, new object[] { level, new Amplitude.StaticString(((PodSettings)settings).DungeonPod) });

                yield break;
            }
            Log("Calling Original GenerateDungeonUsingSeedCoroutine!");
            yield return orig(self, level, randomSeed, shipName);
        }

        private void GameSelectionPanel_UpdateShip(On.GameSelectionPanel.orig_UpdateShip orig, GameSelectionPanel self, Amplitude.StaticString shipName, bool nextOrPrevious, bool syncOverNetwork, bool refreshContent)
        {
            if (shipName == GetName())
            {
                orig(self, shipName, nextOrPrevious, syncOverNetwork, refreshContent);
                DynData<GameSelectionPanel> d = new DynData<GameSelectionPanel>(self);
                GameNetworkManager manager = d.Get<GameNetworkManager>("gameNetManager");
                Dungeon.SetShip(GetName());
                if (syncOverNetwork && manager.IsServer())
                {
                    global::Session session = manager.GetSession();
                    session.SetLobbyData(global::Session.LOBBYDATA_GAME_SHIP, GetName());
                    // Values["AssumedPod"]
                }
                return;
            }
            orig(self, shipName, nextOrPrevious, syncOverNetwork, refreshContent);
        }

        private void GameSelectionPanel_LaunchSelectedShipConfirmAnimation(On.GameSelectionPanel.orig_LaunchSelectedShipConfirmAnimation orig, GameSelectionPanel self)
        {
            DynData<GameSelectionPanel> d = new DynData<GameSelectionPanel>(self);
            Amplitude.StaticString temp = d.Get<Amplitude.StaticString>("selectedShipName");
            if (temp == GetName())
            {
                d.Set<Amplitude.StaticString>("selectedShipName", ((PodSettings)settings).AssumedPod);
            }
            orig(self);
            d.Set<Amplitude.StaticString>("selectedShipName", temp);
        }

        private SpriteAnimationRuntime2 GameSelectionPanel_GetShipAnimation(On.GameSelectionPanel.orig_GetShipAnimation orig, GameSelectionPanel self, Amplitude.StaticString shipName)
        {
            Log("Getting an animation for: " + shipName);
            if (shipName == GetName())
            {
                return orig(self, ((PodSettings)settings).AssumedPod);
            }
            return orig(self, shipName);
        }

        private void GameSelectionPanel_LaunchSelectedShipOutAnimation(On.GameSelectionPanel.orig_LaunchSelectedShipOutAnimation orig, GameSelectionPanel self, bool nextOrPrevious, bool playOutSFX)
        {
            DynData<GameSelectionPanel> d = new DynData<GameSelectionPanel>(self);
            Amplitude.StaticString temp = d.Get<Amplitude.StaticString>("selectedShipName");
            if (temp == GetName())
            {
                d.Set<Amplitude.StaticString>("selectedShipName", ((PodSettings)settings).AssumedPod);
            }
            orig(self, nextOrPrevious, playOutSFX);
            d.Set<Amplitude.StaticString>("selectedShipName", temp);
        }

        // This uses default animation for Pod
        private void GameSelectionPanel_LaunchSelectedShipInAnimation(On.GameSelectionPanel.orig_LaunchSelectedShipInAnimation orig, GameSelectionPanel self)
        {
            DynData<GameSelectionPanel> d = new DynData<GameSelectionPanel>(self);
            Amplitude.StaticString temp = d.Get<Amplitude.StaticString>("selectedShipName");
            if (temp == GetName())
            {
                d.Set<Amplitude.StaticString>("selectedShipName", ((PodSettings)settings).AssumedPod);
            }
            orig(self);
            d.Set<Amplitude.StaticString>("selectedShipName", temp);
        }

        private void GameSelectionPanel_Display(On.GameSelectionPanel.orig_Display orig, GameSelectionPanel self, bool isMultiplayer, string mpSaveKey, int slotCount, GameSelectionPanel.GameSelectionFinishedHandler onGameSelectionFinished)
        {
            Log("Attempting to Display!");
            IDatabase<ShipConfig> db = Databases.GetDatabase<ShipConfig>(false);
            Log("Database loaded!");
            if (db.GetValue(this.GetConfig().Name) != null)
            {
                // It already exists within the database, so we can continue
                Log("The shipconfig with name: " + this.GetConfig().Name + " already exists inside the database!");
            }
            else
            {
                db.Add(this.GetConfig());
            }
            DynData<GameSelectionPanel> paneldyn = new DynData<GameSelectionPanel>(self);

            paneldyn.Set<IDatabase<ShipConfig>>("shipConfigDB", db);
            Log("Database set!");

            orig(self, isMultiplayer, mpSaveKey, slotCount, onGameSelectionFinished);

            Log("Available Ships:");
            foreach (string s in paneldyn.Get<List<Amplitude.StaticString>>("availableShips"))
            {
                Log(s);
            }
            Log("DB:");
            foreach (ShipConfig s in paneldyn.Get<IDatabase<ShipConfig>>("shipConfigDB"))
            {
                Log("Name: " + s.Name + " localized: " + s.GetLocalizedName() + " desc: " + s.GetLocalizedDescription() + " abscissa val: " + s.AbscissaValue);
            }
        }

        public void CreateLocalizationChanges()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            foreach (string s in lines)
            {
                if (s.IndexOf("%ShipTitle_" + GetName()) != -1)
                {
                    // The Localization already contains the Key for the pod.
                    return;
                }
            }
            List<string> linesLst = lines.ToList();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                // Inserts it right before infirmary, right after escape pod.
                if (line.IndexOf("%ShipTitle_Locked_Infirmary") != -1)
                {
                    linesLst.Insert(i, "  <LocalizationPair Name=\"%ShipTitle_"+GetName()+"\">"+GetName()+"</LocalizationPair>");
                    linesLst.Insert(i + 1, "  <LocalizationPair Name=\"%ShipDescription_"+GetName()+"\">"+GetDescription());
                    linesLst.Insert(i + 2, "");
                    linesLst.Insert(i + 3, GetSpecialText() + "</LocalizationPair>");
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
                if (s.IndexOf("%ShipTitle_" + GetName()) != -1)
                {
                    // The Localization already contains the Key for the pod.
                    linesLst.RemoveAt(i);
                }
                if (s.IndexOf("%ShipDescription_" + GetName()) != -1)
                {
                    // The Localization already contains the Key for the pod.
                    int q = i;
                    for (int j = i; j < linesLst.Count; j++)
                    {
                        if (linesLst[j].IndexOf("</LocalizationPair>") != -1)
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

        public ShipConfig GetConfig()
        {
            ShipConfig s = new ShipConfig();

            DynData<ShipConfig> shdyn = new DynData<ShipConfig>(s);
            shdyn.Set<Amplitude.StaticString>("Name", GetName());
            shdyn.Set<int>("AbscissaValue", GetIndex());
            shdyn.Set<int>("LevelCount", GetLevelCount());
            string[] bps = GetInitialBlueprints();
            shdyn.Set<string[]>("InitBluePrints", bps);
            string[] ubps = GetUnavailableBlueprints();
            shdyn.Set<string[]>("UnavailableBluePrints", ubps);
            string[] unItems = GetUnavailableItems();
            shdyn.Set<string[]>("UnavailableItems", unItems);
            shdyn.Set<ShipConfig.ItemDatatableReference[]>("InitialItems", new ShipConfig.ItemDatatableReference[] { });


            return s;
        }

        // Basically, unlocks need to have achievements linked to them.
        // The Achievements should be customizable, including whether or not they have a statistic associated with them
        public ShipUnlockData GetUnlockData()
        {
            ShipUnlockData unlockData = default(ShipUnlockData);
            unlockData.ShipName = GetName();
            // Convert the name to an AchievementName, also need to add the name as a valid AchievementName beforehand
            //unlockData.UnlockingAchievement = new AchievementName[] { GetLocalAchievement().Name }.ToList();
            return unlockData;
        }

        public LocalAchievementDefinition GetLocalAchievementDefinition()
        {
            LocalAchievementDefinition def = new LocalAchievementDefinition();
            return def;
        }

        public LocalAchievement GetLocalAchievement()
        {
            LocalAchievement ach = new LocalAchievement(GetLocalAchievementDefinition());
            return ach;
        }
    }
}
