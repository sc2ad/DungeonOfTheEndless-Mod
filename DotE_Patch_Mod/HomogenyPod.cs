using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Gui;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotE_Patch_Mod
{
    class HomogenyPod : PartialityMod
    {
        ScadMod mod = new ScadMod();

        private int CurrentFloor = -1;
        private List<SelectedMob> CurrentMobs = new List<SelectedMob>();

        public override void Init()
        {
            mod.path = @"HomogenyPod_log.txt";
            mod.config = @"HomogenyPod_config.txt";
            mod.default_config = "# Modify this file to change various settings of the HomogenyPod Mod for DotE.\n" + mod.default_config;
            mod.Initalize();

            // Setup default values for config
            mod.Values.Add("Path to HomogenyPodConfig.xml", "HomogenyPodConfig.xml");
            mod.Values.Add("AssumedPod", "Armory");
            mod.Values.Add("DungeonPod", "Pod");

            mod.ReadConfig();
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                mod.Log("Attempting to create Localization changes!");
                HomogenyPodUtil.CheckLocalization();
                //mod.Log("Attempting to copy sounds!");
                //HomogenyPodUtil.CopySounds();
            }
            mod.Log("Initialized!");
        }
        private void CheckPodConfig()
        {
            if (!System.IO.File.Exists(mod.Values["Path to HomogenyPodConfig.xml"]))
            {
                string str = HomogenyPodUtil.GetDefaultPodXml();
                System.IO.File.WriteAllText(mod.Values["Path to HomogenyPodConfig.xml"], str);
            }
        }
        public override void OnLoad()
        {
            mod.Load();
            
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                CheckPodConfig();
                On.GameSelectionPanel.Display += GameSelectionPanel_Display;

                On.GameSelectionPanel.GetShipAnimation += GameSelectionPanel_GetShipAnimation;
                On.GameSelectionPanel.LaunchSelectedShipInAnimation += GameSelectionPanel_LaunchSelectedShipInAnimation;
                On.GameSelectionPanel.LaunchSelectedShipOutAnimation += GameSelectionPanel_LaunchSelectedShipOutAnimation;
                On.GameSelectionPanel.LaunchSelectedShipConfirmAnimation += GameSelectionPanel_LaunchSelectedShipConfirmAnimation;

                On.GameSelectionPanel.UpdateShip += GameSelectionPanel_UpdateShip;
                On.DungeonGenerator2.GenerateDungeonUsingSeedCoroutine += DungeonGenerator2_GenerateDungeonUsingSeedCoroutine;

                On.Dungeon.TriggerEvents += Dungeon_TriggerEvents;
                On.Dungeon.SpawnMobs += Dungeon_SpawnMobs;
            } else
            {
                mod.Log("Attempting to remove Localization changes!");
                HomogenyPodUtil.RemoveLocalization();
                //mod.Log("Attempting to remove sounds!");
                //HomogenyPodUtil.RemoveSounds();
            }
        }

        private System.Collections.IEnumerator Dungeon_SpawnMobs(On.Dungeon.orig_SpawnMobs orig, Dungeon self, Room spawnRoom, float roomDifficultyValue, MobSpawnType spawnType, StaticString eventType, List<SelectedMob> elligibleMobs, Action<int> spawnCountSetter)
        {
            if (self.ShipName == "HomogenyPod")
            {
                if (CurrentFloor != self.Level && spawnRoom.OpeningIndex > 1)
                {
                    List<SelectedMob> mobs = new List<SelectedMob>();
                    for (int i = 0; i < elligibleMobs.Count; i++)
                    {
                        MobClassConfig config = Databases.GetDatabase<MobClassConfig>(false).GetValue(elligibleMobs[i].MobCfg.Name);
                        if (config == null)
                        {
                            mod.Log("Config is a null");
                        }
                        int index = (!elligibleMobs[i].IsNew) ? config.MinRoomOpeningIndex : config.MinRoomOpeningIndexIfNew;
                        if (spawnRoom.OpeningIndex > index)
                        {
                            mobs.Add(elligibleMobs[i]);
                        }
                    }
                    mod.Log("mobs.Count check!");
                    if (mobs.Count < 1)
                    {
                        return orig(self, spawnRoom, roomDifficultyValue, spawnType, eventType, elligibleMobs, spawnCountSetter);
                    }

                    SelectedMob s = mobs.GetWeightedRandom((SelectedMob m) => m.MobCfg.SpawnProbWeight.GetValue(spawnRoom));

                    mod.Log("Logging s: "+s);

                    CurrentMobs = new List<SelectedMob>();
                    CurrentMobs.Add(s);
                    mod.Log("Currently Selected mobs for floor: " + self.Level);
                    foreach (SelectedMob q in CurrentMobs)
                    {
                        mod.Log(q.MobCfg.Name);
                    }
                    mod.Log("Out of:");
                    foreach (SelectedMob q in mobs)
                    {
                        mod.Log(q.MobCfg.Name);
                    }
                    CurrentFloor = self.Level;
                }
                if (CurrentMobs.Count > 0)
                {
                    mod.Log("Attempting to spawn: " + CurrentMobs[0].MobCfg.Name);
                    return orig(self, spawnRoom, roomDifficultyValue, spawnType, eventType, CurrentMobs, spawnCountSetter);
                }
            }
            return orig(self, spawnRoom, roomDifficultyValue, spawnType, eventType, elligibleMobs, spawnCountSetter);
        }

        private System.Collections.IEnumerator Dungeon_TriggerEvents(On.Dungeon.orig_TriggerEvents orig, Dungeon self, Room openingRoom, HeroMobCommon opener, bool canTriggerDungeonEvent)
        {
            // Only select first mob from door that can trigger events
            // Also only choose once openingRoom.OpeningIndex > 1
            return orig(self, openingRoom, opener, canTriggerDungeonEvent);
        }

        private System.Collections.IEnumerator DungeonGenerator2_GenerateDungeonUsingSeedCoroutine(On.DungeonGenerator2.orig_GenerateDungeonUsingSeedCoroutine orig, DungeonGenerator2 self, int level, int randomSeed, Amplitude.StaticString shipName)
        {
            if (shipName == "HomogenyPod")
            {
                DynData<DungeonGenerator2> d = new DynData<DungeonGenerator2>(self);
                d.Set<int>("randomSeed", randomSeed);
                mod.Log("Calling GenerateDungeonUsingSeedCoroutine!");
                
                yield return typeof(DungeonGenerator2).GetMethod("GenerateDungeonCoroutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(self, new object[] { level, new Amplitude.StaticString(mod.Values["DungeonPod"]) });

                yield break;
            }
            mod.Log("Calling Original GenerateDungeonUsingSeedCoroutine!");
            yield return orig(self, level, randomSeed, shipName);
        }

        private void GameSelectionPanel_UpdateShip(On.GameSelectionPanel.orig_UpdateShip orig, GameSelectionPanel self, Amplitude.StaticString shipName, bool nextOrPrevious, bool syncOverNetwork, bool refreshContent)
        {
            if (shipName == "HomogenyPod")
            {
                orig(self, shipName, nextOrPrevious, syncOverNetwork, refreshContent);
                DynData<GameSelectionPanel> d = new DynData<GameSelectionPanel>(self);
                GameNetworkManager manager = d.Get<GameNetworkManager>("gameNetManager");
                Dungeon.SetShip("HomogenyPod");
                if (syncOverNetwork && manager.IsServer())
                {
                    global::Session session = manager.GetSession();
                    session.SetLobbyData(global::Session.LOBBYDATA_GAME_SHIP, "HomogenyPod");
                    // mod.Values["AssumedPod"]
                }
                return;
            }
            orig(self, shipName, nextOrPrevious, syncOverNetwork, refreshContent);
        }

        private void GameSelectionPanel_LaunchSelectedShipConfirmAnimation(On.GameSelectionPanel.orig_LaunchSelectedShipConfirmAnimation orig, GameSelectionPanel self)
        {
            DynData<GameSelectionPanel> d = new DynData<GameSelectionPanel>(self);
            Amplitude.StaticString temp = d.Get<Amplitude.StaticString>("selectedShipName");
            if (temp == "HomogenyPod")
            {
                d.Set<Amplitude.StaticString>("selectedShipName", mod.Values["AssumedPod"]);
            }
            orig(self);
            d.Set<Amplitude.StaticString>("selectedShipName", temp);
        }

        private SpriteAnimationRuntime2 GameSelectionPanel_GetShipAnimation(On.GameSelectionPanel.orig_GetShipAnimation orig, GameSelectionPanel self, Amplitude.StaticString shipName)
        {
            mod.Log("Getting an animation for: " + shipName);
            if (shipName == "HomogenyPod")
            {
                return orig(self, mod.Values["AssumedPod"]);
            }
            return orig(self, shipName);
        }

        private void GameSelectionPanel_LaunchSelectedShipOutAnimation(On.GameSelectionPanel.orig_LaunchSelectedShipOutAnimation orig, GameSelectionPanel self, bool nextOrPrevious, bool playOutSFX)
        {
            DynData<GameSelectionPanel> d = new DynData<GameSelectionPanel>(self);
            Amplitude.StaticString temp = d.Get<Amplitude.StaticString>("selectedShipName");
            if (temp == "HomogenyPod")
            {
                d.Set<Amplitude.StaticString>("selectedShipName", mod.Values["AssumedPod"]);
            }
            orig(self, nextOrPrevious, playOutSFX);
            d.Set<Amplitude.StaticString>("selectedShipName", temp);
        }

        // This uses default animation for Pod
        private void GameSelectionPanel_LaunchSelectedShipInAnimation(On.GameSelectionPanel.orig_LaunchSelectedShipInAnimation orig, GameSelectionPanel self)
        {
            DynData<GameSelectionPanel> d = new DynData<GameSelectionPanel>(self);
            Amplitude.StaticString temp = d.Get<Amplitude.StaticString>("selectedShipName");
            if (temp == "HomogenyPod")
            {
                d.Set<Amplitude.StaticString>("selectedShipName", mod.Values["AssumedPod"]);
            }
            orig(self);
            d.Set<Amplitude.StaticString>("selectedShipName", temp);
        }

        private void GameSelectionPanel_Display(On.GameSelectionPanel.orig_Display orig, GameSelectionPanel self, bool isMultiplayer, string mpSaveKey, int slotCount, GameSelectionPanel.GameSelectionFinishedHandler onGameSelectionFinished)
        {
            mod.Log("Attempting to Display!");
            IDatabase<ShipConfig> db = Databases.GetDatabase<ShipConfig>(false);
            mod.Log("Database loaded!");
            if (db.GetValue(HomogenyPodUtil.GetConfig().Name) != null)
            {
                // It already exists within the database, so we can continue
                mod.Log("The shipconfig with name: " + HomogenyPodUtil.GetConfig().Name + " already exists inside the database!");
            }
            else
            {
                db.Add(HomogenyPodUtil.GetConfig());
            }
            DynData<GameSelectionPanel> paneldyn = new DynData<GameSelectionPanel>(self);

            paneldyn.Set<IDatabase<ShipConfig>>("shipConfigDB", db);
            mod.Log("Database set!");

            orig(self, isMultiplayer, mpSaveKey, slotCount, onGameSelectionFinished);

            mod.Log("Available Ships:");
            foreach (string s in paneldyn.Get<List<Amplitude.StaticString>>("availableShips"))
            {
                mod.Log(s);
            }
            mod.Log("DB:");
            foreach (ShipConfig s in paneldyn.Get<IDatabase<ShipConfig>>("shipConfigDB"))
            {
                mod.Log("Name: " + s.Name + " localized: " + s.GetLocalizedName() + " desc: " + s.GetLocalizedDescription() + " abscissa val: " + s.AbscissaValue);
            }

            // Need to add a thing to the animation list
        }
    }
}
