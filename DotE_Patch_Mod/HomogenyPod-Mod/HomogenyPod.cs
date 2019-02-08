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

namespace HomogenyPod_Mod
{
    public class HomogenyPod : PartialityMod
    {
        HomogenyPodConfig mod = new HomogenyPodConfig(typeof(HomogenyPod));

        private int CurrentFloor = -1;
        private List<SelectedMob> CurrentMobs = new List<SelectedMob>();

        public override void Init()
        {
            mod.PartialityModReference = this;
            mod.Initialize();
        }
        public override void OnLoad()
        {
            mod.Load();

            if (mod.settings.Enabled)
            {
                On.Dungeon.SpawnMobs += Dungeon_SpawnMobs;
            }
        }
        public void UnLoad()
        {
            mod.UnLoad();
            On.Dungeon.SpawnMobs -= Dungeon_SpawnMobs;
        }

        private System.Collections.IEnumerator Dungeon_SpawnMobs(On.Dungeon.orig_SpawnMobs orig, Dungeon self, Room spawnRoom, float roomDifficultyValue, MobSpawnType spawnType, StaticString eventType, List<SelectedMob> elligibleMobs, Action<int> spawnCountSetter)
        {
            if (self.ShipName == mod.GetName())
            {
                if (CurrentFloor != self.Level && spawnRoom.OpeningIndex >= 1)
                {
                    List<SelectedMob> mobs = new List<SelectedMob>();
                    for (int i = 0; i < elligibleMobs.Count; i++)
                    {
                        MobClassConfig config = Databases.GetDatabase<MobClassConfig>(false).GetValue(elligibleMobs[i].MobCfg.Name);

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
    }
}
