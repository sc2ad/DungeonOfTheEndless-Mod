using BepInEx;
using BepInEx.Configuration;
using DustDevilFramework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DungeonModifications_Mod
{
    [BepInPlugin("com.sc2ad.DungeonModificationsMod", "Dungeon Modifications", "1.0.0")]
    public class DungeonModificationsMod : BaseUnityPlugin
    {
        private ScadMod mod;

        private ConfigWrapper<bool> capRoomsWrapper;
        private ConfigWrapper<int> maxRoomWrapper;

        public void Awake()
        {
            mod = new ScadMod("Dungeon Modifications", this);

            // Wrap Settings here!
            capRoomsWrapper = Config.Wrap("Settings", "CapRooms", "Whether to cap the number of rooms provided by the config value: MaxRooms or not.", true);
            maxRoomWrapper = Config.Wrap("Settings", "MaxRooms", "Maximum number of rooms to have in the dungeon.", 4);
            Config.Wrap("SettingsIgnore", "MaxRoomsMin", defaultValue: 4);

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
            {
                On.DungeonGenerator2.GenerateDungeonCoroutine += DungeonGenerator2_GenerateDungeonCoroutine;
                // Add hooks here!
            }
        }

        private System.Collections.IEnumerator DungeonGenerator2_GenerateDungeonCoroutine(On.DungeonGenerator2.orig_GenerateDungeonCoroutine orig, DungeonGenerator2 self, int level, Amplitude.StaticString shipName)
        {
            if (self.GetField<DungeonGenerator2, bool>("IsGeneratingForRuntime"))
            {
                GameConfig c = GameConfig.GetGameConfig();
                if (capRoomsWrapper.Value && (int) c.DungeonRoomCountMax.GetValue() != maxRoomWrapper.Value)
                {
                    mod.Log("Setting DungeonRoomCountMax to: " + maxRoomWrapper.Value);
                    CurveDefinedValue v = c.DungeonRoomCountMax;
                    mod.Log("Original: " + v.GetValue());
                    CurveOperation op = v.CurveOperation;
                    mod.Log("Setting Max for CurveOperation...");
                    typeof(CurveOperation).GetProperty("Max").SetValue(op, (float)maxRoomWrapper.Value, null);
                    //new DynData<CurveOperation>(op).Set<float>("Max", maxRoomWrapper.Value);
                    mod.Log("Setting curveoperation...");
                    mod.Log("CurveOperation Max: " + op.Max);
                    //new DynData<CurveDefinedValue>(v).Set("CurveOperation", op);
                    typeof(CurveDefinedValue).GetProperty("CurveOperation").SetValue(v, op, null);
                    mod.Log("CurveDefinedValue.CurveOperation.Max: " + v.CurveOperation.Max);
                    mod.Log("Setting DungeonRoomCountMax...");
                    //new DynData<GameConfig>(c).Set("DungeonRoomCountMax", v);
                    typeof(GameConfig).GetProperty("DungeonRoomCountMax").SetValue(c, v, null);
                    mod.Log("Final value: " + c.DungeonRoomCountMax.GetValue());

                    // Need to also set min to be the same...
                    mod.Log("Setting DungeonRoomCountMin to: " + maxRoomWrapper.Value);
                    CurveDefinedValue m = c.DungeonRoomCountMin;
                    mod.Log("Original min: " + m.GetValue());
                    CurveOperation op2 = m.CurveOperation;
                    typeof(CurveOperation).GetProperty("Max").SetValue(op2, (float)maxRoomWrapper.Value, null);
                    typeof(CurveDefinedValue).GetProperty("CurveOperation").SetValue(m, op2, null);
                    typeof(GameConfig).GetProperty("DungeonRoomCountMin").SetValue(c, m, null);
                    mod.Log("Final min: " + c.DungeonRoomCountMin.GetValue());
                }
            }
            yield return orig(self, level, shipName);
        }

        public void UnLoad()
        {
            mod.UnLoad();
            On.DungeonGenerator2.GenerateDungeonCoroutine -= DungeonGenerator2_GenerateDungeonCoroutine;
            // Remove hooks here!
        }
    }
}
