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
using UnityEngine;

namespace DuplicateHeroSelection_Mod
{
    public class DuplicateHeroSelectionMod : PartialityMod
    {
        ScadMod mod = new ScadMod("DuplicateHeroSelection", typeof(DuplicateHeroSelectionSettings), typeof(DuplicateHeroSelectionMod));
        public override void Init()
        {
            mod.PartialityModReference = this;
            mod.Initialize();

            mod.settings.ReadSettings();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (mod.settings.Enabled)
            {
                On.HeroSelectionItem.OnLeftClick += HeroSelectionItem_OnLeftClick;
            }
        }

        public void UnLoad()
        {
            // Need to have a way of unloading entirety of the mod from only ScadMod (also deals with unloading partialitymod)
            mod.UnLoad();
            On.HeroSelectionItem.OnLeftClick -= HeroSelectionItem_OnLeftClick;
        }

        private void HeroSelectionItem_OnLeftClick(On.HeroSelectionItem.orig_OnLeftClick orig, HeroSelectionItem self)
        {
            self.GetComponent<AgeControlToggle>().OnRightClickMethod = "OnRightClick";
            try
            {
                self.gameObject.AddComponent<RightClickScript>();
            }
            catch (Exception e)
            {
                mod.Log("Already added RightClickScript!");
            }
            // Even if the hero has been selected, try to select it again.
            int duplicates = 0;
            foreach (Amplitude.StaticString s in Dungeon.NextDungeonGenerationParams.NewGame_SelectedHeroes)
            {
                if (s.Equals(self.HeroStats.ConfigName))
                {
                    duplicates++;
                }
            }
            if (duplicates < (mod.settings as DuplicateHeroSelectionSettings).AllowedDuplicateCount)
            {
                // Try selecting it again!
                mod.Log("Selecting Hero: " + self.HeroStats.ConfigName + " again!");
                IAudioEventService service = Services.GetService<IAudioEventService>();
                SingletonManager.Get<GameSelectionPanel>(true).TrySelectHero(self.HeroStats.ConfigName);
                service.Play2DEvent("Master/GUI/Main_SelectHero");
                return;
            }
            
            orig(self);
        }

        private class RightClickScript : MonoBehaviour
        {
            public void OnRightClick()
            {
                SingletonManager.Get<GameSelectionPanel>(true).TryUnselectHero(GetComponent<HeroSelectionItem>().HeroStats.ConfigName);
            }
        }
    }
}
