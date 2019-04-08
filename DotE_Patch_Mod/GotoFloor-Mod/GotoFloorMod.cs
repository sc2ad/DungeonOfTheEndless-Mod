using Amplitude.Unity.Audio;
using Amplitude.Unity.Framework;
using DustDevilFramework;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotoFloor_Mod
{
    class GotoFloorMod : PartialityMod
    {
        internal ScadMod mod = new ScadMod("GotoFloor", typeof(GotoFloorSettings), typeof(GotoFloorMod));

        /// <summary>
        /// This is the private reference variable to your settings that has the correct type, as oppossed to mod.settings.
        /// This gets set to after mod.settings is read from.
        /// </summary>
        private GotoFloorSettings settings;

        private bool CompletedSkip = false;

        public override void Init()
        {
            mod.PartialityModReference = this;
            mod.Initialize();

            // Versioning
            mod.MajorVersion = 0;
            mod.MinorVersion = 0;
            mod.Revision = 1;

            mod.settings.ReadSettings();

            settings = (mod.settings as GotoFloorSettings);

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                On.Dungeon.PrepareForNewGame += Dungeon_PrepareForNewGame;
                On.Dungeon.PrepareForNextLevel += Dungeon_PrepareForNextLevel;
                On.Dungeon.Update += Dungeon_Update;
                // Add hooks here!
            }
        }

        private void Dungeon_Update(On.Dungeon.orig_Update orig, Dungeon self)
        {
            orig(self);
            List<Hero> heroes = Hero.GetAllPlayersActiveRecruitedHeroes();
            if (!CompletedSkip && self.Level == 1 && settings.LevelTarget != 1 && self.ShipConfig != null && self.RoomCount != 0 && self.StartRoom != null && heroes != null && heroes.Count > 0 && heroes[0] != null && heroes[0].RoomElement != null)
            {
                Room exit = self.StartRoom;

                var method = typeof(Dungeon).GetMethod("SpawnExit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method.Equals(null))
                {
                    mod.Log("SpawnExit method is null!");
                    return;
                }
                if (Services.GetService<IAudioLayeredMusicService>() == null)
                {
                    // Waiting for music!
                    mod.Log("Music does not exist!");
                    return;
                }

                mod.Log("Calling SpawnExit!");

                method.Invoke(self, new object[] { self.StartRoom });

                mod.Log("Attempting to plug exit and end level immediately!");
                self.NotifyCrystalStateChanged(CrystalState.Unplugged);
                new DynData<Dungeon>(self).Set("ExitRoom", exit);

                mod.Log("Setting heroes to contain crystal and be in exit room!");
                new DynData<Hero>(heroes[0]).Set("HasCrystal", true);
                foreach (Hero h in heroes)
                {
                    h.RoomElement.SetParentRoom(exit);
                    h.WasInExitRoomAtExitTime = true;
                }
                // Must be greater than 0!
                float delay = 1f;
                new DynData<Dungeon>(self).Set("vistoryScreenDisplayDelay", delay);
                mod.Log("Attempting to end level with wait delay of: ");
                self.LevelOver(true);
                self.OnCrystalPlugged();
                CompletedSkip = true;
            }
        }

        private void Dungeon_PrepareForNextLevel(On.Dungeon.orig_PrepareForNextLevel orig)
        {
            Dungeon d = SingletonManager.Get<Dungeon>(false);
            if (d.Level == 1)
            {
                mod.Log("Setting Level for next level to: " + (settings.LevelTarget - 1));
                new DynData<Dungeon>(d).Set("Level", settings.LevelTarget - 1);
            }
            orig();
            CompletedSkip = false;
        }

        private void Dungeon_PrepareForNewGame(On.Dungeon.orig_PrepareForNewGame orig, bool multiplayer)
        {
            orig(multiplayer);
            // Get the nextDungeonGenerationParams to modify!
            var field = typeof(Dungeon).GetField("nextDungeonGenerationParams", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            DungeonGenerationParams p = (DungeonGenerationParams)field.GetValue(null);
            p.Level = settings.LevelTarget;
            mod.Log("Set the nextDungeonGenerationParams to level: " + settings.LevelTarget);
            CompletedSkip = false;
        }

        public void UnLoad()
        {
            mod.UnLoad();
            // Remove hooks here!
            On.Dungeon.PrepareForNewGame -= Dungeon_PrepareForNewGame;
            On.Dungeon.PrepareForNextLevel -= Dungeon_PrepareForNextLevel;
            On.Dungeon.Update -= Dungeon_Update;
        }
    }
}
