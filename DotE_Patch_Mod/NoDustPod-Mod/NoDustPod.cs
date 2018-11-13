﻿using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Gui;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoDustPod_Mod
{
    class NoDustPod : PartialityMod
    {
        NoDustPodConfig mod = new NoDustPodConfig();

        public override void Init()
        {
            mod.Initialize();
        }
        public override void OnLoad()
        {
            mod.Load();

            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                On.Dungeon.DoAddDust += Dungeon_DoAddDust;
            }
        }

        private void Dungeon_DoAddDust(On.Dungeon.orig_DoAddDust orig, Dungeon self, float dustAmount, bool displayFeedback, bool triggerDungeonFIDSChangedEvent)
        {
            if (self.ShipName == mod.GetName())
            {
                orig(self, dustAmount, displayFeedback, triggerDungeonFIDSChangedEvent);
                if (self.DustStock > 9)
                {
                    mod.Log("Capping dust at 9!");
                    new DynData<Dungeon>(self).Set<float>("DustStock", 9);
                }
                return;
            }
            orig(self, dustAmount, displayFeedback, triggerDungeonFIDSChangedEvent);
        }
    }
}