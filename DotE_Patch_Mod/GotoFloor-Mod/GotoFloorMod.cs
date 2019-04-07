using DustDevilFramework;
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
                // Add hooks here!
            }
        }

        private void Dungeon_PrepareForNewGame(On.Dungeon.orig_PrepareForNewGame orig, bool multiplayer)
        {
            orig(multiplayer);
            // Get the nextDungeonGenerationParams to modify!
            var field = typeof(Dungeon).GetField("nextDungeonGenerationParams", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            DungeonGenerationParams p = (DungeonGenerationParams)field.GetValue(null);
            p.Level = settings.LevelTarget;
            mod.Log("Set the nextDungeonGenerationParams to level: " + settings.LevelTarget);
        }

        public void UnLoad()
        {
            mod.UnLoad();
            // Remove hooks here!
            On.Dungeon.PrepareForNewGame -= Dungeon_PrepareForNewGame;
        }
    }
}
